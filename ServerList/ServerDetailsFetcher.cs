using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;
using FishNet.Managing.Timing;
using FishNet;

// README
// Step 2 in the server list
// This will GET from the API
// On the CLIENT Network Manager
// From here on is all client facing and client side

public class ServerDetailsFetcher : MonoBehaviour  
{
    private ServerDetailsEntryCreator serverDetailsEntryCreator;
    TimeManager timeManager;

    private List<SentServerDetails> serverDetailsList = new List<SentServerDetails>(); // Declare list to store server details

    private string serverAddress = "149.28.179.225"; // Here we will fetch the CORRECT IP from the Servers.
    // You cant just get the networkmanager ping. You need to get the ping from the API for EACH server...
    private long clientPing;

    public void OnEnable()
    {
        serverDetailsEntryCreator = GetComponent<ServerDetailsEntryCreator>();
        string apiUrl = "https://syncers.io/api/ServerInfo";
        StartCoroutine(PingServerCoroutine());
        StartCoroutine(GetServerDetails(apiUrl));
    }

    public class SentServerDetails
    {
        public string tugboatPort { get; set; }
        public string bayouPort { get; set; }
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Continent { get; set; }
        public string serverName { get; set; }
        public string gameMode { get; set; }
        public string players { get; set; }
        public string map {get; set; }
    }

    IEnumerator PingServerCoroutine()
    {
        while (true)
        {
            long ping;
            TimeManager tm = InstanceFinder.TimeManager;

            if (tm == null)
            {
                ping = 0;
            }

            else
            {
                ping = tm.RoundTripTime;
                clientPing = ping;
            } 
            yield return new WaitForSeconds(5f); // Ping every 5 seconds
        }
    }

    private IEnumerator GetServerDetails(string url)
    {
        while (true) // Loop indefinitely
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();

            try
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonPayload = webRequest.downloadHandler.text;
                    RecieveJsonPayLoad(jsonPayload);
                }
                else
                {
                    Debug.LogError($"Error fetching server details: {webRequest.error}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"An error occurred: {e}");
            }

            yield return new WaitForSeconds(10f);
        }
    }

    public void RecieveJsonPayLoad(string jsonPayload)
    {
        // Debug.Log("Received JSON payload: " + jsonPayload); // logs RAW JSON payload/string

        try
        {
            // Deserialize JSON payload into a list of server details
            var receivedServerDetailsList = JsonConvert.DeserializeObject<List<SentServerDetails>>(jsonPayload);

            // Debug.Log(receivedServerDetailsList);

            // Clear the existing list before populating with new data
            serverDetailsList.Clear();
            
            // Add received server details to the list
            serverDetailsList.AddRange(receivedServerDetailsList);

            // Log the details of all servers
            foreach (SentServerDetails ssd in serverDetailsList)
            {
                serverDetailsEntryCreator.PopulateDetails(
                ssd.tugboatPort,  // Edgegap will still change these
                ssd.bayouPort,  // Edgegap will still change these
                ssd.serverName,
                ssd.gameMode,
                ssd.players,
                ssd.map,
                ssd.Continent ?? "Unknown",        // Edegap Data. If left null, WILL NOT work... and catch e
                ssd.Ip ?? "Unknown",        // Edegap Data. If left null, WILL NOT work... and catch e
                ssd.Port ?? "Unknown"      // Edegap Data. If left null, WILL NOT work... and catch e
            );



                // serverDetailsEntryCreator.PopulateDetails(ssd.tugboatPort, ssd.bayouPort, ssd.Ip, ssd.Port, ssd.Continent, ssd.serverName, ssd.gameMode, ssd.players, ssd.map);
                // Debug.Log($"TugboatPort: {ssd.tugboatPort}, BayouPort: {ssd.bayouPort}, Ip: {ssd.Ip}, Port: {ssd.Port}, Contient: {ssd.Continent}, GameMode: {ssd.gameMode}, GameMode: {ssd.gameMode}, Players: {ssd.players}, Map: {ssd.map}");
            }
        }

        catch (Exception e)
        {
            // Prevents erros from flooding if theres no servers in API...
            Debug.LogError($"An error occurred during deserialization: {e}");
        }
    }
}
            // var serverDetailsList = JsonConvert.DeserializeObject<List<SentServerDetails>>(jsonPayload);
            // SentServerDetails ssd = serverDetailsList.FirstOrDefault(); // Get the first item
            // Debug.Log($"TugboatPort: {ssd.tugboatPort}, BayouPort: {ssd.bayouPort}, GameMode: {ssd.gameMode}, Players: {ssd.players}");

            // public void Update()
            // {
            //     foreach (var entry in serverDetailsDictionary)
            //     {
            //         // Debug.Log(entry);   
            //     }
            // }


            // foreach (SentServerDetails ssd in serverDetailsList)
            // {
            //     if (serverDetailsDictionary.ContainsKey(ssd.tugboatPort)) // Update existing entry
            //     {
            //         serverDetailsDictionary[ssd.tugboatPort] = ssd;
            //         Debug.Log($"TugboatPort: {ssd.tugboatPort}, BayouPort: {ssd.bayouPort}, GameMode: {ssd.gameMode}, Players: {ssd.players}");

            //         serverDetailsEntryCreator.PopulateDetails(ssd.tugboatPort, ssd.bayouPort, ssd.gameMode, ssd.players);
            //     }
            //     else // Add new entry
            //     {
            //         serverDetailsDictionary.Add(ssd.tugboatPort, ssd);
            //         serverDetailsEntryCreator.PopulateDetails(ssd.tugboatPort, ssd.bayouPort, ssd.gameMode, ssd.players);
            //         // Debug.Log(serverDetailsDictionary);
            //     }
            // }


















