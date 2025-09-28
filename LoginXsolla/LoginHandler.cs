using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Kino;
using Xsolla.Auth;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////// Data will be fetched from Xsolla, not from any carry-over (from LOBBY to GAME) //////////////////////////
//////////////// Ensure everything is saved after being set ////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class LoginHandler : MonoBehaviour 
{
    public Button loginButton;
    public Button enterButton;
    public Button exitButton;
    public Button registerButton;
    public Button forgotPasswordButton;
    public Button hangerButton;
    public Button logoutButton;
    public Button homeButton;
    public Button loginCloseButton;
    
    public GameObject couponButton; // Disabled for PC builds // TODO: add to HangarManager
    public GameObject buySkinWidget; // Disabled for PC builds // TODO: add to HangarManager

    public Camera mainCam;

    public Button saveButton;
    public Button loadButton;
    public Button incrementButton;

    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    public GameObject loginBackground;
    public GameObject registerGameObject;
    public GameObject serverList;
    public GameObject hangarUI;

    RankedManager rankedManager;
    LeaderboardManager leaderboardManager;
    XsollaPlayerDataManager xsollaPlayerDataManager;
    HangarManager hangarManager;
    PopupManager popupManager;

    private string displayName;
    private string projectId = "[OMITTED]"; // TODO check if needed

    private const int MaxCharacterLimit = 10;

    public PlayerData _playerData;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;


    void Start()
    {
        xsollaPlayerDataManager = FindFirstObjectByType<XsollaPlayerDataManager>();
        hangarManager = FindFirstObjectByType<HangarManager>();
        popupManager = FindFirstObjectByType<PopupManager>();
        leaderboardManager = FindFirstObjectByType<LeaderboardManager>();   
        rankedManager = FindFirstObjectByType<RankedManager>();
        
        homeButton.onClick.AddListener(ProcessHome);
        loginCloseButton.onClick.AddListener(ProcessCloseLogin);
        loginButton.onClick.AddListener(ToggleLoginUIElements); // Activate Login Menu
        logoutButton.onClick.AddListener(ProcessLogout); // Activate Login Menu
        exitButton.onClick.AddListener(ProcessCloseLogin); // De-Activate Login Menu
        enterButton.onClick.AddListener(ProcessLogin); // Logs In (or press enter)
        registerButton.onClick.AddListener(OpenRegisterWindow);
        saveButton.onClick.AddListener(xsollaPlayerDataManager.SaveDataXsolla);
        loadButton.onClick.AddListener(xsollaPlayerDataManager.LoadDataXsolla);
        incrementButton.onClick.AddListener(xsollaPlayerDataManager.AddKill);
        forgotPasswordButton.onClick.AddListener(ProcessForgotPassword);
        hangerButton.onClick.AddListener(ProcessHangar);
        
        
        // DISABLE PC ELEMENTS FOR STEAM UNTIL BUILT
        // DISABLE PC ELEMENTS FOR STEAM UNTIL BUILT
        // DISABLE PC ELEMENTS FOR STEAM UNTIL BUILT 
        // DISABLE PC ELEMENTS FOR STEAM UNTIL BUILT
        // DISABLE PC ELEMENTS FOR STEAM UNTIL BUILT
    
    #if UNITY_STANDALONE_WIN
        if (hangerButton != null)
        {
            hangerButton.interactable = false;
            
            CanvasGroup group = hangerButton.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = hangerButton.gameObject.AddComponent<CanvasGroup>();
            }
            group.alpha = 0f;           // Makes it fully invisible
            group.interactable = false; // Optional: disables interaction
            group.blocksRaycasts = false; // Optional: lets clicks pass through
            // hangerButton.gameObject.SetActive(false);

        }
        if (rankedManager != null)
        {
            rankedManager.rankedButton.interactable = false;
            CanvasGroup group = rankedManager.rankedButton.GetComponent<CanvasGroup>();
            
            if (group == null)
            {
                group = rankedManager.rankedButton.gameObject.AddComponent<CanvasGroup>();
            }
            group.alpha = 0f;           // Makes it fully invisible
            group.interactable = false; // Optional: disables interaction
            group.blocksRaycasts = false; // Optional: lets clicks pass through
            // hangerButton.gameObject.SetActive(false);
        }
    #endif


        if (mainCam != null)
        {
            originalCameraPosition = mainCam.transform.position;
            originalCameraRotation = mainCam.transform.rotation;
        }
        
    }

    void Update()
    {
        if (usernameInput.isActiveAndEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ProcessLogin();
            }
        }
    }

    private void ProcessCloseLogin()
    {
        DisableLoginUIElements();
        registerGameObject.gameObject.SetActive(false);
    }

    public void ProcessHome()
    {
        mainCamGlitchOn();
        // Enable Server List
        Canvas serverListCanvas = serverList.GetComponent<Canvas>();            
        serverListCanvas.enabled = true;

        // Login + Register
        DisableLoginUIElements();
        registerGameObject.gameObject.SetActive(false);

        // Hangar
        hangarUI.gameObject.SetActive(false);
        
        // Reset camera incase its in Hangar
        if (mainCam != null)
        {
            mainCam.transform.rotation = originalCameraRotation; // Reset camera to original transform
        }
        
        // Leaderboard
        leaderboardManager.DisableLeaderboard();

        //Ranked
        rankedManager.DisableRankedUI();
    }

    private void ProcessHangar()
    {
        ProcessHome();
        mainCamGlitchOn(); //vfx
        DisableServerList();
        EnableHangarUI();
        
        if (mainCam != null)
        {
            //StartCoroutine(SmoothPanCamera(180)); 
            // If using coroutine, disable the hangar button otherwise spam clicking will mess up rotation
            
            // Rotate the camera 180 degrees around the Y axis
            mainCam.transform.Rotate(0, 180, 0);
        }
    }

    private void mainCamGlitchOn()
    {
        DigitalGlitch digitalGlitch = mainCam.GetComponent<DigitalGlitch>();
        AnalogGlitch analogGlitch = mainCam.GetComponent<AnalogGlitch>();

        digitalGlitch.enabled = true;
        analogGlitch.enabled = true;

        Invoke("mainCamGlitchOff", 0.2f); // Correct syntax for Invoke
    }

    private void mainCamGlitchOff()
    {
        DigitalGlitch digitalGlitch = mainCam.GetComponent<DigitalGlitch>();
        AnalogGlitch analogGlitch = mainCam.GetComponent<AnalogGlitch>();

        digitalGlitch.enabled = false;
        analogGlitch.enabled = false;
    }

    private void EnableHangarUI()
    {
        hangarUI.gameObject.SetActive(!hangarUI.gameObject.activeSelf);
        
        
        // DISABLE PC ELEMENTS FOR STEAM UNTIL BUILT
        // DISABLE PC ELEMENTS FOR STEAM UNTIL BUILT
        // DISABLE PC ELEMENTS FOR STEAM UNTIL BUILT
        
    #if UNITY_STANDALONE_WIN
            DisablePCUIElements();
    #endif

    }

    private void DisablePCUIElements()
    {
        if (couponButton != null) couponButton.SetActive(false);
        if (buySkinWidget != null) buySkinWidget.SetActive(false);
    }

    private IEnumerator SmoothPanCamera(float angle)
    {
        float duration = 2.0f; // Duration of the pan in seconds
        float elapsed = 0.0f;
        Quaternion startRotation = mainCam.transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, angle, 0);

        while (elapsed < duration)
        {
            mainCam.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.rotation = endRotation;
        Debug.Log("Camera pan completed.");
    }

    private void DisableServerList()
    {
        if (serverList != null)
        {
            Canvas serverListCanvas = serverList.GetComponent<Canvas>();            
            serverListCanvas.enabled = !serverListCanvas.enabled;

            // serverList.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Server list GameObject not found!");
        }
    }

    private void ProcessForgotPassword()
    {
        string username = usernameInput.text;
        xsollaPlayerDataManager.ForgotPassword(username);
    }

    private void ToggleLoginUIElements() // Toggle the active state of the buttons and input fields
    {
        ProcessHome();

        loginCloseButton.gameObject.SetActive(!loginCloseButton.gameObject.activeSelf);
        loginBackground.gameObject.SetActive(!loginBackground.gameObject.activeSelf);
        enterButton.gameObject.SetActive(!enterButton.gameObject.activeSelf);
        exitButton.gameObject.SetActive(!exitButton.gameObject.activeSelf);

        usernameInput.gameObject.SetActive(!usernameInput.gameObject.activeSelf);
        passwordInput.gameObject.SetActive(!passwordInput.gameObject.activeSelf);

        registerButton.gameObject.SetActive(!registerButton.gameObject.activeSelf);
        forgotPasswordButton.gameObject.SetActive(!forgotPasswordButton.gameObject.activeSelf);
    }

    private void DisableLoginUIElements()
    {
        loginBackground.gameObject.SetActive(false);
        enterButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
        loginCloseButton.gameObject.SetActive(false);

        usernameInput.gameObject.SetActive(false);
        passwordInput.gameObject.SetActive(false);

        registerButton.gameObject.SetActive(false);
        forgotPasswordButton.gameObject.SetActive(false);
    }

    private void ToggleLoginLogoutButton()
    {
        // Remove Login, Add Logout.
        loginButton.gameObject.SetActive(!loginButton.gameObject.activeSelf);
        logoutButton.gameObject.SetActive(!logoutButton.gameObject.activeSelf);
    }

    public void DelayedToggleLoginLogoutButton()
    {
        Invoke("ToggleLoginLogoutButton", 3f);
    }



    private void ProcessLogout()
    {
        XsollaAuth.Logout(OnSuccess, OnError); // LogoutType.All);    
    }

    private void OnError(Xsolla.Core.Error error) // Adjusted parameter type
    {
        Debug.LogError($"Operation failed. Error: {error.errorMessage}");
        string errorMessage = error.errorMessage;
        popupManager.ShowPopup(errorMessage, false);
        // Add any additional actions to be taken in case of error
    }

    private void OnSuccess() // LOGOUT SUCCESS
    {
        popupManager.ShowPopup("logged out", false);
        ToggleLoginLogoutButton();
        xsollaPlayerDataManager.ClearData(); // DATA CLEARED ON LOGOUT
        xsollaPlayerDataManager.SetBoolLogout();
        hangarManager.ClearHangarText();
        hangarManager.ClearOwnedSkus();
    }



    private void ProcessLogin()
    {
        if (XsollaAuth.IsUserAuthenticated()) 
        {
            popupManager.ShowPopup("Already logged in!", true);
            return;
        }

        string username = usernameInput.text;
        string password = passwordInput.text;
        
        xsollaPlayerDataManager.SignInXsolla(username, password);
        passwordInput.text = null;
    }

    private void OpenRegisterWindow()
    {
        // ToggleLoginUIElements();
        ToggleRegisterUIElements();
        // Debug.Log("OpenRegister");
    }

    private void ToggleRegisterUIElements()
    {
        registerGameObject.gameObject.SetActive(true);

        // registerGameObject.gameObject.SetActive(!registerButton.gameObject.activeSelf);
    }
}


