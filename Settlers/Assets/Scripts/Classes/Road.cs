using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class Road : EdgeUnit
{
    public Road(Player p, Edges e) : base(p, e)
    {

    }

    /// <summary>
    /// Initializer called by Unity to properly set the values (we won't be calling the constructor)
    /// </summary>
    /// <param name="owner">The player owner of the unit</param>
    /// <param name="location">The intersection location of the unit</param>
    public void Init(Player owner, Edges location)
    {
        this.Owner = owner;
        this.locatedAt = location;
    }
}

