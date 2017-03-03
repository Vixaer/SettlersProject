using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class BuildRoadCommand : NetworkCommand
{
    public Edges location
    {
        get; private set;
    }

    public BuildRoadCommand(NetworkIdentity n, Edges e) : base(n)
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
        if (!player.HasResources(1, ResourceKind.Brick) || !player.HasResources(1, ResourceKind.Lumber))
        {
            // Send some kind fo error to the client
            return;
        }
        player.PayResources(1, ResourceKind.Brick);
        player.PayResources(1, ResourceKind.Lumber);
        var newRoad = new Road(player, this.location);
        // Check the longest road

        // UI
        var newRoadUI = GameObject.Find("UIFactory").GetComponent<UIObjectFactory>().CreateRoad(this.location.gameObject.transform, 1);
        newRoadUI.Init(player, this.location);
    }
}
