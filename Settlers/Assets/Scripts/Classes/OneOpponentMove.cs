using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Class representing move involving a single other player
/// </summary>
public class OneOpponentMove
{
    public Player Opponent { get; private set; }

    public OneOpponentMove(Player p)
    {
        this.Opponent = p;
    }
}

