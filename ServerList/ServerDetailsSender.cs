// #if UNITY_SERVER || UNITY_EDITOR // If Server, Dump ServerDetails to a .json payload for Server
// #if(Application.Platform == RuntimePlatform.LinuxServer) 
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting.Bayou;
using FishNet.Transporting.Tugboat;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

#if UNITY_SERVER || UNITY_EDITOR
using Edgegap;
using IO.Swagger.Model;
#endif


public class ServerDetailsSender : NetworkBehaviour    // This class/gameObject is in 'Game' scene, must communicate with playerManager.
{

    #if UNITY_SERVER || UNITY_EDITOR

    // README
    // Step 1 in the server list
    // This will POST to the API
    // Attached SeverDetailsSender game object in scene: `Game`.
    // Will also store below password to ensure only the SERVER can make POST requests.
    // Data/Server information is accessed from gameManager and networkManager.

    private string password = "[OMITTED]"; // syncers API
    private string appToken = "[OMITTED]"; //EdgeGap token

    Tugboat tugboat;
    Bayou bayou;
    PlayerManager playerManager;
    GameManager gameManager;
    NetworkManager networkManager;
    EdgegapManager edgegapManager; // Identify Edgegap NetworkManager
    private Status pendingStatus;


    // Array of deployments to list amount
    private Deployment[] deployments = new Deployment[0];

    // Dictionary for ListAllDeployments to populate
    private Dictionary<string, DeploymentDetails> deploymentDetails = new Dictionary<string, DeploymentDetails>();

    // Store Deployment Details in a class.
    public class DeploymentDetails
    {
        public string Fqdn { get; set; }
        public string tugboatExternal { get; set; }
        public string bayouExternal { get; set; }
        public string Continent { get; set; }
    }

    public class SentServerDetails
    {
        public string TugboatPort; // THIS IS THE KEY FOR THE API DICTIONARY!!!!!!!
        public string BayouPort;
        public string Ip; // EdgeGap
        public string Port; // EdgeGap
        public string Continent; // EdgeGap
        public string ServerName;
        public string GameMode;
        public string Map;
        public string Players;
    }

    // Edgegap
    /// This script acts as an interface to display and use the necessary variables from the Edgegap tool.
    /// The server info can be accessed from the tool window, as well as through the public script property.
    public class EdgegapToolScript : MonoBehaviour
    {
        public Status ServerStatus => EdgegapServerDataManager.GetServerStatus();
    }

    public struct PagginationPage<T>
    {
        public T[] data;
        public int total_count;
        public Paggination pagination;
        //public string messages;
    }

    public struct Paggination
    {
        public int number;
        public bool has_next;
        public bool has_previous;
    }

    public override void OnStartServer() // #1 Start of EdgeGap/VPS server differentiation
    {
        if (!IsServerInitialized) return; 

        StartCoroutine(CheckNetworkManager());
        // Begin the chain, Edgegap has issues finding network manager OnStartServer...
        // So lets see if this works...
    }

    private IEnumerator CheckNetworkManager() // Delay to ensure NetowrkManager is Initialized, then we can start sending details...
    {
        while (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
            
            if (networkManager == null)
            {
                Debug.Log("NetworkManager is null, checking again...");
                yield return new WaitForSeconds(1f); // Wait for 1 second before checking again
            }
        }

        edgegapManager = networkManager.GetComponent<EdgegapManager>();

        if (edgegapManager != null) // If this is an edgegap deployment with edgegap network manager then...
        {
            Debug.Log("Edgegap deployment detected");
            Invoke("StartCoroutineGetDeploymentsList", 2.0f); // 2s Delay for EdgeGap

        }
        else // If a server on VPS then just do as per usual
        {
            Debug.Log("Non-Edgegap deployment detected");
            Invoke("StartCoroutineCreateServerDetails", 2.0f); // 2s Delay for playerManager to load.
        }
    }


    private void StartCoroutineCreateServerDetails() // VPS servers POST
    {
        StartCoroutine(CreateSeverDetails());
    }

    private void StartCoroutineGetDeploymentsList() // Fetch EdgeGap
    {
        StartCoroutine(GetDeploymentsList()); // EdgeGap // Finds deployments & populates dictionary  
    }

    private void StartCoroutineCreateEdgegapServerDetails() // 3f delay populate EdgeGap servers POST
    {
        StartCoroutine(CreateEdgegapServerDetails()); // Edegegap // Uses dictionary and sends API request
    } 