// USING UNITY PRE XSOLLA

// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using Unity.Services.Authentication;
// using Unity.Services.Core;
// using System.Collections.Generic;
// using Unity.Services.CloudSave;

// public class LoginHandler : MonoBehaviour // Game Manager
// {
//     public Button loginButton;
//     public Button enterButton;
//     public Button exitButton;
//     public Button settingsButton;
//     public Button logoutButton;
//     public Button loadButton;
//     public TMP_InputField usernameInput;
//     public TMP_InputField passwordInput;
//     public GameObject loginBackground;
    
//     public PlayerData _playerData;

//     public string username;

//     private const int MaxCharacterLimit = 10;

//     void Start()
//     {
//         loginButton.onClick.AddListener(ToggleUIElements);
//         exitButton.onClick.AddListener(ToggleUIElements);
//         enterButton.onClick.AddListener(ProcessLogin);

//         settingsButton.onClick.AddListener(AddKill);

//         logoutButton.onClick.AddListener(SaveData);
//         loadButton.onClick.AddListener(LoadData);

//         // InitializeUnityServices();
//         // InitializePlayerData(); // Just load a blank player data so it's initialized and not null when calling load.
//     }

//     void Update()
//     {
//         if (usernameInput.isActiveAndEnabled)
//         {
//             if (Input.GetKeyDown(KeyCode.Return))
//             {
//                 ProcessLogin();
//             }
//         }
//     }


