using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game {

    public EventKind eventDiceRoll { get; private set; }
    public int redDiceRoll { get; private set; }
    public int yellowDiceRoll { get; private set; }
    public GamePhase currentPhase { get; private set; }
    public List<MultiStepMove> currentMultiStepMoves { get; set; }
    public Player[] gamePlayers { get; set; }
    public Player currentPlayer { get; private set; }
    public List<TerrainHex> board { get; set; }

    
    public Game()
    {
        gamePlayers = new Player[4];
    }
    public void setDiceRolls(int red, int yellow, EventKind e)
    {
        this.eventDiceRoll = e;
        this.redDiceRoll = red;
        this.yellowDiceRoll = yellow;
    }

    /// <summary>
    /// Method used to distribute resources based on the dice roll
    /// </summary>
    public void DistributeResources()
    {
        // Get the terrain hexes that produce this turn
        var producingHexes = board.Where(h => h.numberToken == this.redDiceRoll + this.yellowDiceRoll);
        var receivingPlayers = new List<Player>();

        foreach (TerrainHex h in producingHexes)
        {
            foreach (Intersection i in h.corners)
            {
                var unit = i.positionedUnit;
                if (unit != null && unit is Village)
                {
                    var village = unit as Village;
                    var unitOwner = unit.Owner;
                    receivingPlayers.Add(unitOwner);
                    switch(h.myTerrain)
                    {
                        case TerrainKind.Pasture:
                            unitOwner.AddResources(1, ResourceKind.Wool);
                            if (village.myKind != VillageKind.Settlement)
                                unitOwner.AddCommodities(1, CommodityKind.Cloth);
                            break;
                        case TerrainKind.Forest:
                            unitOwner.AddResources(1, ResourceKind.Lumber);
                            if (village.myKind != VillageKind.Settlement)
                                unitOwner.AddCommodities(1, CommodityKind.Paper);
                            break;
                        case TerrainKind.Mountains:
                            unitOwner.AddResources(1, ResourceKind.Ore);
                            if (village.myKind != VillageKind.Settlement)
                                unitOwner.AddCommodities(1, CommodityKind.Coin);
                            break;
                        case TerrainKind.Hills:
                            if (village.myKind == VillageKind.Settlement)
                                unitOwner.AddResources(1, ResourceKind.Brick);
                            else
                                unitOwner.AddResources(2, ResourceKind.Brick);
                            break;
                        case TerrainKind.Fields:
                            if (village.myKind == VillageKind.Settlement)
                                unitOwner.AddResources(1, ResourceKind.Grain);
                            else
                                unitOwner.AddResources(2, ResourceKind.Grain);
                            break;
                        case TerrainKind.GoldMine:
                            if (village.myKind == VillageKind.Settlement)
                                unitOwner.updateGold(1);
                            else
                                unitOwner.updateGold(2);
                            break;
                        default:
                            // Nothing happens
                            break;
                    }
                }
            }
        }

        // Now, for players who didn't receive anything, we check for aqueducts
        foreach (Player p in gamePlayers)
        {
            var hasAqueduct = p.GetCityImprovementLevel(CommodityKind.Cloth) >= 3;
            if (hasAqueduct && !receivingPlayers.Contains(p))
            {
                p.updateGold(2);
            }
        }
    }

    public bool hasOutstandingMoves()
    {
        return currentMultiStepMoves.Count > 0;
    }

    public void setGamePhase(GamePhase phase)
    {
        this.currentPhase = phase;
    }
}
