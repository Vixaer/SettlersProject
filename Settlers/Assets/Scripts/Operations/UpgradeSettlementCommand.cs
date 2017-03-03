using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UpgradeSettlementCommand : NetworkCommand
{
    public Village settlement
    {
        get; private set;
    }

    public UpgradeSettlementCommand(NetworkIdentity n, Village v) : base(n)
    {
        this.settlement = v;
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
        if (!player.HasResources(3, ResourceKind.Ore) || !player.HasResources(2, ResourceKind.Grain))
        {
            // Send some kind fo error to the client
            return;
        }
        player.UpgradeSettlementToCity(this.settlement, false);

        // UI
        var newCityUI = GameObject.Find("UIFactory").GetComponent<UIObjectFactory>().CreateCity(this.settlement.locatedAt.gameObject.transform, 1);
        ////newCityUI.Init(player, this.settlement.locatedAt);
        GameObject.Destroy(this.settlement.gameObject);
    }
}