    // EdgeGap // // EdgeGap // // EdgeGap // // EdgeGap // 
    private IEnumerator GetDeploymentsList() 
    {
        while (true)
        {
            using (UnityWebRequest www = CreateApiRequest("https://api.edgegap.com/v1/deployments", UnityWebRequest.kHttpVerbGET, appToken))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    string jsonResponse = Encoding.UTF8.GetString(www.downloadHandler.data);
                    // Debug.Log(www.responseCode + " " + jsonResponse);
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.NullValueHandling = NullValueHandling.Ignore;
                    PagginationPage<Deployment> result = JsonConvert.DeserializeObject<PagginationPage<Deployment>>(jsonResponse, settings);
                    deployments = result.data;
                    // Debug.Log(result.data);

                    if (deployments == null || deployments.Length == 0)
                    {
                        Debug.LogWarning("No deployments returned from API.");
                    }

                    foreach (var deployment in deployments)
                    {
                        // Debug.Log($"FQDN: {deployment.Fqdn}, tugboat External: {deployment.Ports["tugboat"].External}");
                        // Debug.Log($"FQDN: {deployment.Fqdn}, bayou External: {deployment.Ports["bayou"].External}");

                        // Store deployment details temporarily
                        var deploymentDetail = new DeploymentDetails
                        {
                            Fqdn = deployment.Fqdn,
                            tugboatExternal = deployment.Ports["tugboat"].External?.ToString(),
                            bayouExternal = deployment.Ports["bayou"].External?.ToString()
                        };

                        deploymentDetails[deployment.RequestId] = deploymentDetail;
                        StartCoroutine(GetDeployementStatus(deployment.RequestId));
                    }
                }

                yield return new WaitForSeconds(10f); // Request EdgeGap deployments every 10 seconds...
            }
        }
    }

    // EdgeGap // // EdgeGap // // EdgeGap // // EdgeGap // 
    private IEnumerator GetDeployementStatus(string request_id)
    {
        using (UnityWebRequest www = CreateApiRequest($"https://api.edgegap.com/v1/status/{request_id}", UnityWebRequest.kHttpVerbGET, appToken))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                string jsonResponse = Encoding.UTF8.GetString(www.downloadHandler.data);
                // Debug.Log(www.responseCode + " " + jsonResponse);
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                pendingStatus = JsonConvert.DeserializeObject<Status>(jsonResponse, settings);
                // Debug.Log($"Continent: {pendingStatus.Location.Continent}"); // Logs "North America"
                deploymentDetails[request_id].Continent = pendingStatus.Location.Continent; // Will fetch Continent
                // And then store it in the dictionary based on the key!
                StartCoroutineCreateEdgegapServerDetails(); // After all is fetched, start the server details creator
            }
        }
    }

private IEnumerator CreateEdgegapServerDetails()
{
    while (true)
    {
            networkManager = FindObjectOfType<NetworkManager>();
            tugboat = networkManager.GetComponent<Tugboat>();
            bayou = networkManager.GetComponent<Bayou>();      // Bayou is only on WebGL NetworkManager
            playerManager = FindObjectOfType<PlayerManager>();
            gameManager = FindObjectOfType<GameManager>();

        foreach (var detail in deploymentDetails)
        {
            SentServerDetails ssd = new SentServerDetails
            {
                TugboatPort = detail.Value.tugboatExternal, // Was tugboat/
                BayouPort = detail.Value.bayouExternal, // was bayou to string
                Ip = detail.Value.Fqdn, // EdgeGap
                Port = detail.Value.tugboatExternal, // Just EdgeGap doubling the TugBoat port to ensure API receives .json
                Continent = detail.Value.Continent, // EdgeGap
                ServerName = gameManager._serverName,
                GameMode = gameManager._gameMode,
                Players = playerManager.playerCount.Value.ToString(), // + " / 10" 
                Map = gameManager._map
            };
            string url = "https://syncers.io/api/ServerInfo";
            string jsonPayload = JsonUtility.ToJson(ssd); // Convert to JSON

            StartCoroutine(Post(url, jsonPayload));
        }

        yield return new WaitForSeconds(10f);
    }
}




    private IEnumerator CreateSeverDetails() // Creates a non-Edgegap server
    {
        while (true)
        {
            networkManager = FindObjectOfType<NetworkManager>();
            tugboat = networkManager.GetComponent<Tugboat>();
            bayou = networkManager.GetComponent<Bayou>();      // Bayou is only on WebGL NetworkManager
            playerManager = FindObjectOfType<PlayerManager>();
            gameManager = FindObjectOfType<GameManager>();

            // Debug.Log(networkManager);
            // Debug.Log(tugboat);
            // Debug.Log(bayou);
            // Debug.Log(playerManager);
            // Debug.Log(gameManager._gameMode);
            // Debug.Log(gameManager._map);
  

            SentServerDetails ssd = new()
            {
                TugboatPort = tugboat.GetPort().ToString(),
                BayouPort = bayou.GetPort().ToString(),
                Ip = gameManager._ip, // Unused for VPS servers
                Port = gameManager._port, // Unused for VPS servers
                Continent = gameManager._continent, // Unused for VPS servers
                ServerName = gameManager._serverName,
                GameMode = gameManager._gameMode,
                Players = playerManager.playerCount.Value.ToString(), // + " / 10" 
                Map = gameManager._map
            };

        //Debug.Log($"TugboatPort: {ssd.TugboatPort}, BayouPort: {ssd.BayouPort}, GameMode: {ssd.GameMode}, Players: {ssd.Players}");
        // Debug.Log($"Ip : {ssd.Ip}, Port: {ssd.Port}, Continent: { ssd.Continent} ");
        string url = "https://syncers.io/api/ServerInfo";
        string jsonPayload = JsonUtility.ToJson(ssd); // Convert to JSON

        StartCoroutine(Post(url, jsonPayload ));
        
        yield return new WaitForSeconds(10f);

        // SendJsonPostRequest(jsonPayload); // Send the JSON payload as a POST request
        // SendServerDetails(ssd);
        }
    }


    IEnumerator Post(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Add the Authorization header with the password
        string encodedPassword = Convert.ToBase64String(Encoding.ASCII.GetBytes(password));
        request.SetRequestHeader("Authorization", "Basic " + encodedPassword);

        yield return request.SendWebRequest();

        // Debug.Log("Status Code: " + request.responseCode);

        // !!! CARE !!!!ELSE WILL SPAM IF YOU INCLUDE IN BUILD AND INTERRUPT LINUX TERMINAL!!! LOL!!! 
        // if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        //     {
        //         Debug.LogError("Error: " + request.error);
        //     }
        // else
        //     {
        //         Debug.Log("Status Code: " + request.responseCode);
        //     }
    }
            #endif

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


// #else
//     // This code will be included in non-server builds
//     gameObject.SetActive(false);
