using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public class DiceController
{
    //the random has to be static as if you reset each time its always the same value as the seed remains the same
    //static wont reset it
    static Random roller = new Random();
    static Random yellowDiceRandom = new Random();
    static Random eventDiceRandom = new Random();

    //these 2 are used for gameboard setup best place to keep cuz we need static randoms
    static Random terrainSetter = new Random();
    static Random tokenRoll = new Random();
    static Random harborRoll = new Random();

    ArrayList tilePool = new ArrayList();
    ArrayList tokenPool = new ArrayList();
    ArrayList harborPool = new ArrayList();

    public DiceController()
    {
        for(int i = 0; i<5; i++)
        {
            //added once to the pool
            if (i < 1)
            {
                //add to pool one of each type
                harborPool.Add(HarbourKind.Brick);
                harborPool.Add(HarbourKind.Wool);
                harborPool.Add(HarbourKind.Lumber);
                harborPool.Add(HarbourKind.Grain);
                harborPool.Add(HarbourKind.Ore);
            }
            //added twice
            if (i < 2)
            {
                tokenPool.Add(1);
                tokenPool.Add(12);
                
                tilePool.Add(TerrainKind.GoldMine);
            }
            //basically added 3 times
            if( i < 3)
            {
                tilePool.Add(TerrainKind.Desert);
                tokenPool.Add(3);
                tokenPool.Add(4);
                tokenPool.Add(10);
                tokenPool.Add(11);
                tokenPool.Add(5);
                tokenPool.Add(9);
                tokenPool.Add(6);
                tokenPool.Add(8);
                
            }
            //added 4 times
            if (i < 4)
            {
                harborPool.Add(HarbourKind.Generic);
            }
            //added 5 times
            tilePool.Add(TerrainKind.Hills);
            tilePool.Add(TerrainKind.Mountains);
            tilePool.Add(TerrainKind.Pasture);
            tilePool.Add(TerrainKind.Fields);
            tilePool.Add(TerrainKind.Forest);
        }
        for(int i = 0; i<19; i++)
        {
            tilePool.Add(TerrainKind.Sea);
        }
        

    }

    int redDiceValue, yellowDiceValue, eventDiceValue, terrainKind, tokenValue;
    public void rollDice()
    {
        redDiceValue = roller.Next(1, 7);
        yellowDiceValue = roller.Next(1, 7);
        eventDiceValue = roller.Next(1, 6);
    }
    public void rollTile()
    {
        int tileIndex = roller.Next(0, tilePool.Count);
        int tokenIndex = roller.Next(0, tokenPool.Count);
        if (tilePool.Count > 0 && tokenPool.Count > 0)
        {
            terrainKind = (int)tilePool[tileIndex];
            tokenValue = (int)tokenPool[tokenIndex];
        }
        else
        {
            terrainKind = (int)tilePool[0];
            tokenValue = (int)tokenPool[0];
        }
        //we dont waste token pool on sea and desert;
        if(terrainKind != (int)TerrainKind.Desert && terrainKind != (int)TerrainKind.Sea)
        {
            tokenPool.Remove(tokenValue);
        }
        tilePool.Remove((TerrainKind)terrainKind);
        
    }
    private static EventKind generateEventDiceRoll()
    {
        var randomRoll = new Random().Next(1, 6);
        switch (randomRoll)
        {
            case 4:
                return EventKind.Politics;
            case 5:
                return EventKind.Science;
            case 6:
                return EventKind.Trade;
            default:
                return EventKind.Barbarian;
        }
    }
    public int getRed()
    {
        return redDiceValue;
    }
    public int getYellow()
    {
        return yellowDiceValue;
    }
    public int getEvent()
    {
        return eventDiceValue;
    }

    // Returns the current event role as a kind
    public EventKind getEventKind()
    {
        switch (eventDiceValue)
        {
            case 4:
                return EventKind.Politics;
            case 5:
                return EventKind.Science;
            case 6:
                return EventKind.Trade;
            default:
                return EventKind.Barbarian;
        }
    }

    public int getTerrain()
    {
        return terrainKind;
    }
    public int getToken()
    {
        return tokenValue;
    }
    
    public HarbourKind getHarbour()
    {
        if (harborPool.Count > 0)
        {
            HarbourKind harbourType = (HarbourKind)harborPool[harborRoll.Next(0, harborPool.Count)];
            harborPool.Remove(harbourType);
            return harbourType;
        }
        else
        {
            return HarbourKind.None;
        }
        
    }
}

