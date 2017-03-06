using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class Village : IntersectionUnit
{
    public VillageKind myKind { get; private set; }
    public bool cityWall { get; private set; }
    
    public Village(Player p) : base(p)
    {
        this.myKind = VillageKind.Settlement;
        this.cityWall = false;
    }
    
    public void setVillageType(VillageKind k)
    {
        this.myKind = k;
    }       
}

