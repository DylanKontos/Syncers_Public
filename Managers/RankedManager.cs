using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RankedManager : MonoBehaviour
{

    XsollaPlayerDataManager xsollaPlayerDataManager;
    PopupManager popupManager;
    private LoginHandler loginHandler;

    public Button rankedButton;
    public Button closeButton;

    public GameObject rankedBackground;
    public TMP_Text rankedText;
    public TMP_Text rankedText2;

    public Button requestButton;
    public TMP_Text requestButtonText;

    private bool isPlayerSearching = false; // Bool/flag that ensures coroutines only run when this is set true.

    private string appToken = "[OMITTED]"; // PUBLIC Edegap Matchmaker Token


    // Stored locally 
    // The LAST ticket needs to be stored incase of a player quit/exit/disconnect DURING search...
    // Otherwise, "stop matching" is never hit, and ticket will continue to exist...
    private string id = null;
    private string getTicketUrl = null;  // stored

    
    void Start()
    {
        loginHandler = FindFirstObjectByType<LoginHandler>();
        xsollaPlayerDataManager = FindFirstObjectByType<XsollaPlayerDataManager>();
        popupManager = FindFirstObjectByType<PopupManager>();

        closeButton.onClick.AddListener(DisableRankedUI); // Activate Login Menu
        rankedButton.onClick.AddListener(ToggleRankedUI); // Activate Login Menu
        requestButton.onClick.AddListener(ProcessRequest); // Activate Login Menu
    }

    void OnApplicationQuit()
    {
        DeleteTicket(getTicketUrl, id);
    }

    private void ToggleRankedUI()
    {
        // REQUIRE LOGIN
        if ( xsollaPlayerDataManager._playerData.Name == null ) { popupManager.ShowPopup("login required!", true); return; }

        loginHandler.ProcessHome();

        closeButton.gameObject.SetActive(!closeButton.gameObject.activeSelf);
        rankedBackground.gameObject.SetActive(!rankedBackground.gameObject.activeSelf);
        rankedText.gameObject.SetActive(!rankedText.gameObject.activeSelf);
        rankedText2.gameObject.SetActive(!rankedText2.gameObject.activeSelf);

        requestButton.gameObject.SetActive(!requestButton.gameObject.activeSelf);
    }

    public void DisableRankedUI()
    {
        closeButton.gameObject.SetActive(false);
        rankedBackground.gameObject.SetActive(false);
        rankedText.gameObject.SetActive(false);
        rankedText2.gameObject.SetActive(false);
        requestButton.gameObject.SetActive(false);
    }

    public class SentMatchmakerDetails
    {
        public string profile;
        public Attributes attributes;
    }

    public class ResponseDetails
    {
        public string id { get; set; }
        public string profile { get; set; }
        public Assignment assignment { get; set; }
        public DateTime created_at { get; set; }
    }

    public class Attributes 
    { 
        public Dictionary<string, float> beacons; 
    }

    public class Assignment // waiting ticket Assignment == null // Successful ticket Assignment != null
    {
        public string fqdn { get; set; }
        public string public_ip { get; set; }
        public Ports ports { get; set; }
        public Location location { get; set; }
    }

    public class Ports
    {
        public Tugboat tugboat { get; set; }
        public Bayou bayou { get; set; }
    }

    public class Tugboat
    {
        public int internalPort { get; set; }
        public int externalPort  { get; set; }
        public string link { get; set; }
        public string protocol { get; set; }
    }

    public class Bayou
    {
        public int internalPort { get; set; }
        public int externalPort  { get; set; }
        public string link { get; set; }
        public string protocol { get; set; }
    }

    public class Location
    {
        public string city { get; set; }
        public string country { get; set; }
        public string continent { get; set; }
        public string administrative_division { get; set; }
        public string timezone { get; set; }
    }



    // Method to process the request
    private void ProcessRequest()
    {
        isPlayerSearching = !isPlayerSearching; // Initialized at false. Will switch state true/false each ProcessRequest
        //Debug.Log("isPlayerSearching: " + isPlayerSearching);

        if (isPlayerSearching)
        {
            //Debug.Log("isPlayerSearching: " + isPlayerSearching);
            StartMatchmaking();
        }
    }


    IEnumerator ActivateMatchmakingLoadingText()
    {
        string baseText = "matching"; 
        int dotCount = 0;             // Count of dots to append

        while (isPlayerSearching) 
        {
            // Update the text with the correct number of dots
            requestButtonText.text = baseText + new string('.', dotCount);

            // Cycle the dot count between 0 and 3 (so we get loading, loading., loading.., loading...)
            dotCount = (dotCount + 1) % 4;

            // Wait for 1 second before updating the text again
            yield return new WaitForSeconds(1);
        }

        requestButtonText.text = "find match";
    }

    // Method to start matchmaking
    private void StartMatchmaking()
    {
        Debug.Log("Matchmaking...");

        StartCoroutine(ActivateMatchmakingLoadingText());

        // Create the request payload
        SentMatchmakerDetails smd = new SentMatchmakerDetails
        {
            profile = "simple-example",
            attributes = new Attributes
            {
                beacons = new Dictionary<string, float>
                {
                    { "Montreal", 12.3f },
                    { "Toronto", 45.6f },
                    { "Quebec", 78.9f }
                }
            }
        };

        string url = "[OMITTED]";
        string jsonPayload = JsonConvert.SerializeObject(smd); // Convert to JSON using Newtonsoft.Json

        // Start the POST request coroutine
        StartCoroutine(PostTicket(url, jsonPayload));
    }

    IEnumerator PostTicket(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", appToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Process the response
            string responseBody = request.downloadHandler.text;
            //Debug.Log(responseBody);

            // Deserialize the response to get the id
            ResponseDetails responseDetails = JsonConvert.DeserializeObject<ResponseDetails>(responseBody);

            id = responseDetails.id;
            getTicketUrl = url + "/" + id;

            //Debug.Log("ID: " + id);
            // Also store these locally as the last tickets incase of player exist while searching...



            
            StartCoroutine(GetTicket(getTicketUrl, id)); // Lets get the ticket first.
        }

        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }

    IEnumerator GetTicket(string getTicketUrl, string id)
    {
        while (isPlayerSearching)
        {
            var request = new UnityWebRequest(getTicketUrl, "GET");
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", appToken);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseBody = request.downloadHandler.text;
                // Debug.Log("GET Response: " + responseBody);

                try
                {
                    var responseDetails = JsonConvert.DeserializeObject<ResponseDetails>(responseBody);

                    // Check if assignment is not null or response has changed
                    if (responseDetails.assignment != null)
                    {
                        // Debug.Log("Condition met: " + responseBody);   
                        popupManager.ShowPopup("server created!", false);
                        isPlayerSearching = false;
                        // DeleteTicket( getTicketUrl, id); // DELETE THE TICKET if user cancels early.
                        yield break; // Exit the coroutine
                    }
                }
                catch (JsonReaderException ex)
                {
                    Debug.LogError("JsonReaderException: " + ex.Message);
                    Debug.LogError("Response Body: " + responseBody); // Log the full response for diagnosis
                }
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }

            // Wait for 3 seconds before making the next request
            yield return new WaitForSeconds(3);
        }

        // Outside of while (isPlayerSearching)
        DeleteTicket(getTicketUrl, id); // Delete the ticket if the player stops searching
    }

    private void DeleteTicket(string delTicketUrl, string id)
    {
        var request = new UnityWebRequest(delTicketUrl, "DELETE");
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", appToken);
        request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseBody = request.downloadHandler.text;
            Debug.Log("DELETE Response: " + responseBody);
            popupManager.ShowPopup("Stopped Ranked Search...", true);
            Debug.Log("Matchmaking Cancelled");
        }
    }

    // EdgeGap // UnityWebRequest // 
    public static UnityWebRequest CreateApiRequest(string url, string method, string token, object body = null)
    {
        string bodyString = null;
        if (body is string) {
            bodyString = (string)body;
        } else if (body != null) {
            bodyString = JsonConvert.SerializeObject(body);
        }

        var request = new UnityWebRequest();
        request.url = url;
        request.method = method;
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(string.IsNullOrEmpty(bodyString) ? null : Encoding.UTF8.GetBytes(bodyString));
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", token);
        request.timeout = 30;
        return request;
    }
}


