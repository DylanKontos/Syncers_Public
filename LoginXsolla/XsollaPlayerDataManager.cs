using UnityEngine;
using System.Collections.Generic;
using Xsolla.Auth;
using Xsolla.Core;
using Xsolla.UserAccount;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class XsollaPlayerDataManager : MonoBehaviour // Persistent accross all scenes.
{
    string apiUrl = "[OMITTED]";
    string apiPublicToken = "[OMITTED];
    private const string GetGeoLocationUrl =  "[OMITTED]";
    // private const string GetGeoLocationUrl = "[OMITTED]";

    private string XsollaUserID;
    public string displayName;
    public string selectedSkin;

    public bool isLoggedIn { get; private set; } = false;

    public PlayerData _playerData;

    private LeaderboardManager leaderboardManager;
    private LoginHandler loginHandler;
    private PopupManager popupManager;

    private void Start()
    {
        InitializePlayerData();
        
        popupManager = FindFirstObjectByType<PopupManager>();
        leaderboardManager = FindFirstObjectByType<LeaderboardManager>();
        loginHandler = FindFirstObjectByType<LoginHandler>();
    }

    private void InitializePlayerData()
    {
        _playerData = new PlayerData();   
    }

    public void SignInXsolla(string username, string password) // Login to Xsolla
    {
        XsollaAuth.SignIn(username, password, OnLoginSuccess, OnError);
    }

    private void OnLoginSuccess() // If successful
    {
        popupManager.ShowPopup("login successful", false);
        XsollaAuth.GetUserInfo(OnGetUserInfoSuccess, OnError);

        loginHandler.DelayedToggleLoginLogoutButton();

        Invoke("LoadHangar", 1f); // Invoke should be okay here, Just loading the store page AND UI text.
    }

    private void LoadHangar()
    {
        HangarManager hangarManager = FindFirstObjectByType<HangarManager>();
        hangarManager.LoadHangarBackend();
    }

    private void OnError(Error error) // If unsuccessful / error // Could be In Game!!!
    {
        Debug.LogError($"Authorization failed. Error: {error.errorMessage}");
        string errorMessage = error.errorMessage;
        if ( popupManager != null ) { popupManager.ShowPopup(errorMessage, true); } // Could be In Game!!!
    }

    private void OnGetUserInfoSuccess(UserInfo userInfo)
    {
        XsollaUserID = userInfo.id; // MUST RETRIEVE userInfo.id FIRST!!!!!!!
        displayName = userInfo.username;
        isLoggedIn = true;
        // Debug.Log("DisplayName: " + displayName);
        LoadDataXsolla(); // load data
    }

    public void SetBoolLogout()
    {
        isLoggedIn = false;
    }

    public void LoadDataXsolla()
    {
        // Define the attribute type as CUSTOM
        UserAttributeType attributeType = UserAttributeType.CUSTOM;

        // Define the list of keys you want to retrieve
        List<string> keys = new List<string> { "game_data" };

        // Call GetUserAttributes with the required parameters
        XsollaUserAccount.GetUserAttributes(attributeType, keys, XsollaUserID, OnGetUserAttributesSuccess, OnError);
    }

    public void ForgotPassword(string username)
    {
        XsollaAuth.ResetPassword(username, OnSuccess, OnError);
    }

    private void OnSuccess() // password reset
    {
        popupManager.ShowPopup("success, check your email", false);
        Debug.Log("Password reset successful"); // Reset email is sent...
    }

    public void GenerateNewAccountData(string displayName) // CALLED ON REGISTER
    {
        // Debug.Log("GeneratingNewAccountData"); 
        _playerData = PlayerData.GenerateNewAccountData(displayName);
        SaveDataXsolla(); // NOT LOGGED IN YET. so User cant save. WOW.
    }

    public void ClearData()
    {
        InitializePlayerData();
        displayName = null;
    }

    public void SaveDataXsolla()
    {
        PlayerData saveData = new PlayerData
        {
            Name = _playerData.Name,
            Kills = _playerData.Kills,
            SelectedSkin = _playerData.SelectedSkin,
            Country = _playerData.Country
        };

        string jsonData = JsonUtility.ToJson(saveData); // Convert SaveData to JSON string

        // Debug.Log("jsonData = " + jsonData);
        // Debug.Log("saveData = " + saveData);

        var jsonAttribute = new UserAttribute
        {
            key = "game_data",
            value = jsonData,
            permission = "public" // Use the appropriate permission value
        };

        XsollaUserAccount.UpdateUserAttributes(new List<UserAttribute> { jsonAttribute }, OnSaveUserAttributesSuccess, OnError);
    }

    public void AddKill()
    {
        if (displayName == null) return;

        _playerData.AddKill(); // Increment kills in the existing player data
        // Debug.Log("_playerData.Kills: " + _playerData.Kills);
        SaveDataXsolla(); // Save the updated player data
    }

    public void SelectFlag(string selectedFlag)
    {
        if (displayName == null) return;

        _playerData.Country = selectedFlag;
        SaveDataXsolla();
    }

    private void OnGetUserAttributesSuccess(UserAttributes userAttributes)
    {
        foreach (var attribute in userAttributes.items)
        {
            if (attribute.key == "game_data")   // IF WE FIND AN EXISTING PLAYER
            {
                // Deserialize the JSON string back into PlayerData object
                PlayerData loadedData = JsonUtility.FromJson<PlayerData>(attribute.value);

                _playerData = loadedData;        // Assign loaded data to _playerData

                selectedSkin = _playerData.SelectedSkin; //FishNet Player class will read this

                SetLeaderboardEntry();
                // leaderboardManager.SetLeaderboardEntry(nameLeaderboard, killsLeaderboard, PlayerXsollaIdLeaderboard);
                
                return; // Exit the loop once we find and process 'game_data'
            }
        }

        // If 'game_data' not found, generate new account data
        GenerateNewAccountData(XsollaUserID); 
        selectedSkin = _playerData.SelectedSkin; //FishNet Player class will read this
    }

    public void SetLeaderboardEntry()
    {
        int killsLeaderboard = _playerData.Kills;
        string nameLeaderboard = displayName;
        string PlayerXsollaIdLeaderboard = XsollaUserID;
        string country = _playerData.Country;

        SetLeaderboardEntry(nameLeaderboard, killsLeaderboard, PlayerXsollaIdLeaderboard, country);
    }

    private void OnSaveUserAttributesSuccess()
    {
        selectedSkin = _playerData.SelectedSkin;
    }

    // [POST]
    public void SetLeaderboardEntry(string username, int score, string xsollaUserID, string country)
    {
        StartCoroutine(PostLeaderboardEntry(username, score, xsollaUserID, country));
    }


    // Will only replace if the SCORE is higher! // NOTE!
    private IEnumerator PostLeaderboardEntry(string username, int score, string xsollaUserID, string country)
    {
        var entry = new APILeaderboardEntry         // Create a new LeaderboardEntry object
        {
            xsollaID = xsollaUserID,
            username = username,
            kills = score.ToString(), //
            country = country,
            clan = "", // empty for now...
            // LastUpdated = DateTime.UtcNow // 
        };

        string jsonEntry = JsonConvert.SerializeObject(entry);
        // Debug.Log("JSON Payload: " + jsonEntry);

        // Set up the POST request
        UnityWebRequest www = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonEntry);

        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        // Add authentication header
        string encodedPassword = Convert.ToBase64String(Encoding.ASCII.GetBytes(apiPublicToken));
        www.SetRequestHeader("Authorization", "Basic " + encodedPassword);

        // Send the request and wait for a response
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            // Debug.LogError("Error posting leaderboard entry: " + www.error);
        }
        else
        {
            // Debug.Log("Successfully posted leaderboard entry: " + www.downloadHandler.text);
        }
    }

