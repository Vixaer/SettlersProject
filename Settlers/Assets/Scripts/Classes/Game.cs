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
    public GameObject robberTile,pirateTile,merchantTile;

    public List<ProgressCardKind> CardsInPlay = new List<ProgressCardKind>();

    public bool isLoaded = false;

    private Dictionary<string, Player> tempPlayersByName;

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
        player.transform.GetComponent<playerControl>().RpcCheckNameResult(!isLoaded || tempPlayersByName.ContainsKey(name));
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
        System.Random temp = new System.Random();
        foreach (GameObject tile in boardTile)
        {
            gameDices.rollTile();
            tile.GetComponent<TerrainHex>().setTile(gameDices.getTerrain(), gameDices.getToken());
            if(gameDices.getTerrain() == (int)TerrainKind.Desert && !robberSpawned)
            {
                tile.GetComponent<TerrainHex>().isRobber = true;
                robberTile = tile;
                robberSpawned = true;
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
        upPlayer.GetComponent<playerControl>().setTextValues(data.resources, data.commodities, data.gold, data.victoryPoints);

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
            //offering resrouce wants  a resource
            if (offer < 5 && wants < 5)
            {
                //special payment
                if (hasSpecial && tradingPlayer.HasResources(2, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(2, (ResourceKind)offer);
                    log += "Has traded 2 ";
                }
                //generic
                else if (hasGeneric && tradingPlayer.HasResources(3, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(3, (ResourceKind)offer);
                    log += "Has traded 3 ";
                }
                //shitty
                else if (tradingPlayer.HasResources(4, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(4, (ResourceKind)offer);
                    log += "Has traded 4 ";
                }  
                tradingPlayer.AddResources(1, (ResourceKind)wants);
                log += ((ResourceKind)offer).ToString() + " for 1 " + ((ResourceKind)wants).ToString();
                check = true;
            }
            //offering resources wants a commodity
            else if (offer < 5 && wants >= 5)
            {
                //special
                if (hasSpecial && tradingPlayer.HasResources(2, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(2, (ResourceKind)offer);
                    log += "Has traded 2 ";
                }
                //generic
                else if (hasGeneric && tradingPlayer.HasResources(3, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(3, (ResourceKind)offer);
                    log += "Has traded 3 ";
                }
                //shitty
                else if (tradingPlayer.HasResources(4, (ResourceKind)offer))
                {
                    tradingPlayer.PayResources(4, (ResourceKind)offer);
                    log += "Has traded 4 ";
                }
                gamePlayers[player].AddCommodities(1, (CommodityKind)wants - 5);
                log += ((ResourceKind)offer).ToString() + " for 1 " + ((CommodityKind)wants - 5).ToString();
                check = true;

            }
            //offering commodity wants resource
            else if (offer >= 5 && wants < 5)
            {
                if (gamePlayers[player].HasCommodities(4, (CommodityKind)(offer - 5)))
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
                if (gamePlayers[player].HasCommodities(4, (CommodityKind)(offer - 5)))
                {
                    gamePlayers[player].PayCommoditys(4, (CommodityKind)(offer - 5));
                    gamePlayers[player].AddCommodities(1, (CommodityKind)wants - 5);
                    log += "Has Traded 4 " + ((CommodityKind)offer - 5).ToString() + " for 1 " + ((CommodityKind)wants - 5).ToString();
                    check = true;
                }
            }        //update his ui
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

                waitingForRoad = true;
            }
            //second setup spawns City
            else if (currentPhase == GamePhase.SetupRoundTwo && !waitingForRoad && canBuild)
            {
                    inter.BuildCity(currentBuilder);
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
                        //update his UI to let him know he lost the resources;
                        updatePlayerResourcesUI(player);
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
                            CardsInPlay.Remove(ProgressCardKind.MedicineCard);
                            //update his UI to let him know he lost the resources;
                            updatePlayerResourcesUI(player);
                            logAPlayer(player, "You upgraded your settlement into a city!");
                        }
                       
                    }
                }
            }   
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
                            logAPlayer(player, "You've reached the 3 basic Knight cap, try upgrading a knight before attempting to hire another knight");
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
                            logAPlayer(player, "Your resources are insufficient for upgrading this Knight.");
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
                            if (currentBuilder.HasKnights(KnightLevel.Mighty))
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
                gamePlayers[player].PayShipResources();
                edge.GetComponent<Edges>().BuildRoad(gamePlayers[player]);
                //update his UI to let him know he lost the resources;
                updatePlayerResourcesUI(player);
            }
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
                edge.GetComponent<Edges>().BuildRoad(gamePlayers[player]);
                CardsInPlay.Remove(ProgressCardKind.RoadBuildingCard);
                logAPlayer(player, "You built a free ship because of the Road Building Card.");
            }
            //during first phase building
            else if (currentPhase == GamePhase.TurnFirstPhase && gamePlayers[player].HasShipResources())
            {
                    gamePlayers[player].PayShipResources();
                    edge.GetComponent<Edges>().BuildShip(gamePlayers[player]);
                    //update his UI to let him know he lost the resources;
                    updatePlayerResourcesUI(player);
            }
            updateTurn();
        } 
    }

	public bool removeShipCheck (GameObject player, GameObject edge) {
		bool correctPlayer = checkCorrectPlayer(player);
		if (!correctPlayer)
		{
			logAPlayer(player, "It isn't your turn.");
			return false;
		}

		//owned check
		Edges temp = edge.GetComponent<Edges>();
		Player temp2 = gamePlayers[player];

		
		if (temp.isShip == true && !temp.belongsTo.Equals(temp2) ){
			logAPlayer (player, "This ship does not belong to you!");
			return false;
		} else if (temp.isShip == true && temp.belongsTo.Equals(temp2)) {
			// not connected to 2 ships check
			bool connectCheck = false;
			int count = 0;
			int count2 = 0;
			foreach (Intersection i in temp.endPoints)
			{
				
				foreach (Edges e in i.paths)
				{
					//check to see if owned or else belongs to is obviously null and return null pointer
					if (e.owned) {
						if (e.belongsTo.Equals(temp2))
						{
							if (!connectCheck) {
								count++;
								if (count == 2) {
									connectCheck = true;
									break;
								}
							} else {
								count2++;
								if (count2 == 2)
									break;
							}
								
						}
					}
				}
			}

			if (count2 > 1) {
				logAPlayer (player, "Can't move ships connected on both ends to your other pieces!");
				return false;
			}
			//pirate check
			foreach (TerrainHex a in temp.inBetween) {
				if (a.isPirate == true) {
					logAPlayer (player, "Can't move ships that are next to pirate!");
					return false;
				}
			}
			logAPlayer (player, "Ship Selected!");
			return true;

		} else {
			logAPlayer (player, "Please select a ship to move.");
			return false;
		}

	}

	public bool placeShipCheck (GameObject player, GameObject edge, GameObject oldEdge) {
		bool correctPlayer = checkCorrectPlayer(player);
		Edges temp = edge.GetComponent<Edges>();
		Edges temp2 = oldEdge.GetComponent<Edges> ();
		temp2.owned = false;
		bool canBuild = canBuildConnectedShip(gamePlayers[player], edge);
		bool onWater = false;
		bool isOwned = temp.owned;

		foreach(TerrainHex tile in temp.inBetween)
		{
			if(tile.myTerrain == TerrainKind.Sea)
			{
				onWater = true;
			}
		}

		//pirate check
		foreach (TerrainHex a in temp.inBetween) {
			if (a.isPirate == true) {
				logAPlayer (player, "Can't move ships next to pirate!");
				return false;
			}
		}
		if (!correctPlayer) {
			logAPlayer (player, "It isn't your turn.");
			temp2.owned = true;
			return false;
		} else if (!onWater) {
			logAPlayer (player, "You cant build a ship on land.");
			temp2.owned = true;
			return false;
		} else if (isOwned) {
			logAPlayer (player, "There's already something built here.");
			temp2.owned = true;
			return false;
		} else if (correctPlayer && onWater && !isOwned && canBuild) {
			edge.GetComponent<Edges> ().BuildShip (gamePlayers [player]);
			oldEdge.GetComponent<Edges> ().RemoveShip (gamePlayers [player]);
			logAPlayer (player, "Ship Moved! You cannot move anymore ships this turn.");
			return true;
		} else {
			logAPlayer (player, "Ship is not connected with one of your roads/ships!");
			temp2.owned = true;
			return false;
		}
	}
    
    //end player turn
    public void endTurn(GameObject player)
    {
        
        if (checkCorrectPlayer(player) && currentPhase != GamePhase.TurnRobberPirate)
        {
            if(currentPhase != GamePhase.TurnDiceRolled)
            {
				player.GetComponent<playerControl> ().movedShipThisTurn = false;
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
            if (gameDices.getRed() + gameDices.getYellow() == 7)
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
                        logAPlayer(player, "The Irrigation card has given you : " + sum + " ore.");
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
        Player currentUpgrader = gamePlayers[player];
        foreach (OwnableUnit unit in currentUpgrader.ownedUnits)
        {
            if(unit is Village && ((Village)unit).myKind == VillageKind.City)
            {
                hasCity = true;
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
        else if (!hasCity)
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
            }
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
					Debug.Log(e.belongsTo);
                    if (e.owned && e.isShip == true)
                    {
						if (e.belongsTo.Equals(player) ){
							check = true;
							break;
						}
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