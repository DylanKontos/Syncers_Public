using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour // Attached directly to Canvas.Leaderboard
{
    string apiUrl = "[OMITTED]";
    string apiPublicToken = "[OMITTED]";

    // private const string GetGeoLocationUrl = "[OMITTED]";
    // private const string GetIpUrl = "[OMITTED]";
    // private const string GetGeoLocationUrl =  "[OMITTED]";
    // https://www.ip2location.io/dashboard // Previous API proivder // Could only get country codes...
    // private const string GetGeoLocationUrl = "[OMITTED]"; // Replace with your actual token

    private Dictionary<string, LeaderboardEntry> leaderboardDictionary = new Dictionary<string, LeaderboardEntry>();

    public Button leaderboardButton;
    public Button closeButton;
    public GameObject leaderboardBackground;
    public TMP_Text leaderboardText;

    private LoginHandler loginHandler;

    [SerializeField] 
    private List<TextMeshProUGUI> names;

    [SerializeField] 
    private List<TextMeshProUGUI> scores;

    [SerializeField] 
    private List<TextMeshProUGUI> countries;

    [SerializeField]
    private List<Image> flags;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loginHandler = FindFirstObjectByType<LoginHandler>();
        leaderboardButton.onClick.AddListener(ToggleLeaderboard); // Activate Login Menu
        closeButton.onClick.AddListener(DisableLeaderboard); // Activate Login Menu
        // GetLeaderboard();
        StartCoroutine(GetLeaderboard()); // Start the GetLeaderboard coroutine
    }

    private void ToggleLeaderboard()
    {
        loginHandler.ProcessHome();
        leaderboardBackground.gameObject.SetActive(!leaderboardBackground.gameObject.activeSelf);
        leaderboardText.gameObject.SetActive(!leaderboardText.gameObject.activeSelf);
        closeButton.gameObject.SetActive(!closeButton.gameObject.activeSelf);
    }


    public void DisableLeaderboard()
    {
        leaderboardBackground.gameObject.SetActive(false);
        leaderboardText.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false); 
    }

    // [GET] //
    private IEnumerator GetLeaderboard()
    {
        while (true)
        {
            using (UnityWebRequest www = ServerDetailsSender.CreateApiRequest(apiUrl, UnityWebRequest.kHttpVerbGET, apiPublicToken))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                    yield return new WaitForSeconds(10);
                }

                else
                {
                    string jsonPayload = www.downloadHandler.text;
                    // Debug.Log("Raw JSON payload: " + jsonPayload);

                    // Remove extra backslashes and quotes
                    jsonPayload = jsonPayload.Replace("\\\"", "\"").Trim('"');

                    // Debug.Log(www.responseCode + " " + jsonPayload);
                    LoadLeaderboard(jsonPayload);
                    yield return new WaitForSeconds(10);
                }
            }
        }
    }

    // Pass in GET and LOAD
    private void LoadLeaderboard(string jsonPayload)
    {
        try
        {
            // Deserialize the JSON payload into a list of LeaderboardEntry objects
            var receivedLeaderboard = JsonConvert.DeserializeObject<List<LeaderboardEntry>>(jsonPayload);
            leaderboardDictionary.Clear(); // Clear previous entries 

            // Iterate through the deserialized leaderboard entries
            foreach (LeaderboardEntry ld in receivedLeaderboard)
            {
                if (ld.country == null) { ld.country = "Unknown"; }

                if (ld.username != null) // Ensure username is not null
                {
                    // Debug.Log($"Username: {ld.username}, Kills: {ld.kills}, Country: {ld.country}, Clan: {ld.clan}, Last Updated: {ld.LastUpdated}");
                    leaderboardDictionary[ld.username] = ld; // Add/update entry in the dictionary
                }
                
                else
                {
                    Debug.LogWarning("Received a leaderboard entry with a null username. Skipping entry.");
                }
            }

            // Populate the leaderboard UI with the new data
            PopulateLeaderboardUI();
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred during deserialization: {e}");
        }
    }


    private void PopulateLeaderboardUI()
    {
        // Sort the entries by the number of kills in descending order
        var sortedEntries = leaderboardDictionary.Values.OrderByDescending(entry => int.Parse(entry.kills)).ToList();

        for (int i = 0; i < sortedEntries.Count; i++)
        {
            if (i >= names.Count || i >= scores.Count || i >= countries.Count)
            {
                break; // Prevent index out of range errors
            }

            // Set names and Kills
            names[i].text = sortedEntries[i].username;
            scores[i].text = sortedEntries[i].kills;

            // Set country name based on a CountryCode received from geolocator
            
            string countryName = sortedEntries[i].country;
            // string countryCode = sortedEntries[i].country;
            // string countryName = CountryDatabase.GetCountryName(countryCode); // Will return "Unknown"; if not here


            // if country is null, break, after setting unkown country.
            // sortedEntries[i].country == null 
            
            if (string.IsNullOrEmpty(sortedEntries[i].country) || countryName == "Unknown" || countryName == null)
            {
                countries[i].text =  "Unknown";
                string flagPathNull = $"Flags/{"australia"}_16";
                Sprite flagSpriteNull = Resources.Load<Sprite>(flagPathNull);

                if (flagSpriteNull != null && flags[i].gameObject != null)
                {
                    flags[i].gameObject.SetActive(true);
                    flags[i].sprite = flagSpriteNull;
                }
            }

            else
            {
                countries[i].text = countryName;
                
                string flagPath = countryName.ToLower().Replace(" ", "_");
                flagPath = $"Flags/{flagPath}_16";
                Sprite flagSprite = Resources.Load<Sprite>(flagPath);

                if (flagSprite != null && flags[i].gameObject != null)
                {
                    flags[i].gameObject.SetActive(true);
                    flags[i].sprite = flagSprite;
                }
            }
        }
    }
            


    // GetIP & GetCountry // // GetIP & GetCountry // GetIP & GetCountry // GetIP & GetCountry // GetIP & GetCountry
    // GetIP & GetCountry // // GetIP & GetCountry // GetIP & GetCountry // GetIP & GetCountry // GetIP & GetCountry
    // GetIP & GetCountry // // GetIP & GetCountry // GetIP & GetCountry // GetIP & GetCountry // GetIP & GetCountry
    // GetIP & GetCountry // // GetIP & GetCountry // GetIP & GetCountry // GetIP & GetCountry // GetIP & GetCountry


    
    // CORS or NULL issue in browser... USE FISHNET TO FETCH IP WHEN PLATER JOINS A SERVER
    // private IEnumerator GetIpAddress(System.Action<string> onIpReceived)
    // {
    //     using (UnityWebRequest ipRequest = UnityWebRequest.Get(GetIpUrl))
    //     {
    //         yield return ipRequest.SendWebRequest();

    //         if (ipRequest.result != UnityWebRequest.Result.Success)
    //         {
    //             Debug.LogError("Error getting IP: " + ipRequest.error);
    //             onIpReceived(null);
    //         }
    //         else
    //         {
    //             string ipAddress = JObject.Parse(ipRequest.downloadHandler.text)["ip"].ToString();
    //             onIpReceived(ipAddress);
    //         }
    //     }
    // }



}


        // Dictionary<string, int> leaderboard = new Dictionary<string, int>();
        // Debug.Log("Received JSON payload: " + jsonPayload); // logs RAW JSON payload/string

                    // Add receivedLeaderboard to dictionary

            // Log the details of all servers
            // foreach (LeaderboardEntry ld in receivedLeaderboard)
            // {
            //     Debug.Log(ld);
            // }


    // Old Leaderboard using LeaderboardCreator
    // https://danqzq.itch.io/leaderboard-creator


    // private string publicLeaderboardKey = "e01c8e036fa6199fd4394c2fdb2ba007d7e66b33c6235c0a21c43f00bd0296d7";

    // public void SetLeaderboardEntry(string username, int score)   
    // {
    //     LeaderboardCreator.UploadNewEntry(publicLeaderboardKey, username, 
    //         score, ((msg) => {
    //         GetLeaderboard();
    //     }));
    // }
    // Dictionary<string, int> leaderboard = new Dictionary<string, int>();
    // LeaderboardCreator.GetLeaderboard(publicLeaderboardKey, ((msg) => {          // GetLeaderboard will return value (msg)


    // // Iterate through the leaderboard entries
    // foreach (var entry in msg)
    // {
    //     // Update the dictionary only if the current score is higher than the stored score
    //     if (!leaderboard.ContainsKey(entry.Username) || leaderboard[entry.Username] < entry.Score)
    //     {
    //         leaderboard[entry.Username] = entry.Score;
    //     }
    // }

    // // Update the names and scores lists using the dictionary
    // int i = 0;
    // foreach (var kvp in leaderboard)
    // {
    //     if (i < names.Count) // Ensure we don't exceed the size of the names and scores lists
    //     {
    //         names[i].text = kvp.Key;
    //         scores[i].text = kvp.Value.ToString();
    //         i++;
    //     }
    // }
    //     }));





            //     // Turn country into lower case, replace " " with _, add _16.
            // string countryLowerCase = sortedEntries[i].country.ToLower().Replace(" ", "_");
            // string flagPath = $"Flags/{countryLowerCase}_16";
            // Sprite flagSprite = Resources.Load<Sprite>(flagPath);

            // if (flagSprite != null && flags[i].gameObject != null)
            // {
            //     flags[i].gameObject.SetActive(true);
            //     flags[i].sprite = flagSprite;
            //     Color flagColor = flags[i].color;
            //     flagColor.a = 1f; // Set alpha to 1 (fully opaque)
            // }
            
            // else
            // {
            //     if (flags[i].gameObject != null) 
            //     { 
            //         flags[i].gameObject.SetActive(false); 
            //         Debug.LogError($"Flag sprite not found for country: {sortedEntries[i].country}");
            //     }
            // }


            
    //         else 
    //         {
    //             sortedEntries[i].country = "Unknown";
    //             countries[i].text = sortedEntries[i].country;

    //             string flagPathNull = $"Flags/{"antarctica"}_16";
    //             Sprite flagSpriteNull = Resources.Load<Sprite>(flagPathNull);

    //             if (flagSpriteNull != null && flags[i].gameObject != null)
    //             {
    //                 flags[i].gameObject.SetActive(true);
    //                 flags[i].sprite = flagSpriteNull;
    //             }

    //             break;
    //         }
            
    //         break;


    //         // Check if country code is null or empty
    //         // string countryCode = sortedEntries[i].country;
            
    //         if (string.IsNullOrEmpty(countries))
    //         {
    //             countryCode = "unknown"; // Default value for missing country codes
    //         }

    //         // Get country name from database and convert to lowercase
    //         string countryName = CountryDatabase.GetCountryName(countryCode).ToLower();
    //         countries[i].text = countryName.Replace("_", " ");

    //         Debug.Log(countryName);

    //         // Flag logic
    //         try
    //         {
    //             string countryLowerCase = countryCode.ToLower().Replace(" ", "_");
    //             string flagPath = $"Flags/{countryLowerCase}_16";
    //             Sprite flagSprite = Resources.Load<Sprite>(flagPath);

    //             if (flagSprite != null && flags[i].gameObject != null)
    //             {
    //                 flags[i].gameObject.SetActive(true);
    //                 flags[i].sprite = flagSprite;
    //             }

    //             else
    //             {
    //                 string flagPathUnknown = $"Flags/antarctica_16"; // Default flag for unknown countries
    //                 Sprite flagSpriteUnknown = Resources.Load<Sprite>(flagPathUnknown);

    //                 if (flagSpriteUnknown != null && flags[i].gameObject != null)
    //                 {
    //                     flags[i].gameObject.SetActive(true);
    //                     flags[i].sprite = flagSpriteUnknown;
    //                     countries[i].text = "Unknown";
    //                 }

    //                 Debug.LogWarning($"Flag sprite not found for country: {countryCode}");
    //             }
    //         }

    //         catch (Exception ex)
    //         {
    //             Debug.LogWarning($"Error loading flag for country: {countryCode} - {ex.Message}");
    //         }
    //     }
    // }
