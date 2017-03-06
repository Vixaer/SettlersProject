using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    int redDiceValue, yellowDiceValue, eventDiceValue, terrainKind, tokenValue;
    public void rollDice()
    {
        redDiceValue = roller.Next(1, 7);
        yellowDiceValue = roller.Next(1, 7);
        eventDiceValue = roller.Next(1, 6);
    }
    public void rollTile()
    {
        terrainKind = roller.Next(0, 8);
        bool getNon7 = false;
        //cant be 7 so
        while (!getNon7)
        {
            tokenValue = roller.Next(1, 13);
            if (tokenValue != 7)
            {
                getNon7 = true;
            }
        }
        
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
    public int getNonSand()
    {
        bool getNonSand = false;
        while (!getNonSand)
        {
            terrainKind = roller.Next(0, 8);
            if (terrainKind != (int)TerrainKind.Desert)
            {
                getNonSand = true;
            }
        }
        return terrainKind;
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
    public int getTerrain()
    {
        return terrainKind;
    }
    public int getToken()
    {
        return tokenValue;
    }

}

