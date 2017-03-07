using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Game : NetworkBehaviour
{
    DiceController gameDices = new DiceController();
    public bool waitingForRoad = false;
    public GamePhase currentPhase { get; private set; }

    public Dictionary<GameObject, Player> gamePlayers = new Dictionary<GameObject, Player>();
    //inverse for easier lookup only max 4 values so
    public Dictionary<Player, GameObject> playerObjects = new Dictionary<Player, GameObject>();
    //
    public IEnumerator currentPlayer;
    public GameObject[] reverseOrder;
    public GameObject[] board;
    public GameObject canvas;
    int reverseCount = 0;

    void Start()
    {
        currentPhase = GamePhase.SetupRoundOne;
        setupBoard();
    }

    //setup references for the game
    public void setPlayer(GameObject setPlayer)
    {
        Player temp = new Player();
        gamePlayers.Add(setPlayer, temp);
        playerObjects.Add(temp, setPlayer);
        updatePlayerResourcesUI(setPlayer);
        reverseOrder[reverseCount] = setPlayer;
        reverseCount++;
    }

    public void setPlayerName(GameObject player, string name)
    {
        gamePlayers[player].name = name;
    }

    private void setupBoard()
    {
        bool firstSand = false;
        foreach (GameObject tile in board)
        {
            gameDices.rollTile();
            if (!firstSand)
            {
                if (gameDices.getTerrain() == (int)TerrainKind.Desert)
                {
                    firstSand = true;
                }
                tile.GetComponent<TerrainHex>().setTile(gameDices.getTerrain(), gameDices.getToken());
            }
            else
            {
                tile.GetComponent<TerrainHex>().setTile(gameDices.getNonSand(), gameDices.getToken());
            }

        }

    }

    /// <summary>
    /// Methods used for updating Client UI
    /// </summary>
    /// 

    public void updateTurn()
    {
        IEnumerator keys = (gamePlayers.Keys).GetEnumerator();
        bool remaining = true;
        //set to first player
        keys.MoveNext();
        while (remaining)
        {
            GameObject player = (GameObject)keys.Current;
            string playerTurn =   playerTurn = ((Player)(currentPlayer.Current)).name;
            switch (currentPhase)
            {
                case GamePhase.TurnDiceRolled: playerTurn += " Roll Dice";break;
                case GamePhase.SetupRoundOne: playerTurn += " First Setup"; break;
                case GamePhase.SetupRoundTwo: playerTurn += " Second Setup"; break;
                case GamePhase.TurnFirstPhase: playerTurn += " Build & Trade"; break;
                case GamePhase.TurnSecondPhase: playerTurn += " Build & Trade"; break;

            }
            player.GetComponent<playerControl>().RpcUpdateTurn(playerTurn);
            if (!keys.MoveNext())
            {
                remaining = false;
            }
        }
    }

    public void updatePlayerResourcesUI(GameObject upPlayer)
    {
        Player data;
        gamePlayers.TryGetValue(upPlayer, out data);
        upPlayer.GetComponent<playerControl>().setTextValues(data.resources, data.commodities);

    }

    public void updateRollsUI()
    {
        IEnumerator keys = (gamePlayers.Keys).GetEnumerator();
        bool remaining = true;
        //set to first player
        keys.MoveNext();
        while (remaining)
        {
            GameObject player = (GameObject)keys.Current;
            player.GetComponent<playerControl>().setDiceValues(gameDices.getRed(), gameDices.getYellow(), gameDices.getEvent());
            if (!keys.MoveNext())
            {
                remaining = false;
            }
        }
    }

    public void updatePlayerStatisticsUI()
    {
        int count = 1; int i = 0;
        int[] player1 = new int[8], player2 = new int[8], player3 = new int[8], player4 = new int[8];
        string[] names = new string[gamePlayers.Count];
        IEnumerator values = (gamePlayers.Values).GetEnumerator();
        //set to first player

        while (values.MoveNext() != false)
        {
            Player temp = (Player)values.Current;
            names[i] = temp.name;
            switch (count)
            {
                case 1: player1 = temp.getResourceValues(); break;
                case 2: player2 = temp.getResourceValues(); break;
                case 3: player3 = temp.getResourceValues(); break;
                case 4: player4 = temp.getResourceValues(); break;

            }
            count++;
            i++;
        }
        canvas.GetComponent<statistics>().RpcSetStatistics(player1, player2, player3, player4, names);
    }

    public void chatOnServer(GameObject player, string message)
    {
        IEnumerator keys = (gamePlayers.Keys).GetEnumerator();
        //set to first player

        while (keys.MoveNext() != false)
        {
            GameObject sendToPlayer = (GameObject)keys.Current;
            sendToPlayer.transform.GetComponent<playerControl>().RpcUpdateChat(gamePlayers[player].name + ": " + message + "\n");
        }
    }

    /// <summary>
    /// Methods used for player Actions
    /// </summary>
    /// 
    //selection when aqueduct is used and no resources gained
    public void updateSelection(GameObject player, int value)
    {
        if (value < 5)
        {
            gamePlayers[player].AddResources(1, (ResourceKind)value);
        }
        else
        {
            gamePlayers[player].AddCommodities(1, (CommodityKind)(value - 5));
        }
        updatePlayerResourcesUI(player);

    }
    public void NpcTrade(GameObject player, int offer, int wants)
    {
        bool check = false;
        string log = " has traded 4 ";
        if (checkCorrectPlayer(player) && (currentPhase == GamePhase.TurnFirstPhase || currentPhase == GamePhase.TurnFirstPhase))
        {
            
            //offering resrouce wants  a resource
            if (offer < 5 && wants < 5)
            {
                if (gamePlayers[player].HasResources(4, (ResourceKind)offer))
                {
                    gamePlayers[player].PayResources(4, (ResourceKind)offer);
                    gamePlayers[player].AddResources(1, (ResourceKind)wants);
                    log += ((ResourceKind)offer).ToString() + " for 1 " + ((ResourceKind)wants).ToString();
                    check = true;
                }
            }
            //offering resources wants a commodity
            else if (offer < 5 && wants >= 5)
            {
                if (gamePlayers[player].HasResources(4, (ResourceKind)offer))
                {
                    gamePlayers[player].PayResources(4, (ResourceKind)offer);
                    gamePlayers[player].AddCommodities(1, (CommodityKind)wants - 5);
                    log += ((ResourceKind)offer).ToString() + " for 1 " + ((CommodityKind)wants-5).ToString();
                    check = true;
                }
            }
            //offering commodity wants resource
            else if (offer >= 5 && wants < 5)
            {
                if (gamePlayers[player].HasCommodities(4, (CommodityKind)(offer - 5)))
                {
                    gamePlayers[player].PayCommoditys(4, (CommodityKind)(offer - 5));
                    gamePlayers[player].AddResources(1, (ResourceKind)wants);
                    log += ((CommodityKind)offer-5).ToString() + " for 1 " + ((ResourceKind)wants).ToString();
                    check = true;
                }
            }
            //offering commodity wants commodity
            else if (offer >= 5 && wants >= 5)
            {
                if (gamePlayers[player].HasCommodities(4, (CommodityKind)(offer - 5)))
                {
                    gamePlayers[player].PayCommoditys(4, (CommodityKind)(offer - 5));
                    gamePlayers[player].AddCommodities(1, (CommodityKind)wants - 5);
                    log += ((CommodityKind)offer - 5).ToString() + " for 1 " + ((CommodityKind)wants-5).ToString();
                    check = true;
                }
            }        //update his ui
            updatePlayerResourcesUI(player);
            player.GetComponent<playerControl>().RpcCloseTrade(check);
            // log
            chatOnServer(player, log);
        }

    }

    //buildSettlement ran on server from playerCOntrol class with authority
    //runs the build settlement on the intersection selected by the player
    public void buildSettlement(GameObject player, GameObject intersection)
    {
        bool correctPlayer = checkCorrectPlayer(player);
        bool isOwned = intersection.GetComponent<Intersection>().owned;
        
        //first Phase Spawn settlement
        if (currentPhase == GamePhase.SetupRoundOne)
        {
            if (correctPlayer && !isOwned && !waitingForRoad)
            {
                intersection.GetComponent<Intersection>().CmdBuildSettlement(gamePlayers[player]);

                waitingForRoad = true;
            }
        }
        //second setup spawns City
        else if (currentPhase == GamePhase.SetupRoundTwo)
        {

            //TO-DO check if other settlement is only 2 roads away
            if (correctPlayer && !isOwned && !waitingForRoad && canBuildConnectedCity(gamePlayers[player], intersection))
            {
                intersection.GetComponent<Intersection>().CmdBuildCity(gamePlayers[player]);
                foreach (TerrainHex hex in intersection.GetComponent<Intersection>().linked)
                {
                    payCitySpawn(gamePlayers[player], hex);
                }
                updatePlayerResourcesUI(player);
                waitingForRoad = true;

            }
        }
        //during first phase building
        else if (currentPhase == GamePhase.TurnFirstPhase)
        {
            if (correctPlayer && !isOwned && gamePlayers[player].hasSettlementResources())
            {
                gamePlayers[player].paySettlementResources();
                intersection.GetComponent<Intersection>().CmdBuildSettlement(gamePlayers[player]);
                //update his UI to let him know he lost the resources;
                updatePlayerResourcesUI(player);
            }
        }
        updateTurn();
    }

    //buildRoad ran on server from playerCOntrol class with authority
    //runs the build Road on the Edge selected by the player
    public void buildRoad(GameObject player, GameObject edge)
    {
        bool canBuild = canBuildConnectedRoad(gamePlayers[player], edge);
        //TO-DO check for ship or road to be built and ask when a choice needs to be made or send a variable bool when player has selected ship/road in UI
        bool correctPlayer = checkCorrectPlayer(player);
        bool isOwned = edge.GetComponent<Edges>().owned;

        //first Phase Spawn settlement
        if (currentPhase == GamePhase.SetupRoundOne)
        {
            if (correctPlayer && !isOwned && waitingForRoad && canBuild)
            {
                edge.GetComponent<Edges>().CmdBuildRoad(gamePlayers[player]);
                waitingForRoad = false;
                
                if (!currentPlayer.MoveNext())
                {
                    currentPlayer.Reset();
                    currentPlayer.MoveNext();
                    currentPhase = GamePhase.SetupRoundTwo;
                }
            }
        }
        //second setup spawns City
        else if (currentPhase == GamePhase.SetupRoundTwo)
        {
            if (correctPlayer && !isOwned && waitingForRoad && canBuild)
            {
                edge.GetComponent<Edges>().CmdBuildRoad(gamePlayers[player]);
                waitingForRoad = false;
                
                if (!currentPlayer.MoveNext())
                {
                    currentPlayer.Reset();
                    currentPlayer.MoveNext();
                    currentPhase = GamePhase.TurnDiceRolled;
                    
                }
               
            }
        }
        //during first phase building
        else if (currentPhase == GamePhase.TurnFirstPhase)
        {
            if (correctPlayer && !isOwned && gamePlayers[player].hasRoadResources() && canBuild)
            {
                gamePlayers[player].payRoadResources();
                edge.GetComponent<Edges>().CmdBuildRoad(gamePlayers[player]);
                //update his UI to let him know he lost the resources;
                updatePlayerResourcesUI(player);
            }
        }
        updateTurn();
    }

    public void endTurn(GameObject player)
    {
        if (checkCorrectPlayer(player)) {
            currentPhase = GamePhase.TurnDiceRolled;

            if (!currentPlayer.MoveNext())
            {
                currentPlayer.Reset();
                currentPlayer.MoveNext();
            }
        }
        updateTurn();
    }

    //roll dice and update values and uis
    public void rollDice(GameObject player)
    {
        //check to make sure rolls are done when needed
        if (checkCorrectPlayer(player) && currentPhase == GamePhase.TurnDiceRolled)
        {
            gameDices.rollDice();
            updateRollsUI();
            currentPhase = GamePhase.TurnFirstPhase;
            DistributeResources();
        }
        updateTurn();
    }


    /// <summary>
    /// Methods used for check constraints
    /// </summary>
    /// 
    private bool checkCorrectPlayer(GameObject player)
    {
        if (currentPlayer == null)
        {
            currentPlayer = gamePlayers.Values.GetEnumerator();
            currentPlayer.MoveNext();
        }
        bool check = false;
        if ((currentPlayer.Current).Equals(gamePlayers[player]))
        {
            check = true;
        }
        return check;
    }

    private bool canBuildConnectedRoad(Player player, GameObject edge)
    {
        bool check = false;
        foreach (Intersection i in edge.GetComponent<Edges>().endPoints)
        {
            //check owned true or else owned is null pointer
            if (i.owned && i.positionedUnit.Owner.Equals(player))
            {
                check = true;
                break;
            }
            foreach (Edges e in i.paths)
            {
                //check to see if owned or else bleongs to is obviously null and return null pointer
                if (e.owned && e.belongsTo.Equals(player))
                {
                    check = true;
                    break;
                }
            }
        }
        return check;
    }

    private bool canBuildConnectedCity(Player player, GameObject intersection)
    {
        bool check = true;
        foreach (Edges e in intersection.GetComponent<Intersection>().paths)
        {
            foreach (Intersection i in e.endPoints)
            {

                if (i.owned)
                {
                    check = false;
                    break;
                }
            }
        }
        return check;
    }

    /// <summary>
    /// Method used to distribute resources based on the dice roll
    /// </summary>
    /// 

    //gives correct resources after a diced is rolled on beggining of turn
    public void DistributeResources()
    {
        List<Player> receivingPlayers = new List<Player>();
        int sum = gameDices.getRed() + gameDices.getYellow();
        if (sum != 7)
        {
            foreach (GameObject tile in board)
            {
                if (tile.GetComponent<TerrainHex>().numberToken == sum)
                {
                    foreach (Intersection connected in tile.GetComponent<TerrainHex>().corners)
                    {
                        //check if owned and is a village type as knights dont fucking gain resources
                        if (connected.owned == true && connected.positionedUnit.GetType() == typeof(Village))
                        {
                            Player gainer = connected.positionedUnit.Owner;
                            Village hisVillage = (Village)(connected.positionedUnit);
                            switch (tile.GetComponent<TerrainHex>().myTerrain) {
                                case TerrainKind.Pasture:
                                    {
                                        gainer.AddResources(1, ResourceKind.Wool);
                                        if (hisVillage.myKind != VillageKind.Settlement)
                                        {
                                            gainer.AddCommodities(1, CommodityKind.Cloth);
                                        }
                                        break;
                                    }

                                case TerrainKind.Forest:
                                    {
                                        gainer.AddResources(1, ResourceKind.Lumber);
                                        if (hisVillage.myKind != VillageKind.Settlement)
                                        {
                                            gainer.AddCommodities(1, CommodityKind.Paper);
                                        }
                                        break;
                                    }

                                case TerrainKind.Mountains:
                                    {
                                        gainer.AddResources(1, ResourceKind.Ore);
                                        if (hisVillage.myKind != VillageKind.Settlement)
                                        {
                                            gainer.AddCommodities(1, CommodityKind.Coin);
                                        }
                                        break;
                                    }

                                case TerrainKind.Hills:
                                    {
                                        if (hisVillage.myKind == VillageKind.Settlement)
                                        {
                                            gainer.AddResources(1, ResourceKind.Brick);
                                        }
                                        else
                                        {
                                            gainer.AddResources(2, ResourceKind.Brick);
                                        }
                                        break;
                                    }

                                case TerrainKind.Fields:
                                    {
                                        if (hisVillage.myKind == VillageKind.Settlement)
                                        {
                                            gainer.AddResources(1, ResourceKind.Grain);
                                        }
                                        else
                                        {
                                            gainer.AddResources(2, ResourceKind.Grain);
                                        }
                                        break;
                                    }

                                case TerrainKind.GoldMine:
                                    {
                                        if (hisVillage.myKind == VillageKind.Settlement)
                                        {
                                            gainer.updateGold(1);
                                        }
                                        else
                                        {
                                            gainer.updateGold(2);
                                        }
                                        break;
                                    }
                            }
                            updatePlayerResourcesUI(playerObjects[gainer]);
                            receivingPlayers.Add(gainer);
                        }
                    }

                }

            }
            // Now, for players who didn't receive anything, we check for aqueducts
            IEnumerator values = (gamePlayers.Values).GetEnumerator();


            while (!values.MoveNext())
            {
                Player cur = (Player)values.Current;
                //means he isnt receiving during the roll and has an aqueduct green(3+)
                if (!receivingPlayers.Contains(cur) && cur.GetCityImprovementLevel(CommodityKind.Cloth) >= 3)
                {
                    GameObject selector;
                    playerObjects.TryGetValue(cur, out selector);
                    selector.GetComponent<playerControl>().RpcAskDesiredAquaResource();
                }
            }
        }

        updatePlayerStatisticsUI();
    }

    public void payCitySpawn(Player paidTo, TerrainHex hex)
    {
        switch (hex.myTerrain)
        {
            case TerrainKind.Pasture:
                {
                    paidTo.AddResources(1, ResourceKind.Wool);
                    paidTo.AddCommodities(1, CommodityKind.Cloth);
                    break;
                }

            case TerrainKind.Forest:
                {
                    paidTo.AddResources(1, ResourceKind.Lumber);
                    paidTo.AddCommodities(1, CommodityKind.Paper);
                    break;
                }

            case TerrainKind.Mountains:
                {
                    paidTo.AddResources(1, ResourceKind.Ore);
                    paidTo.AddCommodities(1, CommodityKind.Coin);

                    break;
                }

            case TerrainKind.Hills:
                {
                    paidTo.AddResources(2, ResourceKind.Brick);
                    break;
                }

            case TerrainKind.Fields:
                {
                    paidTo.AddResources(2, ResourceKind.Grain);
                    break;
                }

            case TerrainKind.GoldMine:
                {
                    paidTo.updateGold(2);
                    break;
                }
        }
    }
}
