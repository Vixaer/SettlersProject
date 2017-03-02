using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class DiceController
{
    /*
    public static void rollDice(Game currentGame)
    {
        // Generate the rolls for each of the dice
        var eventDiceRoll = generateEventDiceRoll();
        var redDiceRoll = generateStandardDiceRoll();
        var yellowDiceRoll = generateStandardDiceRoll();

        currentGame.setDiceRolls(redDiceRoll, yellowDiceRoll, eventDiceRoll);

        // Handle the event dice
        if (eventDiceRoll == EventKind.Barbarian)
        {
            // TODO: handle the barbarian attacks
        }
        else
        {
            // TODO: distribute progress cards based on event type
        }

        // Handle the regular dice
        if (redDiceRoll + yellowDiceRoll == 7)
        {
            // TODO: resource discarding and robber/pirate
        }
        else
        {
            currentGame.DistributeResources();
        }
        
        // Set the game phase
        if (currentGame.hasOutstandingMoves())
        {
            currentGame.setGamePhase(GamePhase.TurnDiceRolled);
        }
        else
        {
            currentGame.setGamePhase(GamePhase.TurnFirstPhase);
        }
    }*/

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

    private static int generateStandardDiceRoll()
    {
        return new Random().Next(1, 6);
    }
}

