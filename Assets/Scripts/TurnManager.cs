﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<ConnectedPlayerManager> playersConnected;
    public int playersTurn = -1;

    void Start() {
        
    }

    void Update() {
        if (playersTurn < 0 && playersConnected.Count > 0) {
            playersTurn = 0;
            Debug.Log("1");
            playersConnected[0].StartTurn();
        }
    }

    public void NextTurn() {
        playersTurn++;
        playersTurn %= playersConnected.Count;
        playersConnected[playersTurn].StartTurn();
    }

    public void UpdatePlayers() {
        ConnectedPlayerManager[] managers = GameObject.FindObjectsOfType<ConnectedPlayerManager>();
        foreach (ConnectedPlayerManager manager in managers) {
            if (!playersConnected.Contains(manager)) {
                playersConnected.Add(manager);
            }
        }
    }
}