//     private async void InitializeUnityServices()
//     {
//         await UnityServices.InitializeAsync(); // call before using any unity services
//     }

//     private void InitializePlayerData()
//     {
//         _playerData = new PlayerData();   
//     }



//     private void ToggleUIElements() // Toggle the active state of the buttons and input fields
//     {
//         loginBackground.gameObject.SetActive(!loginBackground.gameObject.activeSelf);
//         enterButton.gameObject.SetActive(!enterButton.gameObject.activeSelf);
//         exitButton.gameObject.SetActive(!exitButton.gameObject.activeSelf);
//         usernameInput.gameObject.SetActive(!usernameInput.gameObject.activeSelf);
//         passwordInput.gameObject.SetActive(!passwordInput.gameObject.activeSelf);
//     }

//     private async void ProcessLogin()
//     {
//         string username = usernameInput.text;
//         string password = passwordInput.text;

//         Debug.Log("Entered Username: " + username);
//         Debug.Log("Entered Password: " + password);

//         try   // Sign up or sign in the user with the provided username and password
//         {
//             await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
//             Debug.Log("SIGN-UP successful.");
//             GenerateNewAccountData(username); // Pass username to generate new account data
//             SaveData(); // invoke? wait? async?
            
//         }
//         catch (AuthenticationException ex)
//         {
//             Debug.LogError("Authentication error: " + ex.Message);
//             try    // Attempt to sign in if SignUp fails (username might already exist)
//             {
//                 await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
//                 Debug.Log("SIGN-IN successful.");
                

