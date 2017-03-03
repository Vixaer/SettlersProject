using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {

    public Dictionary<CommodityKind, int> cityImprovementLevels { get; set; }
    public Dictionary<ResourceKind, int> resources { get; set; }
    public Dictionary<CommodityKind, int> commodities { get; set; }
    public int gold { get; private set; }
    public List<OwnableUnit> ownedUnits { get; set; }

    // Use this for initialization
    void Start () {
        // Possibly move this code to a constructor
        resources = new Dictionary<ResourceKind, int>()
        {
            { ResourceKind.Brick, 16 },
            { ResourceKind.Grain, 16 },
            { ResourceKind.Lumber, 16 },
            { ResourceKind.Ore, 16 },
            { ResourceKind.Wool, 16 }
        };
        commodities = new Dictionary<CommodityKind, int>()
        {
            { CommodityKind.Cloth, 0 },
            { CommodityKind.Coin, 0 },
            { CommodityKind.Paper, 0 }
        };
        cityImprovementLevels = new Dictionary<CommodityKind, int>()
        {
            { CommodityKind.Cloth, 0 },
            { CommodityKind.Coin, 0 },
            { CommodityKind.Paper, 0 }
        };
        gold = 0;
        ownedUnits = new List<OwnableUnit>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    #region Player attributes
    public void AddResources(int quantity, ResourceKind resourceKind)
    {
        resources[resourceKind] += quantity;
    }

    public void PayResources(int quantity, ResourceKind resourceKind)
    {
        resources[resourceKind] -= quantity;
    }

    public void AddCommodities(int quantity, CommodityKind commodityKind)
    {
        commodities[commodityKind] += quantity;
    }

    public bool HasResources(int quantity, ResourceKind resourceKind)
    {
        return resources[resourceKind] >= quantity;
    }

    public bool HasCommodities(int quantity, CommodityKind commodityKind)
    {
        return commodities[commodityKind] >= quantity;
    }

    public int GetCityImprovementLevel(CommodityKind kind)
    {
        return cityImprovementLevels[kind];
    }

    public void updateGold(int delta)
    {
        this.gold += delta;
    }

    #endregion

    #region GameActions
    public void MaritimeTrade(ResourceKind traded, ResourceKind returned)
    {
        PayResources(4, traded);
        AddResources(1, returned);
    }

    public void UpgradeSettlementToCity(Village settlement, bool playedMedicinePC)
    {
        if (playedMedicinePC)
        {
            PayResources(1, ResourceKind.Grain);
            PayResources(2, ResourceKind.Ore);
        }
        else
        {
            PayResources(2, ResourceKind.Grain);
            PayResources(3, ResourceKind.Ore);
        }
        settlement.setVillageType(VillageKind.City);
    }
    #endregion
}
