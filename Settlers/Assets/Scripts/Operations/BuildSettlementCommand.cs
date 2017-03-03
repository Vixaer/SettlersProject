using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class BuildSettlementCommand : NetworkCommand
{
    public Intersection location
    {
        get; private set;
    }

    public BuildSettlementCommand(NetworkIdentity n, Intersection i) : base(n)
    {
        this.location = i;
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
        var player = gameState.currentPlayer; // gameState.gamePlayers[this.sender];
        if (player != gameState.currentPlayer) return;
        if (!(player.HasResources(1, ResourceKind.Brick) && player.HasResources(1, ResourceKind.Lumber) &&
            player.HasResources(1, ResourceKind.Wool) && player.HasResources(1, ResourceKind.Grain)))
        {
            // Send some kind of error to the client
            return;
        }
        player.PayResources(1, ResourceKind.Brick);
        player.PayResources(1, ResourceKind.Lumber);
        player.PayResources(1, ResourceKind.Wool);
        player.PayResources(1, ResourceKind.Grain);
        var newSettlement = new Village(player, this.location);

        // UI
        var newSettlementUI = GameObject.Find("UIFactory").GetComponent<UIObjectFactory>().CreateSettlement(this.location.gameObject.transform, 1);
        newSettlementUI.Init(player, this.location);
    }
}
