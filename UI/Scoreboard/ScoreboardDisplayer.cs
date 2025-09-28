using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.InputSystem;

public class ScoreboardDisplayer : NetworkBehaviour
{
    private List<ScoreboardEntry> scoreboardEntries = new List<ScoreboardEntry>();

    [SerializeField]
    private TextMeshProUGUI nameEntryText;

    [SerializeField]
    private TextMeshProUGUI deathsEntryText;

    [SerializeField]
    private TextMeshProUGUI killsEntryText;

    [SerializeField]
    private TextMeshProUGUI pingEntryText;

    PlayerObjectInput playerObjectInput; 
    PlayerInput playerInput;
    Canvas scoreboardCanvas;

    public struct ScoreboardEntry
    {
        public int ObjectId;
        public string playerName;
        public int playerKills;
        public int playerDeaths;
        public long ping;
    }

    private void Awake()
    {
        scoreboardCanvas = GetComponent<Canvas>();
        playerInput = FindObjectOfType<PlayerInput>();
        playerObjectInput = FindObjectOfType<PlayerObjectInput>();
    }


    private void Update()
    {
        ScoreBoardPerformed();

        // foreach (ScoreboardEntry entry in scoreboardEntries)
        // {
        //     Debug.Log($"PlayerName: {entry.playerName}, Kills: {entry.playerKills}, Deaths: {entry.playerDeaths}, ObjectId: {entry.ObjectId}");
        // }
    }

    private void ScoreBoardPerformed()      // Displays scoreboard if <TAB> key held
    {
        bool isScoreboardPerformed = Player.GetScoreboardButtonState();

        if (isScoreboardPerformed == true) {scoreboardCanvas.enabled = true; }
        if (isScoreboardPerformed == false) { scoreboardCanvas.enabled = false; }
    }

    [ObserversRpc]
    public void RefreshScoreboard(List<ScoreboardEntry> entries) 
    {
        string scoreboardTextName = "";
        string scoreboardTextDeaths = "";
        string scoreboardTextKills = "";
        string scoreboardTextPing = "";

        foreach (ScoreboardEntry entry in entries)
        {
            scoreboardTextName += $"{entry.playerName} \n";             
            scoreboardTextDeaths += $"{entry.playerDeaths}\n";
            scoreboardTextKills += $"{entry.playerKills}\n";
            scoreboardTextPing += $"{entry.ping}\n";
        }

        nameEntryText.text = scoreboardTextName;
        deathsEntryText.text = scoreboardTextDeaths;
        killsEntryText.text = scoreboardTextKills;
        pingEntryText.text = scoreboardTextPing;
    }

    public void AddScoreboardEntry(string newName, int kills, int deaths, int objectId, long clientPing)
    {
        if (ContainsPlayerInstance(objectId))
        {
            RemoveDuplicate(objectId);
        }

        ScoreboardEntry entry = new ScoreboardEntry
        {
            playerName = newName,
            playerKills = kills,
            playerDeaths = deaths,
            ObjectId = objectId,  // int we will use to check if duplicates on entries
            ping = clientPing,
        };

        scoreboardEntries.Add(entry);
        RefreshScoreboard(scoreboardEntries);
    }


    private bool ContainsPlayerInstance(int objectId)
    {
        return scoreboardEntries.Exists(entry => entry.ObjectId == objectId); 
    }

    private void RemoveDuplicate(int objectId)
    {
        scoreboardEntries.RemoveAll(entry => entry.ObjectId == objectId); 
    }

    public void RemoveScoreboardEntry(int objectId)
    {
        ScoreboardEntry entryToRemove = scoreboardEntries.Find(entry => entry.ObjectId == objectId);
        scoreboardEntries.Remove(entryToRemove);
        RefreshScoreboard(scoreboardEntries);
    }
}