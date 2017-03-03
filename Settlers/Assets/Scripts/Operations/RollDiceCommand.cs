using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class RollDiceCommand : NetworkCommand
{
    public RollDiceCommand(NetworkIdentity n) : base(n)
    {
    }

    // This potentially runs on the server (for game state) and the client (for UI)
    public override void Execute()
    {
        var gameState = GameObject.Find("GameState").GetComponent<Game>();
        DiceController.rollDice(gameState);
    }
}
