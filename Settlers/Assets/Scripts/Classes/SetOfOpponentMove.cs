using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Class representing moves requiring input from >1 other player
/// </summary>
public class SetOfOpponentMove : MultiStepMove
{
    /// <summary>
    /// Boolean for reply received from each player involved
    /// </summary>
    public Dictionary<Player, bool> Interactions { get; private set; }

    public List<Player> Opponents
    {
        get
        {
            return Interactions.Keys.ToList();
        }
    }

    public SetOfOpponentMove(IEnumerable<Player> players)
    {
        this.Interactions = new Dictionary<Player, bool>();
        foreach (Player p in players)
        {
            this.Interactions.Add(p, false);
        }
    }
}

