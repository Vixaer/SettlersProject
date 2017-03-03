using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class MaritimeTradeCommand : NetworkCommand
{
    public ResourceKind provided { get; private set; }
    public ResourceKind requested { get; private set; }

    public MaritimeTradeCommand(NetworkIdentity n, ResourceKind p, ResourceKind r) : base(n)
    {
        this.provided = p;
        this.requested = r;
    }

    // This potentially runs on the server (for game state) and the client (for UI)
    public override void Execute()
    {
        var gameState = GameObject.Find("GameState").GetComponent<Game>();
        var player = gameState.currentPlayer; // gameState.gamePlayers[this.sender];
        if (player != gameState.currentPlayer) return;
        if (this.provided == this.requested) return;
        player.MaritimeTrade(provided, requested);
    }
}
