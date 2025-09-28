using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using FishNet.Object;

public class NameDisplayer : NetworkBehaviour            // This is attatched to PlayerObject 
{
    [SerializeField]
    private TextMeshPro playerName;
    public GameObject ParentObject;

    public override void OnStartClient()
    {
        base.OnStartClient();

        SetGraphicalObjectParent(); // Manually set the parent as it sets to RedShip !Bug! Solved :)
    }

    private void SetGraphicalObjectParent()
    {
        Transform graphicalObjectTransform = ParentObject.transform.Find("GraphicalObject");
        if (graphicalObjectTransform != null) { transform.SetParent(graphicalObjectTransform);}
        else { Debug.LogError("GraphicalObject not found as a child of ParentObject."); }
    }

    public void SetName(string newName) 
    { 
        playerName.text = newName;
        ObserversSetName(newName);
    }

    [ObserversRpc(BufferLast = true)]
    public void ObserversSetName(string newName) 
    {
        playerName.text = newName;  // Set the text using the passed parameter
    }
}