//                 LoadData();
//                 // Generate a default/blank player data, InitializePlayerData(); has been run in start.

//             }
//             catch (AuthenticationException signInEx)
//             {
//                 Debug.LogError("SignIn error: " + signInEx.Message);
//             }
//         }
//     }

//     private void AddKill()
//     {
//         _playerData.AddKill(); // Increment kills in the existing player data
//         SaveData(); // Save the updated player data
//     }

//     // Example of a client modifying PlayerData without permission.
//     public void GenerateNewAccountData(string username)
//     {
//         _playerData = PlayerData.GenerateNewAccountData(username);
//     }


//     async void LoadData()
//     {
//         var loadData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>() { "name", "kills", "class",});

//         if (loadData.TryGetValue("name", out var nameObject)) _playerData.Name = nameObject.Value.GetAsString();
//         if (loadData.TryGetValue("kills", out var killsObject)) _playerData.Kills = killsObject.Value.GetAs<int>();
//         if (loadData.TryGetValue("class", out var classObject)) _playerData.Class = classObject.Value.GetAsString();


//         Debug.Log("Loaded Name: " + _playerData.Name);
//         Debug.Log("Loaded Kills: " + _playerData.Kills);
//         Debug.Log("Loaded Class: " + _playerData.Class);

//         // TODO
//         // Save this lodad data to a GameObject that passes into 'game' scene 
//     }
        
//     async void SaveData()
//     {
//         var saveData = new Dictionary<string, object>();
//         saveData ["name"] = _playerData.Name;
//         saveData ["kills"] = _playerData.Kills;
//         saveData ["class"] = _playerData.Class;

//         Debug.Log("Saved Name: " + _playerData.Name);
//         Debug.Log("Saved Kills: " + _playerData.Kills);
//         Debug.Log("Saved Class: " + _playerData.Class);

//         await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
//     }


//     // Player Class // 

//     public class PlayerData
//     {
//         public string Name; // set to username from LoginHandler
//         public int Kills; // Change to Kills
//         public string Class; // SelectedSkin
//         // public list UnlockedSkins
//         // TotalLogins

//         public static PlayerData GenerateNewAccountData(string username) // Public Static, howto get into 'game' scene?
//         {
//             // TODO Generate Blank
//             return new PlayerData()
//             {
//                 Name =  username, // "Player" + UnityEngine.Random.Range(0,100),
//                 Kills = 0,
//                 Class = "Freighter" 
//             };
//         }

//         // Method to increment Kills by 1
//         public void AddKill()
//         {
//             Kills += 1;
//         }
//     }

// }