// 

}

public class LeaderboardEntry
{
    public string username { get; set; }
    public string kills { get; set; }
    public string country { get; set; }
    public string clan { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class APILeaderboardEntry
{
    public string xsollaID { get; set; }
    public string username { get; set; }
    public string kills { get; set; }
    public string country { get; set; }
    public string clan { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class PlayerData
{
    public string Name; // set to username from LoginHandler
    public int Kills; // Change to Kills
    public string SelectedSkin; // SelectedSkin
    public string Country;
    // public list UnlockedSkins
    // TotalLogins

    public static PlayerData GenerateNewAccountData(string userId) // Public Static, howto get into 'game' scene?
    {
        return new PlayerData()
        {
            Name =  userId, 
            Kills = 0,
            SelectedSkin = "freighter",
            Country = "Australia"
        };
    }

    // Method to increment Kills by 1
    public void AddKill()
    {
        Kills += 1;
    }
}

// private IEnumerator GetGeoLocation(string ipAddress, System.Action<string> onCountryReceived)
// {
//     Debug.Log("GetGeoLocation: " + ipAddress);

//     using (UnityWebRequest geoRequest = UnityWebRequest.Get(string.Format(GetGeoLocationUrl, ipAddress)))
//     {
//         yield return geoRequest.SendWebRequest();

//         if (geoRequest.result != UnityWebRequest.Result.Success)
//         {
//             Debug.LogError("Error getting geolocation: " + geoRequest.error);
//             onCountryReceived(null);
//         }
//         else
//         {
//             // Check if the downloadHandler text is null or empty
//             if (string.IsNullOrEmpty(geoRequest.downloadHandler.text))
//             {
//                 Debug.LogError("Error: Received an empty response");
//                 onCountryReceived(null);
//                 yield break;
//             }

//             // Parse the JSON and check for the presence of the "country" field
//             JObject responseJson = JObject.Parse(geoRequest.downloadHandler.text);
//             JToken countryToken;
//             string country = responseJson.TryGetValue("country", out countryToken) ? countryToken.ToString() : null;

//             if (country == null)
//             {
//                 Debug.LogWarning("Warning: Country field is missing in the response");
//                 onCountryReceived(null);
//             }
//             else
//             {
//                 onCountryReceived(country);
//                 // Debug.Log(country);
//             }
//         }
//     }
// }