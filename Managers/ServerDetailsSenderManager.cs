


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerDetailsSenderManager : MonoBehaviour
{
    [SerializeField]
    private GameObject serverDetailsSenderPrefab;

    // Start is called before the first frame update
    void Start()
    {
        #if UNITY_SERVER
        // Instantiate the server details sender manager prefab
        Instantiate(serverDetailsSenderPrefab);
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

