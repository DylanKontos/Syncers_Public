using UnityEngine;

    // README
    // Step 3 in the server list
    // Create/Instantiate an entry into the client-side and client facing Server List

public class ServerDetailsEntryCreator : MonoBehaviour
{
    [SerializeField] 
    private ServerDetailsEntry entryPrefab;   // "Prefab to spawn for a server entry."

    [SerializeField] 
    private Transform _content; // "Transform to spawn entries under."

    public void PopulateDetails(string tugboatPort, string bayouPort, string serverName, string gameMode, string players, string map, string continent, string ip, string port)
    {
        // Instantiate the entry prefab
        ServerDetailsEntry newEntry = Instantiate(entryPrefab, _content);
        newEntry.PopulateDetails(tugboatPort, bayouPort, serverName, gameMode, players, map, continent, ip, port);
    }
}