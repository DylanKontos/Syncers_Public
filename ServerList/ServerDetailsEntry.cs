using TMPro;
using UnityEngine;
using FishNet;
using FishNet.Transporting.Bayou;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;

// This class is the ACTUAL object the client clicks in the lobby (offline).
// Server details are received from API
// DefaultScene component of NetworkManager loads correct online scene

public class ServerDetailsEntry : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _serverName = null;

    [SerializeField]
    private TextMeshProUGUI _gameMode = null;

    [SerializeField]
    private TextMeshProUGUI _players = null;

    [SerializeField]
    private TextMeshProUGUI _map = null;

    [SerializeField]
    private TextMeshProUGUI _ping = null;


    public string connectBayouIp;
    public string connectTugboatIp;
    public string connectBayouPort;
    public string connectTugboatPort;
    public bool localIsEdgegap = false;

    Bayou bayou;
    Tugboat tugboat;
    NetworkManager networkManager;

    private void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        bayou = networkManager.GetComponent<Bayou>();      // Bayou is only on WebGL NetworkManager
        tugboat = networkManager.GetComponent<Tugboat>();   // Tugboat is only on WebGL NetworkManager

        if (gameObject.name == "Header") return; // Dont destroy if header
        Destroy(gameObject, 10f);    // Destroy the game object after x seconds
    }

    public string GetContinentAbbreviation(string continent)
    {
        switch (continent)
        {
            case "Europe":
                return "EU";
            case "North America":
                return "NA";
            case "South America":
                return "SA";
            case "South Americas":
                return "SA";
            case "Oceanic":
            case "Oceania":
                return "OC";
            // Add more cases as needed
            default:
                return continent; // Return the original value if no match found
        }
    }

    public void PopulateDetails(string tugboatPort, string bayouPort, string serverName, string gameMode, string players, string map, string continent, string ip, string port)
    {
        string continentAbbr = GetContinentAbbreviation(continent);
 
        if (continent != "Unknown" ) // This is gettintg called for non edgegap deploymnets
        {
            // CLIENT UI VISUAL ONLY
            _serverName.text = continentAbbr.ToString(); // continent.ToString(); 
            _gameMode.text = gameMode.ToString(); 
            _players.text = players.ToString();
            _map.text = map.ToString();

            // _ping.text = clientPing.ToString();
            // playerTargetScene = map.ToString();

            connectTugboatIp = ip; // Tugboat for PC/Android/Mobile
            connectTugboatPort = tugboatPort; // or tugboatPort
            connectBayouIp = ip; // trying an IP for bayouIP...  26/12/24 6:44pm
            connectBayouPort = bayouPort;
            localIsEdgegap = true; // This is gettintg called for non edgegap deploymnets
        }

        else
        {   
            _serverName.text = serverName.ToString(); 
            _gameMode.text = gameMode.ToString(); 
            _players.text = players.ToString();
            _map.text = map.ToString();
            // _ping.text = clientPing.ToString();
            // playerTargetScene = map.ToString();

            
            // IP Set manually in editor before build but TODO update...
            connectBayouPort = bayouPort; // bayouPort isn't displayed, just set so that client can connect using it
            connectTugboatPort = tugboatPort;
        }
    }

    public void OnClick_Button()
    {
        ushort port; // for bayou
        ushort port2; // for tugboat

        // This is gettintg called for non edgegap deploymnets also... not an issue???
        if (localIsEdgegap == true && ushort.TryParse(connectBayouPort, out port) && ushort.TryParse(connectTugboatPort, out port2)) 
        {   
            bayou.SetClientAddress(connectBayouIp); // Always syncers.io
            tugboat.SetClientAddress(connectTugboatIp); // But we do set the Tugboat IP for PC

            //Debug.Log("BayouPortString: " + connectBayouPort + "ushort: " + port );
            //Debug.Log("TugboatPortString: " + connectTugboatPort + "ushort: " + port2 );

            //Debug.Log("TugboatIP " + connectTugboatIp);
            //Debug.Log("BayouIP " + connectBayouIp);

            bayou.SetPort(port);
            tugboat.SetPort(port2);

            InstanceFinder.ClientManager.StartConnection(); // Using default scene component
        }

        // NON-EDGEGAP // NON-EDGEGAP // NON-EDGEGAP // NON-EDGEGAP 
        if (ushort.TryParse(connectBayouPort, out port) && ushort.TryParse(connectTugboatPort, out port2))
        {
            bayou.SetPort(port);
            tugboat.SetPort(port2);

            Debug.Log("OnClick - StartingClientConnection");
            InstanceFinder.ClientManager.StartConnection(); // Using default scene component
        }
    }
}
    // Potential ping Coroutine???
    // private IEnumerator PingServer(string ip, int port)
    // {
    //     Ping p = new Ping(ip + ":" + port.ToString());
    //     while (!p.isDone)
    //         yield return null;

    //     _ping.text = p.time.ToString();
    // }