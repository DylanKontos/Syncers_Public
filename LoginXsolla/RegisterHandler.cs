using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xsolla.Auth;
using Xsolla.Core;

public class RegisterHandler : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField emailInput;
    public Button registerButton;
    public Button exitButton;
    
    private string username;
    
    private LoginHandler loginHandler;
    PopupManager popupManager;

    // private const string BASE_URL = "https://login.xsolla.com/api"; // Base URL for Xsolla API


    private protected void Start()
    {
        loginHandler = FindFirstObjectByType<LoginHandler>();
        popupManager = FindFirstObjectByType<PopupManager>();


        // Handling the button click
        registerButton.onClick.AddListener(() =>
        {
            // Get the username, email and password from input fields
            var email = emailInput.text;
            username = usernameInput.text;
            var password = passwordInput.text;

            // Call the user registration method
            // Pass credentials and callback functions for success and error cases
            XsollaAuth.Register(username, password, email, OnSuccess, OnError);
            passwordInput.text = null;
        });

        exitButton.onClick.AddListener(ToggleRegistrationPage); // De-Activate Login Menu
    }

    private void OnSuccess(LoginLink loginLink)
    {   
        // XsollaAuth.ExchangeCodeToToken(code);
        // Debug.Log("Registration successful."); // TODO: Make UI 
        popupManager.ShowPopup("Registration successful!", false);
    }

    // private void OnSuccess(LoginLink loginLink)
    // {
    //     GenerateJWT(XsollaSettings.OAuthClientId.ToString(), 
    //         jwt =>
    //         {
    //             // Handle successful JWT generation
    //             Debug.Log("JWT generated successfully: " + jwt);
    //             popupManager.ShowPopup("JWT generated successfully!", false);
    //         },
    //         error =>
    //         {
    //             // Handle JWT generation error
    //             Debug.LogError("JWT generation failed: " + error);
    //             popupManager.ShowPopup("JWT generation failed: " + error, true);
    //         });
    // }



    // public static void GenerateJWT(string clientId, Action<string> onSuccess, Action<Error> onError)
    // {
    //     const string url = "https://login.xsolla.com/api/jwt/generate";

    //     var requestData = new WWWForm();
    //     requestData.AddField("client_id", clientId);

    //     WebRequestHelper.Instance.PostRequest<JWTResponse>(
    //         SdkType.Login,
    //         url,
    //         requestData,
    //         response =>
    //         {
    //             onSuccess?.Invoke(response.jwt);
    //         },
    //         error => onError?.Invoke(error));
    // }

    // public class JWTResponse
    // {
    //     public string jwt { get; set; }
    // }

    // public static void ExchangeCodeToToken(string code, Action onSuccess, Action<Error> onError)
    // {
    //     const string url = BASE_URL + "/oauth2/token";

    //     var requestData = new WWWForm();
    //     requestData.AddField("client_id", XsollaSettings.OAuthClientId);
    //     requestData.AddField("grant_type", "authorization_code");
    //     requestData.AddField("code", code);
    //     requestData.AddField("redirect_uri", XsollaSettings.CallbackUrl);

    //     WebRequestHelper.Instance.PostRequest<TokenResponse>(
    //         SdkType.Login,
    //         url,
    //         requestData,
    //         response =>
    //         {
    //             // Handle the successful token exchange response
    //             onSuccess?.Invoke();
    //         },
    //         error => onError?.Invoke(error));
    // }



    // private void OnSuccess(LoginLink loginLink)
    // {   
    //     // XsollaAuth.ExchangeCodeToToken(code);
    //     // Debug.Log("Registration successful."); // TODO: Make UI 
    //     popupManager.ShowPopup("Registration successful!", false);
    // }

    private void OnError(Error error)
    {
        Debug.LogError($"Registration failed. Error: {error.errorMessage}");
        // Add actions taken in case of error
    }


    private void ToggleRegistrationPage()
    {
        gameObject.SetActive(!registerButton.gameObject.activeSelf);
    }

}