//         // // Add the new entry to the list
         
            //_entries.Add(newEntry);
//          serverDetailsEntryCreator.PopulateDetails(newEntry.IP, newEntry.Port, newEntry.Name, newEntry.GameMode, newEntry.Players, newEntry.Inactive);




    // public class ServerDetailsUI : MonoBehaviour
    // {
    //     [SerializeField] private TextMeshProUGUI ipText;
    //     [SerializeField] private TextMeshProUGUI portText;
    //     [SerializeField] private TextMeshProUGUI nameText;
    //     // Add more UI components for other details as needed

    //     public void PopulateDetails(string ip, string port, string name)
    //     {
    //         ipText.text = "IP: " + ip;
    //         portText.text = "Port: " + port;
    //         nameText.text = "Name: " + name;
    //         // Populate other UI elements similarly
    //     }
    // }

        // Instantiate(entryPrefab); // Assuming _content is a Transform
        // //Instantiate(newEntry, _content);

//         GameObject entryObject = Instantiate(serverDetailsPrefab, _content);

        // /// <summary>
        // /// Adds a new ServerDetails entry to the canvas.
        // /// </summary>
        // /// <param name="sd"></param>
        // private void AddServerDetailsEntry(ServerDetails sd)
        // {
        //     //Create a new entry and initialize it.
        //     ServerDetailsEntry entry = Instantiate(_entryPrefab, _content);
        //     entry.Initialize(this, sd);
        //     _entries.Add(entry);
        // }



        // foreach (ServerDetailsEntry entry in _entries)
        // {
        //     // Instantiate(serverDetailsPrefab);

        //     // entry.IP = _entry.IP; 

        //     // instead of isntantiating a game object, lets make a seperate list with seperate variables.

        //     Debug.Log("Server Details Entry:");
        //     Debug.Log("IP: " + entry.IP);

        //     Debug.Log("Port: " + entry.Port);
        //     Debug.Log("Name: " + entry.Name);
        //     Debug.Log("GameMode: " + entry.GameMode);
        //     Debug.Log("CurrentPlayers: " + entry.CurrentPlayers);
        //     Debug.Log("MaximumPlayers: " + entry.MaximumPlayers);
        //     Debug.Log("Inactive: " + entry.Inactive);
        // }