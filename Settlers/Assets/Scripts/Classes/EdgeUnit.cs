using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class EdgeUnit : OwnableUnit
{
    public Edges locatedAt { get; protected set; }

    public EdgeUnit(Player owner, Edges location) : base(owner)
    {
        this.locatedAt = location;
        location.PlaceUnit(this);
    }
}
