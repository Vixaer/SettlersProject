using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class IntersectionUnit : OwnableUnit
{
    public Intersection locatedAt { get; protected set; }

    public IntersectionUnit(Player owner, Intersection location) : base(owner)
    {
        this.locatedAt = location;
        location.PlaceUnit(this);
    }
}
