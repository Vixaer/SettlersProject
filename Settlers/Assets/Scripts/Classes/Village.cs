using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
public class Village : IntersectionUnit
{
    public VillageKind myKind { get; private set; }
    public bool cityWall { get; private set; }
    
    public Village(Player p, Intersection i) : base(p, i)
    {
        this.myKind = VillageKind.Settlement;
        this.cityWall = false;
    }
    
    /// <summary>
    /// Initializer called by Unity to properly set the values (we won't be calling the constructor)
    /// </summary>
    /// <param name="owner">The player owner of the unit</param>
    /// <param name="location">The intersection location of the unit</param>
    public void Init(Player owner, Intersection location)
    {
        this.Owner = owner;
        this.locatedAt = location;
        this.myKind = VillageKind.Settlement;
        this.cityWall = false;
    }
    
    public void setVillageType(VillageKind k)
    {
        this.myKind = k;
    }

    void OnMouseDown()
    {
        var selectorPanel = GameObject.FindObjectOfType<MapSelectorPanel>();
        if (selectorPanel != null)
        {
            selectorPanel.setSelectedObject(this.gameObject);
        }
    }
}

