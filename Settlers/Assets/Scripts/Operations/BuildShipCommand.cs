using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class BuildShipCommand : NetworkCommand
{
    public Edges location
    {
        get; private set;
    }

    public BuildShipCommand(NetworkIdentity n, Edges e) : base(n)
    {
        this.location = e;
    }

    // This potentially runs on the server (for game state) and the client (for UI)
    public override void Execute()
    {
        CmdExecute();
    }

    // This runs on the server
    [Command]
    void CmdExecute()
    {
        var gameState = GameObject.Find("GameState").GetComponent<Game>();
        var player = gameState.currentPlayer; //gameState.gamePlayers[this.sender];
        if (player != gameState.currentPlayer) return;
        if (!player.HasResources(1, ResourceKind.Wool) || !player.HasResources(1, ResourceKind.Lumber))
        {
            // Send some kind of error to the client
            return;
        }
        player.PayResources(1, ResourceKind.Wool);
        player.PayResources(1, ResourceKind.Lumber);
        var newShip = new Ship(player, this.location);
        // Check the longest road

        // UI
        var newShipUI = GameObject.Find("UIFactory").GetComponent<UIObjectFactory>().CreateShip(this.location.gameObject.transform, 1);
        newShipUI.Init(player, this.location);
    }
}
