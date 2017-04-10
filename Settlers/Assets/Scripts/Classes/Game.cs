using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Game : NetworkBehaviour
{
    public const int BARB_ATTACK_POSITION = 7;

    static System.Random rng = new System.Random();

    DiceController gameDices = new DiceController();
    public bool waitingForRoad = false;
    public bool firstBarbAttack = false;
   
    public int barbPosition = 0; // Max 7. Use MoveBarbs()
    public GamePhase currentPhase { get; private set; }

    public Dictionary<GameObject, Player> gamePlayers = new Dictionary<GameObject, Player>();
    //inverse for easier lookup only max 4 values so
    public Dictionary<Player, GameObject> playerObjects = new Dictionary<Player, GameObject>();
    //
    public IEnumerator currentPlayer;
    private string currentPlayerString;
    public Dictionary<GameObject, Player> reverseOrder = new Dictionary<GameObject, Player>();

    //added all the references for easier algorithm writing
    public GameObject[] boardTile;
    public GameObject[] edges;
    public GameObject[] intersections;
    public GameObject canvas;

    //keep track of the tile where the robber is
    public GameObject robberTile,pirateTile,merchantTile,lakeTile;

    public List<ProgressCardKind> CardsInPlay = new List<ProgressCardKind>();

    public bool isLoaded = false;

    private Dictionary<string, Player> tempPlayersByName;
    private VillageKind metropolisType = VillageKind.City;

    #region Initial Setup
    void Start()
    {
        if (!isLoaded)
        {
            currentPhase = GamePhase.SetupRoundOne;
            setupBoard();
        }
        this.pirateTile = null;
        this.merchantTile = null;
    }

    public void ValidateName(GameObject player, string name)
    {
        player.transform.GetComponent<playerControl>().isValidName = !isLoaded || tempPlayersByName.ContainsKey(name);
    }

    //setup references for the game
    public void setPlayer(GameObject setPlayer)
    {
        Player temp = new Player();
        gamePlayers.Add(setPlayer, temp);
        playerObjects.Add(temp, setPlayer);
        updatePlayerResourcesUI(setPlayer);
        reverseOrder = gamePlayers.Reverse().ToDictionary(x => x.Key, x => x.Value);
    }

    public void setPlayerName(GameObject player, string name)
    {
        if (isLoaded)
        {
            var p = tempPlayersByName[name];
            gamePlayers.Add(player, p);
            playerObjects.Add(p, player);
            updatePlayerResourcesUI(player);
            reverseOrder = gamePlayers.Reverse().ToDictionary(x => x.Key, x => x.Value);
            tempPlayersByName.Remove(name);
            if (tempPlayersByName.Count == 0)
            {
                tempPlayersByName = null;
                // All players have joined, we can try setting the current player
                this.currentPlayer = gamePlayers.Values.GetEnumerator();
                currentPlayer.MoveNext();
                while(((Player)currentPlayer.Current).name != currentPlayerString)
                {
                    currentPlayer.MoveNext();
                }
            }
        }
        else
        {
            gamePlayers[player].name = name;
        }
    }

    private void setupBoard()
    {
        bool robberSpawned = false;
        bool lakeSpawned = false;
        int fishToken = 4;
        System.Random temp = new System.Random();
        foreach (GameObject tile in boardTile)
        {
            gameDices.rollTile();
            tile.GetComponent<TerrainHex>().setTile(gameDices.getTerrain(), gameDices.getToken());
            TerrainHex hex = tile.GetComponent<TerrainHex>();
            if (gameDices.getTerrain() == (int)TerrainKind.Desert && !robberSpawned)
            {
                hex.isRobber = true;
                robberTile = tile;
                robberSpawned = true;
            }
            else if (gameDices.getTerrain() == (int)TerrainKind.Desert && robberSpawned && !lakeSpawned)
            {
               hex.isLake = true;
               lakeTile = tile;
               lakeSpawned = true;
            }            
        }
        foreach (GameObject road in edges)
        {
            bool hasSea = false; bool hasLand = false; bool hasProximity = false;
            if (road.GetComponent<Edges>().inBetween.Length == 2)
            {
                foreach (TerrainHex hex in road.GetComponent<Edges>().inBetween)
                {
                    if (hex.myTerrain == TerrainKind.Sea)
                    {
                        hasSea = true;
                    }
                    else
                    {
                        hasLand = true;
                    }
                    foreach (Intersection inter in road.GetComponent<Edges>().endPoints)
                    {
                        if (inter.harbor != HarbourKind.None)
                        {
                            hasProximity = true;
                        }
                    }
                }

            }
            if(road.GetComponent<Edges>().inBetween.Length == 1)
            {
                // check doesnt need to be on edge we automatically assume outside board is sea
                hasSea = true;
                foreach (TerrainHex hex in road.GetComponent<Edges>().inBetween)
                {
                    if(hex.myTerrain != TerrainKind.Sea)
                    {
                        hasLand = true;
                    }
                    foreach (Intersection inter in road.GetComponent<Edges>().endPoints)
                    {
                        if (inter.harbor != HarbourKind.None)
                        {
                            hasProximity = true;
                        }
                    }
                }
            }
            int luck = temp.Next(0, 7);
            if (luck == 0 && hasSea && hasLand && !hasProximity)
            {
                HarbourKind type = gameDices.getHarbour();
                foreach (Intersection inter in road.GetComponent<Edges>().endPoints)
                {
                    inter.harbor = type;
                }
                road.GetComponent<Edges>().setHarborKind(type);
            }

        }
        //fishing docks spawning
        foreach (GameObject tile in boardTile)
        {
            TerrainHex hex = tile.GetComponent<TerrainHex>();

            if (hex.myTerrain == (int)TerrainKind.Sea)
            {
                bool proximity = false;
                bool isBorder = false;
                foreach(Intersection inter in hex.corners)
                {
                    if (inter.isFishingInter)
                    {
                        proximity = true;
                    }
                }
           
                foreach (Intersection inter in hex.corners)
                {
                    if (!hex.hasFishing)
                    {
                        int i = 0;
                        Intersection[] fishingInters = new Intersection[3];
                        bool hasLand = true;
                        fishingInters[i] = inter; i++;
                        foreach (Edges e in inter.paths)
                        {
                            if (e.inBetween.Length == 2 && (hex == e.inBetween[0] || hex == e.inBetween[1]))
                            {
                                //one of the point swill be completely in water
                                if ( (e.inBetween[0].myTerrain == TerrainKind.Sea || e.inBetween[0].isLake) && (e.inBetween[1].myTerrain == TerrainKind.Sea || e.inBetween[1].isLake))
                                {
                                    hasLand = false;
                                }
                                foreach (Intersection inter2 in e.endPoints)
                                {
                                    foreach(TerrainHex hex2 in inter2.linked)
                                    {
                                        if (hex2.hasFishing)
                                        {
                                            proximity = true;
                                        }
                                    }
                                    if (inter2.harbor != HarbourKind.None)
                                    {
                                        proximity = true;
                                    }
                                    else if (!inter2.Equals(inter))
                                    {
                                        fishingInters[i] = inter2;
                                        i++;
                                    }
                                }
                            }
                            else if(e.inBetween.Length == 2)
                            {

                            }
                            else
                            {
                                isBorder = true;
                            }
                            
                        }
                        if (!proximity && hasLand && fishToken < 11 && !isBorder)
                        {
                            fishingInters[0].isFishingInter = true;
                            fishingInters[1].isFishingInter = true;
                            fishingInters[2].isFishingInter = true;
                            hex.hasFishing = true;
                            hex.numberToken = fishToken;
                            fishToken++;
                        }
                    }
                }

            }
        }

    }
    #endregion


    #region UI Updates

    public void updateTurn()
    {
        IEnumerator keys = (gamePlayers.Keys).GetEnumerator();
        bool remaining = true;
        //set to first player
        keys.MoveNext();
        while (remaining)
        {
            GameObject player = (GameObject)keys.Current;
            string playerTurn = playerTurn = ((Player)(currentPlayer.Current)).name;
            switch (currentPhase)
            {
                case GamePhase.TurnDiceRolled: playerTurn += " Roll Dice"; break;
                case GamePhase.SetupRoundOne: playerTurn += " First Setup"; break;
                case GamePhase.SetupRoundTwo: playerTurn += " Second Setup"; break;
                case GamePhase.TurnFirstPhase: playerTurn += " Build & Trade"; break;
                case GamePhase.TurnRobberPirate: playerTurn += " Move Robber or Pirate"; break;

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
        upPlayer.GetComponent<playerControl>().setTextValues(data.resources, data.commodities, data.gold, data.victoryPoints, data.fishTokens);
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
            player.GetComponent<playerControl>().setDiceValues(gameDices.getRed(), gameDices.getYellow(), (int)gameDices.getEventKind());
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

    //normal chat and server wide messages to inform all players, like barbarians have 1 space left to kill you get ready
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

    // Game messages to everyone
    public void broadcastMessage(string message)
    {
        foreach (GameObject recipient in gamePlayers.Keys)
        {
            recipient.transform.GetComponent<playerControl>().RpcUpdateChat("SettlersOfCatan: " + message + "\n");
        }
    }

    //personally sent to the player object that is referenced, used for messaging like you cant roll right now and stuff
    public void logAPlayer(GameObject player, string message)
    {
        player.transform.GetComponent<playerControl>().RpcUpdateChat(message + "\n");
    }

    public void initiateSteal(GameObject player)
    {
        int i = 0;
        string[] names = new string[gamePlayers.Count];
        IEnumerator values = gamePlayers.Values.GetEnumerator();
        while (values.MoveNext())
        {
            Player temp = (Player)values.Current;
            names[i] = temp.name;
            i++;
        }
        player.GetComponent<playerControl>().RpcSetupStealInterface(names);
    }
    #endregion

    #region Game Actions
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
        Player tradingPlayer = gamePlayers[player];
        bool hasSpecial = false;
        bool hasGeneric = false;
        bool hasTradingHouse = false;
        string log = "";

        if (checkCorrectPlayer(player) && currentPhase == GamePhase.TurnFirstPhase)
        {
            //check if he has the special kind of harbor for his trade type
            switch (offer)
            {
                case 0:
                    if (tradingPlayer.ownedHarbour.Contains(HarbourKind.Wool))
                    {
                        hasSpecial = true;
                        
                    }
                    break;
                case 1:
                    if (tradingPlayer.ownedHarbour.Contains(HarbourKind.Lumber))
                    {
                        hasSpecial = true;

                    }
                    break;
                case 2:
                    if (tradingPlayer.ownedHarbour.Contains(HarbourKind.Ore))
                    {
                        hasSpecial = true;
                        
                    }
                    break;
                case 3:
                    if (tradingPlayer.ownedHarbour.Contains(HarbourKind.Brick))
                    {
                        hasSpecial = true;

                    }
                    break;
                case 4:
                    if (tradingPlayer.ownedHarbour.Contains(HarbourKind.Grain))
                    {
                        hasSpecial = true;

                    }
                    break;
                default:
                    break;

            }
            //merchant fleet makes auto 2:1 trade for the duration of the turn
            if (CardsInPlay.Contains(ProgressCardKind.MerchantFleetCard))
            {
                hasSpecial = true;
            }
            if (!hasSpecial && tradingPlayer.ownedHarbour.Contains(HarbourKind.Generic))
            {
                hasGeneric = true;
            }
            if (tradingPlayer.cityImprovementLevels[CommodityKind.Cloth] >= 3)
            {
                hasTradingHouse = true;
            }
            //offering resrouce wants  a resource
            if (offer < 5 && wants < 5)
            {
                //special payment
                if (hasSpecial && tradingPlayer.HasResources(2, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(2, (ResourceKind)offer);
                    log += "Has traded 2 ";
                    check = true;
                }
                //generic
                else if (hasGeneric && tradingPlayer.HasResources(3, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(3, (ResourceKind)offer);
                    log += "Has traded 3 ";
                    check = true;
                }
                //shitty
                else if (tradingPlayer.HasResources(4, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(4, (ResourceKind)offer);
                    log += "Has traded 4 ";
                    check = true;
                }  
                if (check)
                {
                    tradingPlayer.AddResources(1, (ResourceKind)wants);
                    log += ((ResourceKind)offer).ToString() + " for 1 " + ((ResourceKind)wants).ToString();
                }
            }
            //offering resources wants a commodity
            else if (offer < 5 && wants >= 5)
            {
                //special
                if (hasSpecial && tradingPlayer.HasResources(2, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(2, (ResourceKind)offer);
                    log += "Has traded 2 ";
                    check = true;
                }
                //generic
                else if (hasGeneric && tradingPlayer.HasResources(3, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(3, (ResourceKind)offer);
                    log += "Has traded 3 ";
                    check = true;
                }
                //shitty
                else if (tradingPlayer.HasResources(4, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(4, (ResourceKind)offer);
                    log += "Has traded 4 ";
                    check = true;
                }
                if (check)
                {
                    gamePlayers[player].AddCommodities(1, (CommodityKind)wants - 5);
                    log += ((ResourceKind)offer).ToString() + " for 1 " + ((CommodityKind)wants - 5).ToString();
                }
            }
            //offering commodity wants resource
            else if (offer >= 5 && wants < 5)
            {
                if (hasTradingHouse && gamePlayers[player].HasCommodities(2, (CommodityKind)(offer - 5)))
                {
                    gamePlayers[player].PayCommoditys(2, (CommodityKind)(offer - 5));
                    gamePlayers[player].AddResources(1, (ResourceKind)wants);
                    log += "Has traded 2 " + ((CommodityKind)offer - 5).ToString() + " for 1 " + ((ResourceKind)wants).ToString();
                    check = true;
                }
                else if (gamePlayers[player].HasCommodities(4, (CommodityKind)(offer - 5)))
                {

                    gamePlayers[player].PayCommoditys(4, (CommodityKind)(offer - 5));
                    gamePlayers[player].AddResources(1, (ResourceKind)wants);
                    log += "Has traded 4 " +((CommodityKind)offer - 5).ToString() + " for 1 " + ((ResourceKind)wants).ToString();
                    check = true;
                }
            }
            //offering commodity wants commodity
            else if (offer >= 5 && wants >= 5)
            {
                if (hasTradingHouse && gamePlayers[player].HasCommodities(2, (CommodityKind)(offer - 5)))
                {
                    gamePlayers[player].PayCommoditys(2, (CommodityKind)(offer - 5));
                    gamePlayers[player].AddCommodities(1, (CommodityKind)wants - 5);
                    log += "Has Traded 2 " + ((CommodityKind)offer - 5).ToString() + " for 1 " + ((CommodityKind)wants - 5).ToString();
                    check = true;
                }
                else if (gamePlayers[player].HasCommodities(4, (CommodityKind)(offer - 5)))
                {
                    gamePlayers[player].PayCommoditys(4, (CommodityKind)(offer - 5));
                    gamePlayers[player].AddCommodities(1, (CommodityKind)wants - 5);
                    log += "Has Traded 4 " + ((CommodityKind)offer - 5).ToString() + " for 1 " + ((CommodityKind)wants - 5).ToString();
                    check = true;
                }
            }      
            //update his ui
            updatePlayerResourcesUI(player);
            player.GetComponent<playerControl>().RpcCloseTrade(check);
            // log
            chatOnServer(player, log);
        }
        else if(checkCorrectPlayer(player))
        {
            logAPlayer(player, "Please roll dice before performing trade.");
        }
        else
        {
            logAPlayer(player, "Can't trade! It isn't your turn.");
        }

    }

    public void BuyWithGold(GameObject player, int wants)
    {
        Player tradingPlayer = gamePlayers[player];
        string log = "";

        if (checkCorrectPlayer(player) && currentPhase == GamePhase.TurnFirstPhase && tradingPlayer.gold >= 2)
        {
            tradingPlayer.AddGold(-2);
            tradingPlayer.AddResources(1, (ResourceKind)wants);
            updatePlayerResourcesUI(player);
            player.GetComponent<playerControl>().RpcCloseGoldShop(true);
            // log
            chatOnServer(player, log);
        }
        else if (checkCorrectPlayer(player))
        {
            logAPlayer(player, "Please roll dice before performing purchase.");
        }
        else
        {
            logAPlayer(player, "Can't buy! It isn't your turn.");
        }

    }

    //buildSettlement ran on server from playerCOntrol class with authority
    //runs the build settlement on the intersection selected by the player
    public void buildOnIntersection(GameObject player, GameObject intersection)
    {
        Intersection inter = intersection.GetComponent<Intersection>();
        Player currentBuilder = gamePlayers[player];
        bool correctPlayer = checkCorrectPlayer(player);
        bool isOwned = intersection.GetComponent<Intersection>().owned;
        bool canBuild = canBuildConnectedCity(currentBuilder, intersection);
        bool hasSettlements = currentBuilder.HasSettlements();
        bool hasLand = false;

        foreach(TerrainHex tile in intersection.GetComponent<Intersection>().linked)
        {
            if(tile.myTerrain != TerrainKind.Sea)
            {
                hasLand = true;
            }
        }

        if (!correctPlayer)
        {
            logAPlayer(player, "Can't build when it isn't your turn.");
        }
        else if (!hasLand)
        {
            logAPlayer(player, "Can't build in the sea.");
        }
        else if (!canBuild && !isOwned)
        {
            logAPlayer(player, "Not following the distance rule.");
        }
        if (correctPlayer && hasLand)
        {
            //first Phase Spawn settlement
            if (currentPhase == GamePhase.SetupRoundOne && !waitingForRoad && canBuild)
            {
                inter.BuildSettlement(currentBuilder);
                //remove one from the pool
                currentBuilder.RemoveSettlement();
                // Add the victory points
                currentBuilder.AddVictoryPoints(1);
                updatePlayerResourcesUI(player);
                waitingForRoad = true;
            }
            //second setup spawns City
            else if (currentPhase == GamePhase.SetupRoundTwo && !waitingForRoad && canBuild)
            {
                inter.BuildCity(currentBuilder);
                // remove one from the pool
                currentBuilder.RemoveCity();
                // Add the victory points
                currentBuilder.AddVictoryPoints(2);
                foreach (TerrainHex hex in intersection.GetComponent<Intersection>().linked)
                {
                    payCitySpawn(currentBuilder, hex);
                }
                updatePlayerResourcesUI(player);
                waitingForRoad = true;
            }
            else if (currentPhase == GamePhase.TurnFirstPhase)
            {
                //check if empty spot, follow the distance rules and has resources and has not reached the 4 settlemnet cap
                if (currentBuilder.HasSettlementResources() && !isOwned && canBuild)
                {
                    if (currentBuilder.HasSettlements())
                    {
                        currentBuilder.PaySettlementResources();
                        inter.BuildSettlement(currentBuilder);
                        //remove one from the pool
                        currentBuilder.RemoveSettlement();
                        // Add the victory points
                        currentBuilder.AddVictoryPoints(1);
                        //update his UI to let him know he lost the resources;
                        updatePlayerResourcesUI(player);
                        CheckForVictory();
                    }
                    else
                    {
                        logAPlayer(player, "You've reached the 5 settlement cap, try upgrading a city before attempting to place another settlement");
                    }
                    
                }
                else if (isOwned && inter.positionedUnit.Owner.Equals(currentBuilder))
                {
                    // Check that it actually is a settlement
                    var village = inter.positionedUnit as Village;
                    // check for player if he has resources and has actually not reached the 4 city cap
                    if (village != null && village.myKind == VillageKind.Settlement)
                    {
                        bool medCard = false;
                        if (CardsInPlay.Contains(ProgressCardKind.MedicineCard))
                        {
                            medCard = true;
                        }
                        if (!currentBuilder.HasCityUpgradeResources(medCard))
                        {
                            logAPlayer(player, "You're resources are insufficient for upgrading to a city.");
                        }
                        else if (!currentBuilder.HasCities())
                        {
                            logAPlayer(player, "You've reached the cities cap (4).");
                        }
                        else if(currentBuilder.HasCityUpgradeResources(medCard) && currentBuilder.HasCities())
                        {
                            currentBuilder.payCityResources(medCard);
                            inter.UpgradeSettlement(currentBuilder);
                            currentBuilder.RemoveCity();
                            currentBuilder.AddSettlement();
                            // Add the victory points
                            currentBuilder.AddVictoryPoints(1);
                            CardsInPlay.Remove(ProgressCardKind.MedicineCard);
                            //update his UI to let him know he lost the resources;

                            updatePlayerResourcesUI(player);
                            logAPlayer(player, "You upgraded your settlement into a city!");
                            CheckForVictory();
                        }
                       
                    }
                }
            }
            CheckForLongestRoad();
            updateTurn();
        }     
    }

    public void buildKnightOnIntersection(GameObject player, GameObject intersection)
    {
        Intersection inter = intersection.GetComponent<Intersection>();
        Player currentBuilder = gamePlayers[player];
        bool correctPlayer = checkCorrectPlayer(player);
        bool isOwned = intersection.GetComponent<Intersection>().owned;
        bool canBuild = canBuildKnight(currentBuilder, intersection);
        bool hasKnights = currentBuilder.HasKnights(KnightLevel.Basic);
        bool hasLand = false;

        foreach (TerrainHex tile in intersection.GetComponent<Intersection>().linked)
        {
            if (tile.myTerrain != TerrainKind.Sea)
            {
                hasLand = true;
            }
        }

        if (!correctPlayer)
        {
            logAPlayer(player, "Can't build when it isn't your turn.");
        }
        else if (!hasLand)
        {
            logAPlayer(player, "Can't build a Knight in the sea.");
        }
        else if (!canBuild)
        {
            logAPlayer(player, "You need to be connected to your road structure.");
        }
        else
        {
            //if nothing is build hire a knight
            if (!isOwned)
            {
                if (currentPhase == GamePhase.TurnFirstPhase)
                {
                    if (currentBuilder.HasKnightResources())
                    {
                        if (currentBuilder.HasKnights(KnightLevel.Basic))
                        {
                            currentBuilder.PayKnightResources();
                            inter.BuildKnight(currentBuilder);
                            currentBuilder.RemoveKnight(KnightLevel.Basic);
                            //update his UI to let him know he lost the resources;
                            updatePlayerResourcesUI(player);
                        }
                        else
                        {
                            logAPlayer(player, "You've reached the 3 basic Knight cap, try upgrading a kngiht before attempting to hire another knight");
                        }
                    }
                    else
                    {
                        logAPlayer(player, "You need 1 wool and 1 ore to hire a basic knight.");
                    }

                }
                else
                {
                    logAPlayer(player, "You can't hire knights on this phase.");
                }
            }
            //check for activation or upgrading
            else if (isOwned && inter.positionedUnit.Owner.Equals(currentBuilder))
            {
                if (currentPhase == GamePhase.TurnFirstPhase)
                {
                    // Check that it actually is a knight
                    var knight = inter.positionedUnit as Knight;
                    // Upgrading knight
                    if (knight != null && knight.isKnightActive())
                    {
                        if (!currentBuilder.HasKnightResources())
                        {
                            logAPlayer(player, "You're resources are insufficient for upgrading this Knight.");
                        }
                        else if (knight.level == KnightLevel.Mighty)
                        {
                            logAPlayer(player, "Can't upgrade further he's already the mightiest.");
                        }
                        else if (knight.level == KnightLevel.Basic)
                        {
                            if (currentBuilder.HasKnights(KnightLevel.Strong))
                            {
                                currentBuilder.PayKnightResources();
                                knight.upgradeKnight();
                                inter.knight = KnightLevel.Strong;
                                currentBuilder.AddKnight(KnightLevel.Basic);
                                currentBuilder.RemoveKnight(KnightLevel.Strong);
                                updatePlayerResourcesUI(player);
                            }
                            else
                            {
                                logAPlayer(player, "Reached the strong cap(3) upgrade a strong knight before placing another.");
                            }
                            
                        }
                        else if (knight.level == KnightLevel.Strong)
                        {
                            if (currentBuilder.cityImprovementLevels[CommodityKind.Coin] < 3)
                            {
                                logAPlayer(player, "You need a fortress to create mighty knights.");
                            }
                            else if (currentBuilder.HasKnights(KnightLevel.Mighty))
                            {
                                currentBuilder.PayKnightResources();
                                knight.upgradeKnight();
                                inter.knight = KnightLevel.Mighty;
                                currentBuilder.AddKnight(KnightLevel.Strong);
                                currentBuilder.RemoveKnight(KnightLevel.Mighty);
                                updatePlayerResourcesUI(player);
                            }
                            else
                            {
                                logAPlayer(player, "Reached the Mighty cap(3), you can't upgrade strongs anymore.");
                            }
                        }
                    }
                    //activation
                    else if (knight != null && !knight.isKnightActive())
                    {
                        if (!currentBuilder.HasKnightActivatingResources())
                        {
                            logAPlayer(player, "You're resources are insufficient to activate this Knight.");
                        }
                        else
                        {
                            currentBuilder.PayKnightActivationResources();
                            knight.activateKnight();
                            inter.knightActive = true;
                            updatePlayerResourcesUI(player);
                            logAPlayer(player, "You have activated this knight.");
                        }
                    }
                }
                else
                {
                    logAPlayer(player, "You can't upgrade or activate knights in this phase.");
                }

            }
            else
            {
                logAPlayer(player, "This Place is already occupied by something else.");
            }
            CheckForLongestRoad();
            updateTurn();
        }
    }
    //buildRoad ran on server from playerCOntrol class with authority
    //runs the build Road on the Edge selected by the player
    public void buildRoad(GameObject player, GameObject edge)
    {
        bool correctPlayer = checkCorrectPlayer(player);
        bool canBuild = canBuildConnectedRoad(gamePlayers[player], edge);
        bool onLand = false;
        bool isOwned = edge.GetComponent<Edges>().owned;
        foreach (TerrainHex tile in edge.GetComponent<Edges>().inBetween)
        {
            if (tile.myTerrain != TerrainKind.Sea)
            {
                onLand = true;
            }
        }
        if (!correctPlayer)
        {
            logAPlayer(player, "It isn't your turn.");
        }
        else if (!onLand)
        {
            logAPlayer(player, "You cant build a road in the sea.");
        }
        else if (isOwned)
        {
            logAPlayer(player, "There's already something built here.");
        }
        else if (!canBuild && currentPhase == GamePhase.TurnFirstPhase)
        {
            logAPlayer(player, "The road you are trying to build isn't connected");
        }

        if (correctPlayer && onLand && !isOwned && canBuild)
        {
            //first Phase Spawn settlement
            if (currentPhase == GamePhase.SetupRoundOne && waitingForRoad)
            {
                edge.GetComponent<Edges>().BuildRoad(gamePlayers[player]);
                waitingForRoad = false;

                if (!currentPlayer.MoveNext())
                {
                    currentPlayer = reverseOrder.Values.GetEnumerator();
                    //currentPlayer.Reset();
                    currentPlayer.MoveNext();
                    currentPhase = GamePhase.SetupRoundTwo;
                }
            }
            //second setup spawns City
            else if (currentPhase == GamePhase.SetupRoundTwo && waitingForRoad)
            {
                edge.GetComponent<Edges>().BuildRoad(gamePlayers[player]);
                waitingForRoad = false;

                if (!currentPlayer.MoveNext())
                {
                    //currentPlayer.Reset();
                    currentPlayer = gamePlayers.Values.GetEnumerator();
                    currentPlayer.MoveNext();
                    currentPhase = GamePhase.TurnDiceRolled;

                }
            }
            else if(currentPhase == GamePhase.TurnFirstPhase && CardsInPlay.Contains(ProgressCardKind.RoadBuildingCard))
            {
                edge.GetComponent<Edges>().BuildRoad(gamePlayers[player]);
                CardsInPlay.Remove(ProgressCardKind.RoadBuildingCard);
                logAPlayer(player, "You built a free road because of the Road Building Card.");
            }
            //during first phase building
            else if (currentPhase == GamePhase.TurnFirstPhase && gamePlayers[player].HasRoadResources())
            {
                gamePlayers[player].PayRoadResources();
                edge.GetComponent<Edges>().BuildRoad(gamePlayers[player]);
            }
            CheckForLongestRoad();
            updatePlayerResourcesUI(player);
            updateTurn();
        }
    }

    //buildShip on edges
    public void buildShip(GameObject player, GameObject edge)
    {
        bool correctPlayer = checkCorrectPlayer(player);
        bool canBuild = canBuildConnectedShip(gamePlayers[player], edge);
        bool onWater = false;
        bool isOwned = edge.GetComponent<Edges>().owned;
        foreach(TerrainHex tile in edge.GetComponent<Edges>().inBetween)
        {
            if(tile.myTerrain == TerrainKind.Sea)
            {
                onWater = true;
            }
        }
        if (!correctPlayer)
        {
            logAPlayer(player, "It isn't your turn.");
        }
        else if (!onWater)
        {
            logAPlayer(player, "You cant build a ship on land.");
        }
        else if (isOwned)
        {
            logAPlayer(player, "There's already something built here.");
        }
        else if (!canBuild && currentPhase == GamePhase.TurnFirstPhase)
        {
            logAPlayer(player, "The ship you are trying to build isn't connected.");
        }
        
        if(correctPlayer && onWater && !isOwned && canBuild)
        {
            //first Phase Spawn settlement
            if (currentPhase == GamePhase.SetupRoundOne && waitingForRoad)
            {
                    edge.GetComponent<Edges>().BuildShip(gamePlayers[player]);
                    waitingForRoad = false;

                    if (!currentPlayer.MoveNext())
                    {
                        currentPlayer = reverseOrder.Values.GetEnumerator();
                        //currentPlayer.Reset();
                        currentPlayer.MoveNext();
                        currentPhase = GamePhase.SetupRoundTwo;
                    }
            }
            //second setup spawns City
            else if (currentPhase == GamePhase.SetupRoundTwo && waitingForRoad)
            {
                    edge.GetComponent<Edges>().BuildShip(gamePlayers[player]);
                    waitingForRoad = false;

                    if (!currentPlayer.MoveNext())
                    {
                        //currentPlayer.Reset();
                        currentPlayer = gamePlayers.Values.GetEnumerator();
                        currentPlayer.MoveNext();
                        currentPhase = GamePhase.TurnDiceRolled;

                    }
            }
            //check to see if the road card was built
            else if (currentPhase == GamePhase.TurnFirstPhase && CardsInPlay.Contains(ProgressCardKind.RoadBuildingCard))
            {
                edge.GetComponent<Edges>().BuildShip(gamePlayers[player]);
                CardsInPlay.Remove(ProgressCardKind.RoadBuildingCard);
                logAPlayer(player, "You built a free ship because of the Road Building Card."); 
            }
            //during first phase building
            else if (currentPhase == GamePhase.TurnFirstPhase && gamePlayers[player].HasShipResources())
            {
                gamePlayers[player].PayShipResources();
                edge.GetComponent<Edges>().BuildShip(gamePlayers[player]);
                //update his UI to let him know he lost the resources;
            }
            CheckForLongestRoad();
            updatePlayerResourcesUI(player);
            updateTurn();
        } 
    }

    
    //end player turn
    public void endTurn(GameObject player)
    {
        
        if (checkCorrectPlayer(player) && currentPhase != GamePhase.TurnRobberPirate)
        {
            if(currentPhase != GamePhase.TurnDiceRolled)
            {
                currentPhase = GamePhase.TurnDiceRolled;

                if (!currentPlayer.MoveNext())
                {
                    currentPlayer.Reset();
                    currentPlayer.MoveNext();
                }
                //remove all remaining cards
                CardsInPlay.Clear();
            }
            
        }
        else
        {
            logAPlayer(player, "Move the robber before ending your turn.");
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
            HandleEventDice(); // Handle the outcome of the event dice
            if (gameDices.getRed() + gameDices.getYellow() == 7 && firstBarbAttack)
            {
                currentPhase = GamePhase.TurnRobberPirate;
                //SEND ClientRpc to discard correct amount;
                robberDiscarding();

            }
            else
            {
                currentPhase = GamePhase.TurnFirstPhase;
            }
            DistributeResources();
        }
        else
        {
            logAPlayer(player, "You've already rolled this turn");
        }
        updateTurn();
    }

    //allows client to actually move the robber
    public void moveRobber(GameObject player, GameObject tile)
    {
        //TO-DO add constraint for first barbarian attack when they will be implemented
        if(currentPhase == GamePhase.TurnRobberPirate && checkCorrectPlayer(player))
        {
            if (tile.GetComponent<TerrainHex>().isRobber == true)
            {
                logAPlayer(player, "You can't reselect the same hextile");
            }
            else
            {
                robberTile.GetComponent<TerrainHex>().isRobber = false;
                robberTile = tile;
                tile.GetComponent<TerrainHex>().isRobber = true;
                currentPhase = GamePhase.TurnFirstPhase;
                updateTurn();
            }
        }
    }

    public void resetRobber(GameObject player)
    {
        if (checkCorrectPlayer(player))
        {
            if (currentPhase == GamePhase.TurnFirstPhase)
            {
                if (robberTile.GetComponent<TerrainHex>().isRobber)
                {
                    Player tempPlay = gamePlayers[player];
                    robberTile.GetComponent<TerrainHex>().isRobber = false;
                    tempPlay.PayFishTokens(2);
                    updatePlayerResourcesUI(player);
                }
                else
                {
                    logAPlayer(player, "Robber is already off the board");
                }
            }
            else
            {
                logAPlayer(player, "You must be in the build/trade phase.");
            }
            
        }
        else
        {
            logAPlayer(player, "Wait your turn.");
        }
        
    }
    public void movePirate(GameObject player, GameObject tile)
    {
        //TO-DO
        //TO-DO add constraint for first barbarian attack when they will be implemented
        if (currentPhase == GamePhase.TurnRobberPirate && checkCorrectPlayer(player))
        {
            if (tile.GetComponent<TerrainHex>().isPirate == true)
            {
                logAPlayer(player, "You can't reselect the same hextile");
            }
            else
            {
                if(pirateTile != null)
                {
                    pirateTile.GetComponent<TerrainHex>().isPirate = false;
                } 
                pirateTile = tile;
                tile.GetComponent<TerrainHex>().isPirate = true;
                currentPhase = GamePhase.TurnFirstPhase;
                updateTurn();
            }
        }
    }
    public void resetPirate(GameObject player)
    { 

        if (checkCorrectPlayer(player))
        {
            if (currentPhase == GamePhase.TurnFirstPhase)
            {

                if (pirateTile.GetComponent<TerrainHex>().isPirate)
                {
                    Player tempPlay = gamePlayers[player];
                    robberTile.GetComponent<TerrainHex>().isPirate = false;
                    tempPlay.PayFishTokens(2);
                    updatePlayerResourcesUI(player);
                }
                else
                {
                    logAPlayer(player, "Pirate is already off the board.");
                }
            }
            else
            {
                logAPlayer(player, "You must be in the build/trade phase.");
            }
        }
        else
        {
            logAPlayer(player, "Wait your turn.");
        }
    }
    public void playCard(GameObject player, ProgressCardKind k)
    {
        bool rightPlayer = checkCorrectPlayer(player);
        Player cardPlayer = gamePlayers[player];
        if (rightPlayer)
        {
            switch (k)
            {
                case ProgressCardKind.AlchemistCard:
                    {
                        if (currentPhase == GamePhase.TurnDiceRolled)
                        {
                            // command to choose dice rolls
                            cardPlayer.cardsInHand.Remove(k);
                            gameDices.returnCard(k);
                        }
                        else
                        {
                            logAPlayer(player, "Can only play this alchemist card before rolling due to it's effect.");
                        }
                        break;
                    }
                case ProgressCardKind.CraneCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.EngineerCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.InventorCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.IrrigationCard:
                    {
                        int sum = 0;
                        foreach(GameObject tile in boardTile)
                        {
                            if(tile.GetComponent<TerrainHex>().myTerrain == TerrainKind.Fields)
                            {
                                foreach (Intersection inter in tile.GetComponent<TerrainHex>().corners)
                                {
                                    if (inter.owned && inter.positionedUnit.Owner.Equals(player))
                                    {
                                        cardPlayer.AddResources(2, ResourceKind.Grain);
                                        sum += 2;
                                        break;
                                    }
                                }
                            }
                            
                        }
                        updatePlayerResourcesUI(player);
                        player.GetComponent<playerControl>().RpcRemoveProgressCard(k);
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        logAPlayer(player, "The Irrigation card has given you : " + sum + " grain.");
                        break;
                    }
                case ProgressCardKind.MedicineCard:
                    {
                        if(cardPlayer.HasCities() && cardPlayer.HasCityUpgradeResources(true))
                        {
                            CardsInPlay.Add(k);
                            player.GetComponent<playerControl>().RpcRemoveProgressCard(k);
                            cardPlayer.cardsInHand.Remove(k);
                            gameDices.returnCard(k);
                        }
                        else
                        {
                            logAPlayer(player, "You will waste the card as you can't build due to lack of resources or city cap.");
                        }
                        break;
                    }
                case ProgressCardKind.MiningCard:
                    {
                        int sum = 0;
                        foreach (GameObject tile in boardTile)
                        {
                            if (tile.GetComponent<TerrainHex>().myTerrain == TerrainKind.Mountains)
                            {
                                foreach (Intersection inter in tile.GetComponent<TerrainHex>().corners)
                                {
                                    if (inter.owned && inter.positionedUnit.Owner.Equals(player))
                                    {
                                        cardPlayer.AddResources(2, ResourceKind.Ore);
                                        sum += 2;
                                        break;
                                    }
                                }
                            }

                        }
                        updatePlayerResourcesUI(player);
                        player.GetComponent<playerControl>().RpcRemoveProgressCard(k);
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        logAPlayer(player, "The Mining card has given you : " + sum + " ore.");
                        break;
                    }
                case ProgressCardKind.PrinterCard:
                    {
                        cardPlayer.AddVictoryPoints(1);
                        updatePlayerResourcesUI(player);
                        break;
                    }
                case ProgressCardKind.RoadBuildingCard:
                    {
                        //almost like 2 bools but will be removed when road is built.
                        CardsInPlay.Add(k);
                        CardsInPlay.Add(k);
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.SmithCard:
                    {
                        //to bool checks for when promoting a knight
                        CardsInPlay.Add(k);
                        CardsInPlay.Add(k);
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }

                case ProgressCardKind.BishopCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
				case ProgressCardKind.ConstitutionCard:
					{
						cardPlayer.AddVictoryPoints (1);
						updatePlayerResourcesUI(player);
						break;
					}
                case ProgressCardKind.DeserterCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.DiplomatCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.IntrigueCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.SaboteurCard:
                    {
                        IEnumerator keys = gamePlayers.Keys.GetEnumerator();
                        while (keys.MoveNext())
                        {
                            Player temp = (Player)keys.Current;
                            if(!temp.Equals(cardPlayer) && temp.victoryPoints >= cardPlayer.victoryPoints)
                            {
                                //send the discard request to all involved players
                                playerObjects[temp].GetComponent<playerControl>().RpcDiscardTime((int)(temp.SumResources() / 2.0), 
                                    cardPlayer.name +": has played the saboteur card and you must discard some cards.");
                            }
                        }
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.SpyCard: break;
                case ProgressCardKind.WarlordCard:
                    {
                        List<OwnableUnit> units = cardPlayer.ownedUnits;
                        foreach(OwnableUnit unit in units)
                        {
                            if(unit is Knight)
                            {
                                ((Knight)unit).activateKnight();
                            }
                        }
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.WeddingCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.ComercialHarborCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.MasterMerchantCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.MerchantCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        // See if someone owned the merchant before
                        var merchantPlayer = gamePlayers.Values.FirstOrDefault(p => p.hasMerchant);
                        if (merchantPlayer != null)
                        {
                            merchantPlayer.TakeMerchant();
                            updatePlayerResourcesUI(playerObjects[merchantPlayer]);
                        }
                        cardPlayer.GiveMerchant();
                        updatePlayerResourcesUI(player);
                        CheckForVictory();
                        // TODO: place the merchant where you want it.
                        break;
                    }
                case ProgressCardKind.MerchantFleetCard:
                    {
                        CardsInPlay.Add(k);
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.ResourceMonopolyCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
                case ProgressCardKind.TradeMonopolyCard:
                    {
                        cardPlayer.cardsInHand.Remove(k);
                        gameDices.returnCard(k);
                        break;
                    }
            }
            CheckForVictory();
        }
        else
        {
            logAPlayer(player, "Can't play cards when it isn't your turn.");
        }
    }
    
    public void improveCity(GameObject player, int kind)
    {
        bool turnCheck = checkCorrectPlayer(player);
        bool hasCity = false;
        bool hasMetropolis = false;
        var mapper = new Dictionary<CommodityKind, VillageKind>()
                {
                    { CommodityKind.Cloth, VillageKind.TradeMetropole },
                    { CommodityKind.Coin, VillageKind.PoliticsMetropole },
                    { CommodityKind.Paper, VillageKind.ScienceMetropole }
                };
        Player currentUpgrader = gamePlayers[player];
        foreach (OwnableUnit unit in currentUpgrader.ownedUnits)
        {
            if (unit is Village && ((Village)unit).myKind == VillageKind.City)
            {
                hasCity = true;
                break;
            }
            else if (unit is Village && ((Village)unit).myKind == mapper[(CommodityKind)kind])
            {
                hasMetropolis = true;
                break;
            }
        }
        if (!turnCheck)
        {
            logAPlayer(player, "Not your turn!");
        }
        else if (currentPhase != GamePhase.TurnFirstPhase)
        {
            logAPlayer(player, "Can't improve cities on this phase!");
        }
        else if (!hasCity && !hasMetropolis)
        {
            logAPlayer(player, "Can't upgrade without a city. Get a city first!");
        }
        else
        {        
            int level = currentUpgrader.GetCityImprovementLevel((CommodityKind)kind);
            if(level == 5)
            {
                logAPlayer(player, "Your improvement level in this category is MAXED!");
            }
            else if (!currentUpgrader.HasCommodities(level + 1, (CommodityKind)kind))
            {
                logAPlayer(player, "You dont have the Commodities to upgrade you need " + (level+1) + " of " + ((CommodityKind)kind).ToString() + ".");
             
            }
            else
            {
                currentUpgrader.improveCity((CommodityKind)kind);
                logAPlayer(player, "You just improved your cities!");
                player.GetComponent<playerControl>().RpcUpdateSliders(level + 1, kind);
                updatePlayerResourcesUI(player);
                // Check if we are now creating a metropolis
                if (currentUpgrader.cityImprovementLevels[(CommodityKind)kind] >= 4)
                {
                    GameObject metropolis;
                    switch (kind)
                    {
                        case (int)CommodityKind.Cloth:
                            metropolis = intersections.FirstOrDefault(i => i.GetComponent<Intersection>().positionedUnit is Village && ((Village)i.GetComponent<Intersection>().positionedUnit).myKind == VillageKind.TradeMetropole);
                            break;

                        case (int)CommodityKind.Coin:
                            metropolis = intersections.FirstOrDefault(i => i.GetComponent<Intersection>().positionedUnit is Village && ((Village)i.GetComponent<Intersection>().positionedUnit).myKind == VillageKind.PoliticsMetropole);
                            break;

                        case (int)CommodityKind.Paper:
                            metropolis = intersections.FirstOrDefault(i => i.GetComponent<Intersection>().positionedUnit is Village && ((Village)i.GetComponent<Intersection>().positionedUnit).myKind == VillageKind.ScienceMetropole);
                            break;

                        default:
                            updatePlayerResourcesUI(player);
                            return;
                    }
                    if (metropolis == null || metropolis.GetComponent<Intersection>().positionedUnit.Owner.cityImprovementLevels[(CommodityKind)kind] < currentUpgrader.cityImprovementLevels[(CommodityKind)kind])
                    {
                        // You now have the metropolis of this type
                        if (metropolis != null && metropolis.GetComponent<Intersection>().positionedUnit.Owner != currentUpgrader)
                        {
                            metropolis.GetComponent<Intersection>().metropolis = VillageKind.City;
                            metropolis.GetComponent<Intersection>().positionedUnit.Owner.AddVictoryPoints(-2);
                            updatePlayerResourcesUI(playerObjects[metropolis.GetComponent<Intersection>().positionedUnit.Owner]);
                        }
                        // Call the RPC for the new player to set their metropolis
                        logAPlayer(player, "Select a city to upgrade to a metropolis.");
                        metropolisType = mapper[(CommodityKind)kind];
                        player.GetComponent<playerControl>().RpcBeginMetropoleChoice();
                    }
                }
            }
        }     
    }

    public void setMetropole(GameObject player, GameObject intersection)
    {
        var cityUnit = intersection.GetComponent<Intersection>().positionedUnit;
        if (cityUnit != null)
        {
            var city = cityUnit as Village;
            if (city == null)
            {
                logAPlayer(player, "You must select a village.");
                return;
            }
            else if (city.Owner != gamePlayers[player])
            {
                logAPlayer(player, "You must select a city that you own.");
                return;
            }
            else if (city.myKind != VillageKind.City)
            {
                logAPlayer(player, "You must select a city.");
                return;
            }
            city.setVillageType(metropolisType);
            intersection.GetComponent<Intersection>().metropolis = metropolisType;
            gamePlayers[player].AddVictoryPoints(2);
            updatePlayerResourcesUI(player);
            logAPlayer(player, "Your city has become a metropolis!");
            metropolisType = VillageKind.City;
            player.GetComponent<playerControl>().RpcEndMetropoleChoice();
        }
    }

    public void stealPlayer(GameObject player,string name)
    {
        if (checkCorrectPlayer(player))
        {
            if (currentPhase == GamePhase.TurnFirstPhase)
            {
                IEnumerator values = gamePlayers.Values.GetEnumerator();
                Player receiver = (Player)gamePlayers[player];
                Player victim = null;
                bool stolen = true;
                while (values.MoveNext())
                {
                    Player temp = (Player)values.Current;
                    if (temp.name.Equals(name) && !receiver.name.Equals(name))
                    {
                        victim = temp;
                        break;
                    }
                }
                while (stolen)
                {
                    System.Random luck = new System.Random();
                    int roll = luck.Next(0, 2);
                    if (roll == 0)
                    {
                        int roll2 = luck.Next(0, 5);
                        bool has = victim.HasResources(1, (ResourceKind)roll2);
                        if (has)
                        {
                            victim.PayResources(1, (ResourceKind)roll2);
                            receiver.AddResources(1, (ResourceKind)roll2);
                            receiver.PayFishTokens(3);
                            updatePlayerResourcesUI(playerObjects[victim]);                       
                            updatePlayerResourcesUI(player);
                            player.GetComponent<playerControl>().RpcEndStealInterface();
                            logAPlayer(player, "you stole 1 " + (ResourceKind)roll2 + " from " + victim.name + ".");
                            logAPlayer(playerObjects[victim], receiver.name + "stole 1 " + (ResourceKind)roll2 + " from you.");
                            break;
                        }
                    }
                    else
                    {
                        int roll2 = luck.Next(0, 3);
                        bool has = victim.HasCommodities(1, (CommodityKind)roll2);
                        if (has)
                        {
                            victim.PayCommoditys(1, (CommodityKind)roll2);
                            receiver.AddCommodities(1, (CommodityKind)roll2);
                            receiver.PayFishTokens(3);
                            updatePlayerResourcesUI(playerObjects[victim]);
                            updatePlayerResourcesUI(player);
                            player.GetComponent<playerControl>().RpcEndStealInterface();
                            logAPlayer(player, "you stole 1 " + (CommodityKind)roll2 + " from " + victim.name + ".");
                            logAPlayer(playerObjects[victim], receiver.name + "stole 1 " + (CommodityKind)roll2 + " from you.");
                            break;
                        }
                    }
                }
            }
            else
            {
                logAPlayer(player, "You must be in the build/trade phase.");
            }

        }
        else
        {
            logAPlayer(player, "Wait your turn.");
        }
    }
    public void MoveBarbs()
    {
        barbPosition = (barbPosition + 1) % 8;
    }
    #endregion

    #region Constraint Checks
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
        //on first setup road must be built connected to settlement built
        if (currentPhase == GamePhase.SetupRoundOne)
        {
            foreach (Intersection i in edge.GetComponent<Edges>().endPoints)
            {
                //check owned true or else owned is null pointer
                if (i.owned && i.positionedUnit.Owner.Equals(player) && ((Village)i.positionedUnit).myKind == VillageKind.Settlement)
                {
                    check = true;
                    break;
                }
            }
        }
        //on second setup road must be connected to city
        else if (currentPhase == GamePhase.SetupRoundTwo)
        {
            foreach (Intersection i in edge.GetComponent<Edges>().endPoints)
            {
                //check owned true or else owned is null pointer
                if (i.owned && i.positionedUnit.Owner.Equals(player) && ((Village)i.positionedUnit).myKind == VillageKind.City)
                {
                    check = true;
                    break;
                }
            }
        }
        //on build phase it can be built/connect to any road or city.
        else if(currentPhase == GamePhase.TurnFirstPhase)
        {
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
                    if (e.owned && e.belongsTo.Equals(player) && e.isShip == false)
                    {
                        check = true;
                        break;
                    }
                }
            }
        }
        return check;
    }

    private bool canBuildConnectedShip(Player player, GameObject edge)
    {
        bool check = false;
        //on first setup road/ship must be built connected to settlement built
        if (currentPhase == GamePhase.SetupRoundOne)
        {
            foreach (Intersection i in edge.GetComponent<Edges>().endPoints)
            {
                //check owned true or else owned is null pointer
                if (i.owned && i.positionedUnit.Owner.Equals(player) && ((Village)i.positionedUnit).myKind == VillageKind.Settlement)
                {
                    check = true;
                    break;
                }
            }
        }
        //on second setup road/ship must be connected to city
        else if (currentPhase == GamePhase.SetupRoundTwo)
        {
            foreach (Intersection i in edge.GetComponent<Edges>().endPoints)
            {
                //check owned true or else owned is null pointer
                if (i.owned && i.positionedUnit.Owner.Equals(player) && ((Village)i.positionedUnit).myKind == VillageKind.City)
                {
                    check = true;
                    break;
                }
            }
        }
        //on build phase it has to be connected to a city or another boat.
        else if (currentPhase == GamePhase.TurnFirstPhase)
        {
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
                    if (e.owned && e.belongsTo.Equals(player) && e.isShip == true)
                    {
                        check = true;
                        break;
                    }
                }
            }
        }
        return check;
    }

    private bool canBuildConnectedCity(Player player, GameObject intersection)
    {
        bool checkProximity = true;
        bool checkRoadConnection = false;
        bool checkIsLand = false;
        foreach (TerrainHex tile in intersection.GetComponent<Intersection>().linked)
        {
            if(tile.myTerrain != TerrainKind.Sea)
            {
                checkIsLand = true;
            }
        }
        foreach (Edges e in intersection.GetComponent<Intersection>().paths)
        {
            //check that a road is on any of the possible edges of this intersection
            if((currentPhase == GamePhase.TurnFirstPhase) && e.belongsTo != null && e.belongsTo.Equals(player))
            {
                checkRoadConnection = true;
            }
            //automatically can build in setup
            else if(currentPhase == GamePhase.SetupRoundOne || currentPhase == GamePhase.SetupRoundTwo)
            {
                checkRoadConnection = true;
            }
            //check if any close by intersection have already built settlements/cities
            foreach (Intersection i in e.endPoints)
            {

                if (i.owned)
                {
                    checkProximity = false;
                    break;
                }
            }
        }
        return (checkProximity && checkRoadConnection && checkIsLand);
    }

    private bool canBuildKnight(Player player, GameObject intersection)
    {
        bool checkRoadConnection = false;
        bool checkIsLand = false;
        foreach (TerrainHex tile in intersection.GetComponent<Intersection>().linked)
        {
            if (tile.myTerrain != TerrainKind.Sea)
            {
                checkIsLand = true;
            }
        }
        foreach (Edges e in intersection.GetComponent<Intersection>().paths)
        {
            //check that a road is on any of the possible edges of this intersection
            if ((currentPhase == GamePhase.TurnFirstPhase) && e.belongsTo != null && e.belongsTo.Equals(player))
            {
                checkRoadConnection = true;
            }
            //automatically can build in setup
            else if (currentPhase == GamePhase.SetupRoundOne || currentPhase == GamePhase.SetupRoundTwo)
            {
                checkRoadConnection = true;
            }
        }
        return (checkRoadConnection && checkIsLand);
    }

    #endregion

    #region Resource Modifiers
    //gives correct resources after a diced is rolled on beggining of turn
    public void DistributeResources()
    {
        List<Player> receivingPlayers = new List<Player>();
        int sum = gameDices.getRed() + gameDices.getYellow();
        if (sum != 7)
        {
            foreach (GameObject tile in boardTile)
            {
                TerrainHex tempTile = tile.GetComponent<TerrainHex>();
                if (tempTile.numberToken == sum && !tempTile.isRobber)
                {
                    foreach (Intersection connected in tile.GetComponent<TerrainHex>().corners)
                    {
                        //check if owned and is a village type as knights dont fucking gain resources
                        if (connected.owned == true && connected.positionedUnit.GetType() == typeof(Village))
                        {
                            Player gainer = connected.positionedUnit.Owner;
                            Village hisVillage = (Village)(connected.positionedUnit);
                            switch (tile.GetComponent<TerrainHex>().myTerrain)
                            {
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
                                            gainer.AddGold(1);
                                        }
                                        else
                                        {
                                            gainer.AddGold(2);
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
                    paidTo.AddGold(2);
                    break;
                }
        }
    }

    public void discardResources(GameObject player,int[] values)
    {
        Player discardingPlayer = gamePlayers[player];
        bool hasAll = true;
        int i = 0;
        //check if he has enough of what he tries to discard
        while(i < values.Length)
        {
            if (i < 5 && !discardingPlayer.HasResources(values[i],(ResourceKind) i))
            {
                hasAll = false;
                break;
            }
            else if (i >= 5 && !discardingPlayer.HasCommodities(values[i], (CommodityKind)(i-5)))
            {
                hasAll = false;
                break;
            }
            i++;
        }
        if (hasAll)
        {
            i = 0;
            //make payment loop no need to recheck has constraint was checked earlier
            while (i < values.Length)
            {
                if (i < 5 )
                {
                    discardingPlayer.PayResources(values[i], (ResourceKind)i);
                }
                else if (i >= 5)
                {
                    discardingPlayer.PayCommoditys(values[i], (CommodityKind)(i - 5));
                }
                i++;
            }
        }
        else
        {
            string notEnoughOf = "";
            if (i < 5)
            {
                notEnoughOf += "You dont have enough: " + (ResourceKind)i;
            }
            else
            {
                notEnoughOf += "You dont have enough: " + (CommodityKind)(i-5);
            }

            player.GetComponent<playerControl>().RpcDiscardTime((int)(gamePlayers[player].SumResources() / 2.0), notEnoughOf);
            
        }
        updatePlayerResourcesUI(player);
    }

    //send a popup to all players that have over 7 resource/commodities cards asking them to discard the correct amount
    public void robberDiscarding()
        {
            IEnumerator values = (gamePlayers.Values).GetEnumerator();
            while (values.MoveNext())
            {
                Player tempPlayer = (Player)values.Current;
                if(tempPlayer.SumResources() > 7)
                {
                    int toDiscard = (int)(tempPlayer.SumResources() / 2.0);
                    playerObjects[tempPlayer].GetComponent<playerControl>().RpcDiscardTime(toDiscard,"");
                }
            }
        }
    #endregion

    // Methods for handling event rolls
    #region Event Roll

    // Checks the event dice and dispatches the actions accordingly
    private void HandleEventDice()
    {
        Debug.Log(gameDices.getEventKind());
        EventKind roll = gameDices.getEventKind();
        if (roll == EventKind.Barbarian)
        {
            HandleBarbarianRoll();
        }
        else
        {
            HandeCityGateRoll();
        }
    }

    // Handles a barbarian dice roll
    private void HandleBarbarianRoll()
    {
        MoveBarbs();
        if (barbPosition == BARB_ATTACK_POSITION)
        {
            broadcastMessage("Barbarians Rolled. Prepare for the attack!");
            BarbarianAttack();
            firstBarbAttack = true;
            barbPosition = 0;
        }
    }

    // Handle the barbarian attack
    private void BarbarianAttack()
    {
        int playerStrength = getActiveKnightCount();
        int barbStrength = getCityAndMetrCount();
        if (playerStrength > barbStrength)
        {
            broadcastMessage("Players win!");
            defeatBarbarians();
        }
        else
        {
            broadcastMessage("Barbarians win. Prepare for punishment!");
            barbarianPillage();
        }
    }

    private void defeatBarbarians()
    {
        broadcastMessage("Victory Not Yet Implemented. Needs card draw.");
        // TODO: Complete this method
    }

    // TODO: getActiveKnightCount()
    // Handle the barbarian victory and pillaging
    private void barbarianPillage()
    {
        // Find out which players are susceptable to the attack
        List<Player> victims = new List<Player>();
        foreach (Player p in gamePlayers.Values)
        {
            if (p.getCityCount() > 0)
                victims.Add(p);
        }

        // Find out which victim contributed the least
        int leastContributedAmount = 0;
        List<Player> leastContributed = new List<Player>();
        foreach(Player p in victims)
        {
            leastContributedAmount = Mathf.Min(leastContributedAmount, p.getActiveKnightCount());
        }
        foreach(Player p in victims)
        {
            if (p.getActiveKnightCount() == leastContributedAmount)
                leastContributed.Add(p);
        }

        // Punish the victims
        foreach(Player p in leastContributed)
        {
            broadcastMessage(p.name + " was punished for providing the least active knights. A city has been downgraded.");
            List<Village> cities = p.getCities();
            int ind = rng.Next(cities.Count);
            cities[ind].downgradeToSettlement();
        }
    }

    // get the total number of active knights
    private int getActiveKnightCount()
    {
        int total = 0;
        foreach (Player p in gamePlayers.Values)
        {
            total += p.getActiveKnightCount();
        }
        return total;
    }

    // get the total number of cities and metropolises
    private int getCityAndMetrCount()
    {
        int total = 0;
        foreach (Player p in gamePlayers.Values)
        {
            total += p.getCityCount();
            total += p.getMetropolisCount();
        }
        return total;
    }

    // Handles what happens when a city gate is rolled on the event dice
    private void HandeCityGateRoll()
    {
        EventKind gate = gameDices.getEventKind();
        IEnumerator values = gamePlayers.Values.GetEnumerator();
        // TODO: Handle what happens when a city gate is rolled
        switch (gate)
        {
            case EventKind.Politics:
                {
                    while (values.MoveNext())
                    {
                        Player temp = (Player)(values.Current);
                        int hisLevel = temp.GetCityImprovementLevel(CommodityKind.Coin);
                        if(hisLevel != 0 && hisLevel+1 >= gameDices.getRed())
                        {
                            if (temp.cardsInHand.Count == 4)
                            {
                                logAPlayer(playerObjects[temp], "You weren't awarded your card as your hand is full(4).");
                            }
                            else if (!gameDices.HasCardsInPool(EventKind.Politics))
                            {
                                logAPlayer(playerObjects[temp], "All cards have been drawn, better luck next time.");
                            }
                            else
                            {
                                ProgressCardKind card = gameDices.rollCard(EventKind.Politics);
                                //handle the automatically played constitution card
                                if(card == ProgressCardKind.ConstitutionCard)
                                {
                                    playCard(playerObjects[temp], card);
                                    logAPlayer(playerObjects[temp], "You were awarded 1 VP for drawing the constitution Card. Lucky You!");
                                }
                                else
                                {
                                    //card added to hand for reference
                                    temp.cardsInHand.Add(card);
                                    playerObjects[temp].GetComponent<playerControl>().RpcAddProgressCard(card);
                                    logAPlayer(playerObjects[temp], "You have just drawn " + card.ToString());
                                }
                                
                            }
                        }
                    }
                    break;
                }
            case EventKind.Science:
                {
                    while (values.MoveNext())
                    {
                        Player temp = (Player)(values.Current);
                        int hisLevel = temp.GetCityImprovementLevel(CommodityKind.Paper);
                        if (hisLevel != 0 && hisLevel + 1 >= gameDices.getRed())
                        {
                            if (temp.cardsInHand.Count == 4)
                            {
                                logAPlayer(playerObjects[temp], "You weren't awarded your card as your hand is full(4).");
                            }
                            else if (!gameDices.HasCardsInPool(EventKind.Science))
                            {
                                logAPlayer(playerObjects[temp], "All cards have been drawn, better luck next time.");
                            }
                            else
                            {
                                ProgressCardKind card = gameDices.rollCard(EventKind.Science);
                                //the printer card is handle differently so thats why there's an if condition
                                if (card == ProgressCardKind.PrinterCard)
                                {
                                    playCard(playerObjects[temp], card);
                                    logAPlayer(playerObjects[temp], "You were awarded 1 VP for drawing the Printer Card. Lucky You!");
                                }
                                else
                                {
                                    temp.cardsInHand.Add(card);
                                    playerObjects[temp].GetComponent<playerControl>().RpcAddProgressCard(card);
                                    logAPlayer(playerObjects[temp], "You have just drawn " + card.ToString());
                                }
                            }
                        }
                    }
                    break;
                }
            case EventKind.Trade:
                {
                    while (values.MoveNext())
                    {
                        Player temp = (Player)(values.Current);
                        int hisLevel = temp.GetCityImprovementLevel(CommodityKind.Cloth);
                        if (hisLevel != 0 && hisLevel + 1 >= gameDices.getRed())
                        {
                            if (temp.cardsInHand.Count == 4)
                            {
                                logAPlayer(playerObjects[temp], "You weren't awarded your card as your hand is full(4).");
                            }
                            else if (!gameDices.HasCardsInPool(EventKind.Trade))
                            {
                                logAPlayer(playerObjects[temp], "All cards have been drawn, better luck next time.");
                            }
                            else
                            {
                                ProgressCardKind card = gameDices.rollCard(EventKind.Trade);
                                temp.cardsInHand.Add(card);
                                playerObjects[temp].GetComponent<playerControl>().RpcAddProgressCard(card);
                                logAPlayer(playerObjects[temp], "You have just drawn "+ card.ToString());
                            }
                            
                        }
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
    }
    #endregion

    // Victory point checks
    #region Victory points
    
    private void CheckForVictory()
    {
        foreach (Player p in gamePlayers.Values)
        {
            if (p.victoryPoints >= 13)
            {
                string message = "Player " + p.name + " has won this game! \n Press the Exit button to quit.";
                foreach (GameObject pl in gamePlayers.Keys)
                {
                    pl.GetComponent<playerControl>().RpcVictoryPanel(message);
                }
            }
        }
    }

    private void CheckForLongestRoad()
    {
        // Check each player for a potential longest road
        var longestRoadLength = 4;
        var longestRoadPlayer = gamePlayers.Values.FirstOrDefault(p => p.hasLongestTradeRoute);
        Player newLongestRoadPlayer = null;
        foreach (Player p in gamePlayers.Values)
        {
            var longestRoadLengthP = GetPlayerLongestRoad(p);
            if (longestRoadLengthP > longestRoadLength)
            {
                longestRoadLength = longestRoadLengthP;
                newLongestRoadPlayer = p;
            }
        }
        if (newLongestRoadPlayer != null)
        {
            if (longestRoadPlayer != null && longestRoadPlayer != newLongestRoadPlayer)
            {
                longestRoadPlayer.TakeLongestRoad();
                updatePlayerResourcesUI(playerObjects[longestRoadPlayer]);
                logAPlayer(playerObjects[longestRoadPlayer], "You have lost the longest road...");
                newLongestRoadPlayer.GiveLongestTradeRoute();
                updatePlayerResourcesUI(playerObjects[newLongestRoadPlayer]);
                logAPlayer(playerObjects[newLongestRoadPlayer], "You now have the longest road!");
            }
            else if (longestRoadPlayer == null)
            {
                newLongestRoadPlayer.GiveLongestTradeRoute();
                updatePlayerResourcesUI(playerObjects[newLongestRoadPlayer]);
                logAPlayer(playerObjects[newLongestRoadPlayer], "You now have the longest road!");
            }
            CheckForVictory();
        }
        else
        {
            // Check if the longest road was broken up
            if (longestRoadPlayer != null)
            {
                longestRoadPlayer.TakeLongestRoad();
                updatePlayerResourcesUI(playerObjects[longestRoadPlayer]);
                logAPlayer(playerObjects[longestRoadPlayer], "You have lost the longest road...");
            }
        }
    }

    private int GetPlayerLongestRoad(Player p)
    {
        var edgeSets = new List<List<Edges>>();
        // First, we break edges into connected sets
        while (true)
        {
            var edgesToVisit = new Stack<Edges>();
            var visitedEdges = new List<Edges>();
            var temp = edges.FirstOrDefault(e => e.GetComponent<Edges>().belongsTo == p && !edgeSets.Any(l => l.Contains(e.GetComponent<Edges>())));
            if (temp == null)
                break;
            var startingEdge = temp.GetComponent<Edges>();
            foreach (Intersection endpoint in startingEdge.endPoints)
            {
                if (endpoint.positionedUnit == null || endpoint.positionedUnit.Owner == p)
                {
                    foreach (Edges e in endpoint.paths)
                    {
                        if (e.belongsTo == p)
                        {
                            edgesToVisit.Push(e);
                        }
                    }
                }
            }
            visitedEdges.Add(startingEdge);
            while (edgesToVisit.Count > 0)
            {
                var currentEdge = edgesToVisit.Pop();
                if (!visitedEdges.Contains(currentEdge))
                {
                    foreach (Intersection endpoint in currentEdge.endPoints)
                    {
                        if (endpoint.positionedUnit == null || endpoint.positionedUnit.Owner == p)
                        {
                            foreach (Edges e in endpoint.paths)
                            {
                                if (e.belongsTo == p)
                                {
                                    edgesToVisit.Push(e);
                                }
                            }
                        }
                    }
                    visitedEdges.Add(currentEdge);
                }
            }
            edgeSets.Add(visitedEdges);
        }
        // Then we do DFS on each set to find the longest connected set
        var lengthLR = 0;
        foreach (List<Edges> edgeSet in edgeSets)
        {
            var l = ConnectedRoadSegmentLength(edgeSet, p);
            if (lengthLR < l)
            {
                lengthLR = l;
            }
        }
        return lengthLR;
    }

    private int ConnectedRoadSegmentLength(List<Edges> connectedSet, Player p)
    {
        // Find an endpoint
        Edges endpoint = null;
        foreach(Edges temp in connectedSet)
        {
            int connectedEdges = 0;
            foreach (Intersection i in temp.endPoints)
            {
                if (i.positionedUnit == null || i.positionedUnit.Owner == p)
                {
                    foreach (Edges e in i.paths)
                    {
                        if (connectedSet.Contains(e))
                        {
                            connectedEdges++;
                        }
                    }
                }  
            }
            if (connectedEdges == 1)
            {
                endpoint = temp;
                break;
            }
        }
        if (endpoint == null)
        {
            endpoint = connectedSet[UnityEngine.Random.Range(0, connectedSet.Count)];
        }
        // Start the DFS
        var visitedEdges = new List<Edges>();
        var edgesToVisit = new Stack<EdgeDFSNode>();
        var root = new EdgeDFSNode(endpoint, 1);
        int maxLength = 0;
        edgesToVisit.Push(root);
        while (edgesToVisit.Count > 0)
        {
            var currentEdge = edgesToVisit.Pop();
            if (!visitedEdges.Contains(currentEdge.edge))
            {
                if (maxLength < currentEdge.depth)
                {
                    maxLength = currentEdge.depth;
                }
                foreach (Intersection i in currentEdge.edge.endPoints)
                {
                    if (i.positionedUnit == null || i.positionedUnit.Owner == p)
                    {
                        foreach (Edges e in i.paths)
                        {
                            if (connectedSet.Contains(e))
                            {
                                edgesToVisit.Push(new EdgeDFSNode(e, currentEdge.depth + 1));
                            }
                        }
                    }
                }
                visitedEdges.Add(currentEdge.edge);
            }
        }
        return maxLength;
    }


    private class EdgeDFSNode
    {
        public Edges edge { get; private set; }
        public int depth { get; private set; }

        public EdgeDFSNode(Edges e, int d)
        {
            this.edge = e;
            this.depth = d;
        }
    }

    #endregion

    #region Save/Load
    public void SaveGameData(playerControl client)
    {
        if (currentPhase == GamePhase.SetupRoundOne || currentPhase == GamePhase.SetupRoundTwo) return;
        var toSave = new GameData(this);
        MemoryStream stream = new MemoryStream();
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(stream, toSave);
        FileHelper.SendGameData(stream.GetBuffer(), client);
    }

    public void Load(GameData game)
    {
            LoadFromDataFile(game);
    }

    private void LoadFromDataFile(GameData data)
    {
        this.waitingForRoad = data.waitingForRoad;
        this.firstBarbAttack = data.firstBarbAttack;
        this.barbPosition = data.barbPosition;
        this.currentPhase = data.currentPhase;
        this.CardsInPlay = data.CardsInPlay;
        this.currentPlayerString = data.currentPlayer;
        this.tempPlayersByName = new Dictionary<string, Player>();
        this.robberTile = string.IsNullOrEmpty(data.robberTile) ? null : GameObject.Find(data.robberTile);
        this.pirateTile = string.IsNullOrEmpty(data.pirateTile) ? null : GameObject.Find(data.pirateTile);
        this.merchantTile = string.IsNullOrEmpty(data.merchantTile) ? null : GameObject.Find(data.merchantTile);
        var tempOwnedUnitsByPlayer = new Dictionary<IntersectionUnit, Player>();
        foreach (PlayerData pd in data.gamePlayers)
        {
            var player = Player.Load(pd);
            tempPlayersByName.Add(pd.name, player);
            foreach (IntersectionUnit ou in player.ownedUnits)
            {
                tempOwnedUnitsByPlayer.Add(ou, player);
            }

        }

        for (int i = 0; i < data.boardTile.Length; i++)
        {
            var tile = boardTile.First(t => t.name == data.boardTile[i].name);
            if (tile != null)
                tile.GetComponent<TerrainHex>().Load(data.boardTile[i]);
        }
        for (int j = 0; j < data.edges.Length; j++)
        {
            var edge = edges.First(e => e.name == data.edges[j].name);
            if (edge != null)
            {
                if (!string.IsNullOrEmpty(data.edges[j].belongsTo))
                    edge.GetComponent<Edges>().Load(data.edges[j], tempPlayersByName[data.edges[j].belongsTo]);
                else
                    edge.GetComponent<Edges>().Load(data.edges[j], null);
            }
            
        }
        for (int k = 0; k < data.intersections.Length; k++)
        {
            var inter = intersections.First(i => i.name == data.intersections[k].name);
            if (inter != null)
            {
                var positionedUnit = tempOwnedUnitsByPlayer.Keys.FirstOrDefault(u => u.id == data.intersections[k].positionedUnit);
                inter.GetComponent<Intersection>().Load(data.intersections[k], positionedUnit);
            }  
        }

        // Ordering issue: assign the robber tile here
        if (robberTile != null) robberTile.GetComponent<TerrainHex>().isRobber = true;
        if (pirateTile != null) pirateTile.GetComponent<TerrainHex>().isPirate = true;
    }

    #endregion
}

[Serializable]
public class GameData
{
    public bool waitingForRoad  { get; set; }
    public bool firstBarbAttack { get; set; }
    public int barbPosition { get; set; }
    public GamePhase currentPhase { get; set; }
    public List<PlayerData> gamePlayers { get; set; }
    public string currentPlayer;
    public TerrainHexData[] boardTile { get; set; }
    public EdgeData[] edges { get; set; }
    public IntersectionData[] intersections { get; set; }
    public List<ProgressCardKind> CardsInPlay { get; set; }
    public string robberTile { get; set; }
    public string pirateTile { get; set; }
    public string merchantTile { get; set; }

    public GameData(Game source)
    {
        this.waitingForRoad = source.waitingForRoad;
        this.firstBarbAttack = source.firstBarbAttack;
        this.barbPosition = source.barbPosition;
        this.currentPhase = source.currentPhase;
        this.currentPlayer = source.currentPlayer == null ? 
            null : 
            ((Player)source.currentPlayer.Current).name;
        this.CardsInPlay = source.CardsInPlay;
        this.gamePlayers = source.gamePlayers.Values.Select(p => new PlayerData(p)).ToList();
        this.boardTile = source.boardTile.Select(t => new TerrainHexData(t.GetComponent<TerrainHex>())).ToArray();
        this.edges = source.edges.Select(t => new EdgeData(t.GetComponent<Edges>())).ToArray();
        this.intersections = source.intersections.Select(t => new IntersectionData(t.GetComponent<Intersection>())).ToArray();
        this.robberTile = source.robberTile == null ? string.Empty : source.robberTile.name;
        this.pirateTile = source.pirateTile == null ? string.Empty : source.pirateTile.name;
        this.merchantTile = source.merchantTile == null ? string.Empty : source.merchantTile.name;
    }
    // Dummy constructor for Unet
    public GameData()
    {

    }
}