using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Player {
    public int myColor;
    public static int playerCount = 0;
    public Dictionary<CommodityKind, int> cityImprovementLevels { get; set; }
    public Dictionary<ResourceKind, int> resources { get; set; }
    public Dictionary<CommodityKind, int> commodities { get; set; }
    
    public int gold { get; private set; }
    public int victoryPoints { get; private set; }
    public List<OwnableUnit> ownedUnits { get; set; }
    public List<HarbourKind> ownedHarbour { get; set;}

    public List<VillageKind> settlementPool { get; set; }

    public List<VillageKind> citiesPool { get; set; }

    public List<ProgressCardKind> cardsInHand { get; set; }

    public List<KnightLevel> knightTokens { get; set; }

    public string name;
    public Player() {
        myColor = playerCount;
        playerCount++;
        // Possibly move this code to a constructor
        resources = new Dictionary<ResourceKind, int>()
        {
            { ResourceKind.Brick, 100 },
            { ResourceKind.Grain, 100 },
            { ResourceKind.Lumber, 100 },
            { ResourceKind.Ore, 100 },
            { ResourceKind.Wool, 100 }
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
        ownedHarbour = new List<HarbourKind>();
        citiesPool = new List<VillageKind>();
        settlementPool = new List<VillageKind>();
        cardsInHand = new List<ProgressCardKind>();
        knightTokens = new List<KnightLevel>();

        //add the tokens in the pool
        for(int i = 0; i<4; i++)
        {
            citiesPool.Add(VillageKind.City);
            settlementPool.Add(VillageKind.Settlement);
            if (i < 3)
            {
                //add 3 of each knight that can be built
                knightTokens.Add(KnightLevel.Basic);
                knightTokens.Add(KnightLevel.Mighty);
                knightTokens.Add(KnightLevel.Strong);
            }
        }
	    settlementPool.Add(VillageKind.Settlement);
        victoryPoints = 0;

    }

    #region Player attributes Manipulation
    public void AddResources(int quantity, ResourceKind resourceKind)
    {
        resources[resourceKind] += quantity;
    }
    public void AddCommodities(int quantity, CommodityKind commodityKind)
    {
        commodities[commodityKind] += quantity;
    }
    public void AddGold(int delta)
    {
        this.gold += delta;
    }
    public void AddVictoryPoints(int value)
    {
        victoryPoints += value;
    }

    public void PayResources(int quantity, ResourceKind resourceKind)
    {
        resources[resourceKind] -= quantity;
    }
    public void PayCommoditys(int quantity, CommodityKind commodityKind)
    {
        commodities[commodityKind] -= quantity;
    }
    public void PaySettlementResources()
    {
        PayResources(1, ResourceKind.Grain);
        PayResources(1, ResourceKind.Lumber);
        PayResources(1, ResourceKind.Brick);
        PayResources(1, ResourceKind.Wool);
    }
    public void PayRoadResources()
    {
        PayResources(1, ResourceKind.Lumber);
        PayResources(1, ResourceKind.Brick);
    }
    public void PayShipResources()
    {
        PayResources(1, ResourceKind.Lumber);
        PayResources(1, ResourceKind.Wool);
    }
    public void PayKnightResources()
    {
        PayResources(1, ResourceKind.Wool);
        PayResources(1, ResourceKind.Ore);
    }
    public void PayKnightActivationResources()
    {
        PayResources(1, ResourceKind.Grain);
    }

    public void RemoveCity()
    {
        citiesPool.Remove(VillageKind.City);
    }
    public void RemoveSettlement()
    {
        settlementPool.Remove(VillageKind.Settlement);
    }
    public void RemoveKnight(KnightLevel level)
    {
        knightTokens.Remove(level);
    }

    public void AddCity()
    {
        citiesPool.Add(VillageKind.City);
    }
    public void AddSettlement()
    {
        settlementPool.Add(VillageKind.Settlement);
    }
    public void AddKnight(KnightLevel level)
    {
        knightTokens.Add(level);
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

    public void payCityResources(bool playedMedicinePC)
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
    }
    #endregion

    #region Return
    public bool HasCityUpgradeResources(bool playedMedicinePC)
    {
        if (playedMedicinePC)
        {
            return HasResources(1, ResourceKind.Grain) && HasResources(2, ResourceKind.Ore);
        }
        else
        {
            return HasResources(2, ResourceKind.Grain) && HasResources(3, ResourceKind.Ore);
        }
    }
    //return how many resources a player currently has
    public int sumResources()
    {
        int sum = 0;
        IEnumerator counter = resources.Values.GetEnumerator();
        while (counter.MoveNext())
        {
            sum += (int) counter.Current;
        }
        counter = commodities.Values.GetEnumerator();
        while (counter.MoveNext())
        {
            sum += (int)counter.Current;
        }
        return sum;
    }

    public bool HasResources(int quantity, ResourceKind resourceKind)
    {
        return resources[resourceKind] >= quantity;
    }

    public bool HasCommodities(int quantity, CommodityKind commodityKind)
    {
        return commodities[commodityKind] >= quantity;
    }
    public bool HasSettlementResources()
    {
        if (HasResources(1, ResourceKind.Grain) && HasResources(1, ResourceKind.Lumber) && HasResources(1, ResourceKind.Brick) && HasResources(1, ResourceKind.Wool))
        {
            return true;
        }
        return false;
    }
    public bool HasRoadResources()
    {
        if (HasResources(1, ResourceKind.Lumber) && HasResources(1, ResourceKind.Brick))
        {
            return true;
        }
        return false;
    }

    public bool HasShipResources()
    {
        if (HasResources(1, ResourceKind.Wool) && HasResources(1, ResourceKind.Brick))
        {
            return true;
        }
        return false;
    }
    public bool HasKnightResources()
    {
        return this.HasResources(1, ResourceKind.Wool) && this.HasResources(1, ResourceKind.Ore);
    }
    public bool HasKnightActivatingResources()
    {
        return this.HasResources(1, ResourceKind.Grain);
    }


    public int GetCityImprovementLevel(CommodityKind kind)
    {
        return cityImprovementLevels[kind];
    }


    public int[] getResourceValues()
    {
        int[] resourceValues = new int[8];
        resourceValues[0] = resources[ResourceKind.Brick];
        resourceValues[1] = resources[ResourceKind.Ore];
        resourceValues[2] = resources[ResourceKind.Wool];
        resourceValues[3] = commodities[CommodityKind.Coin];
        resourceValues[4] = resources[ResourceKind.Grain];
        resourceValues[5] = commodities[CommodityKind.Cloth];
        resourceValues[6] = resources[ResourceKind.Lumber];
        resourceValues[7] = commodities[CommodityKind.Paper];

        return resourceValues;
    }

    // Get the number of active knights owned by this player
    public int getActiveKnightCount()
    {
        // TODO: Implement this method
        return 0;
    }

    // Get the number of city villages owned by this player
    public int getCityCount()
    {
        return getCities().Count;
    }

    // Get the total number of metropolises owned by this player of any kind
    public int getMetropolisCount()
    {
        return getMetropolises().Count;
    }

    // get a list of villages that are cities
    public List<Village> getCities()
    {
        List<Village> to_ret = new List<Village>();
        foreach (OwnableUnit own in ownedUnits)
        {
            if (own is Village)
            {
                Village vil = own as Village;
                if (vil.myKind == VillageKind.City)
                    to_ret.Add(vil);
            }
        }

        return to_ret;
    }

    // Get a list of villages that are metropolises
    public List<Village> getMetropolises()
    {
        List<Village> to_ret = new List<Village>();
        foreach (OwnableUnit own in ownedUnits)
        {
            if (own is Village)
            {
                Village vil = own as Village;
                if (vil.myKind == VillageKind.PoliticsMetropole || vil.myKind == VillageKind.ScienceMetropole || vil.myKind == VillageKind.TradeMetropole)
                    to_ret.Add(vil);
            }
        }

        return to_ret;
    }

    public bool HasSettlements()
    {
        if (settlementPool.Contains(VillageKind.Settlement))
        {
            return true;
        }
        return false;
    }

    public bool HasCities()
    {
        if (citiesPool.Contains(VillageKind.City))
        {
            return true;
        }
        return false;
    }
    public bool HasKnights(KnightLevel level)
    {
        return knightTokens.Contains(level);
    }
    #endregion
}
