using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class playerControl : NetworkBehaviour {

    private bool resourcesShown = true;
    private bool rollsShown = true;
    private bool cardsShown = true;

    public bool buildShip = false;
	public bool moveShip = false;
	public bool shipSelected = false;
	public bool movedShipThisTurn = false;
	public Color oldEdgeColor;
	public GameObject selectedEdge;

    public bool interactKnight = false;

	public bool activateKnight = false;
	public bool upgradeKnight = false;
	public bool moveKnight = false;


    private bool pickMetropolis = false;

    private GameObject gameState;
    private bool isSeletionOpen = false;
    public GameObject resourcesWindow, ChatWindow, MenuWindow, MaritimeWindow,
                      MapSelector, DiceWindow, SelectionWindow, nameWindow, CardPanel,
                      discardPanel, improvementPanel, inGameMenuPanel, goldShopPanel,
                      victoryPanel;
    public GameObject cardPrefab;
    private List<byte> saveGameData = null;

// @author xingwei
	// P2P Trade Resources
	/* * Brick, Ore, Wool, Coin, Wheat, Cloth, Lumber, Paper, Gold */
	private int giveBrick = 0;
	private int giveOre = 0;
	private int giveWool = 0;
	private int giveCoin = 0;
	private int giveWheat = 0;
	private int giveCloth = 0;
	private int giveLumber = 0;
	private int givePaper = 0;
	private int giveGold = 0;
	private int wantsBrick = 0;
	private int wantsOre = 0;
	private int wantsWool = 0;
	private int wantsCoin = 0;
	private int wantsWheat = 0;
	private int wantsCloth = 0;
	private int wantsLumber = 0;
	private int wantsPaper = 0;
	private int wantsGold = 0;


	public GameObject P2PTradePanel, P2PTrade_PlayerWants, P2PTrade_PlayerGives,P2PTradeOfferPanel;
	public Text P2PTrade_DebugText,P2PTradeOfferedDescriptionText,P2PTradeGivingDescriptionText;

    #region SyncVar
    //resource panel values
    [SyncVar(hook = "OnChangedBrick")]
    string Brick;
    [SyncVar(hook = "OnChangedOre")]
    string Ore;
    [SyncVar(hook = "OnChangedWool")]
    string Wool;
    [SyncVar(hook = "OnChangedCoin")]
    string Coin;
    [SyncVar(hook = "OnChangedGrain")]
    string Grain;
    [SyncVar(hook = "OnChangedCloth")]
    string Cloth;
    [SyncVar(hook = "OnChangedLumber")]
    string Lumber;
    [SyncVar(hook = "OnChangedPaper")]
    string Paper;
    [SyncVar(hook = "OnChangedGold")]
    string Gold;
    [SyncVar(hook = "OnChangedVictory")]
    string VictoryPoints;

    //dice panel Values
    [SyncVar(hook = "OnChangedRed")]
    string Red;
    [SyncVar(hook = "OnChangedYellow")]
    string Yellow;
    [SyncVar(hook = "OnChangedEvent")]
    string Event;

    // valid name
    [SyncVar(hook = "OnNameValidated")]
    public bool isValidName;
    #endregion




    #region Setup
    void Start() {
        if (SceneManager.GetSceneByName("In-Game") != SceneManager.GetActiveScene()) return;
        if (!isLocalPlayer) return;
        nameWindow.gameObject.SetActive(true);

    }
    void getGameStateOnServer()
    {
        if (!isServer) return;
        gameState = GameObject.Find("GameState");

    }

    public override void OnStartLocalPlayer()
    {
        CmdStartUp();
    }
    #endregion
    void Update() {
        if (!isLocalPlayer) return;
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Pressed left click, casting ray.");
            detectClickedObject();
        }
        if (Input.GetButtonDown("Submit"))
        {
            if (nameWindow.activeInHierarchy)
            {
                getNameToSend();
            }
            else
            {
                string message = ChatWindow.transform.GetChild(1).GetChild(2).GetComponent<Text>().text;
                if (!message.Equals("") && message != null)
                {
                    ChatWindow.transform.GetChild(1).GetComponent<InputField>().text = "";
                    CmdSendMessage(gameObject, message);
                }
                
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inGameMenuPanel.SetActive(!inGameMenuPanel.activeInHierarchy);
            // Pre-load game data to the client
            CmdGetGameData();
        }
    }

    #region UI Related
    public void switchResourcesView()
    {
        Animation resourcesAnimation = transform.GetChild(0).GetComponent<Animation>();
        if (resourcesShown)
        {
            resourcesAnimation.Play("HideResources");
        }
        else
        {
            resourcesAnimation.Play("ShowResources");
        }
        resourcesShown = !resourcesShown;
    }

    public void switchRollsView()
    {
        Animation rollsAnimation = DiceWindow.transform.GetComponent<Animation>();
        if (rollsShown)
        {
            rollsAnimation.Play("HideRolls");
        }
        else
        {
            rollsAnimation.Play("ShowRolls");
        }
        rollsShown = !rollsShown;
    }

    public void switchCardPanel()
    {
        Animation cardsAnimation = CardPanel.transform.GetComponent<Animation>();
        if (cardsShown)
        {
            cardsAnimation.Play("HideCards");
            CardPanel.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "Maximize";
        }
        else
        {
            cardsAnimation.Play("ShowCards");
            CardPanel.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "Minimize";
        }
        cardsShown = !cardsShown;
    }

    public void closeSelectView()
    {
        isSeletionOpen = false;
    }
    public void setTextValues(Dictionary<ResourceKind, int> resources, Dictionary<CommodityKind, int> commodities, int gold, int victoryPoints)
    {
        if (!isServer) return;
        int temp;
        resources.TryGetValue(ResourceKind.Brick, out temp);
        Brick = temp.ToString();
        resources.TryGetValue(ResourceKind.Ore, out temp);
        Ore = temp.ToString();
        resources.TryGetValue(ResourceKind.Grain, out temp);
        Grain = temp.ToString();
        resources.TryGetValue(ResourceKind.Lumber, out temp);
        Lumber = temp.ToString();
        resources.TryGetValue(ResourceKind.Wool, out temp);
        Wool = temp.ToString();

        commodities.TryGetValue(CommodityKind.Cloth, out temp);
        Cloth = temp.ToString();
        commodities.TryGetValue(CommodityKind.Coin, out temp);
        Coin = temp.ToString();
        commodities.TryGetValue(CommodityKind.Paper, out temp);
        Paper = temp.ToString();

        Gold = gold.ToString();
        VictoryPoints = victoryPoints.ToString();

    }
    public void setDiceValues(int red, int yellow, int eventValue)
    {
        if (!isServer) return;
        this.Red = "Red Dice Roll: " + red.ToString();
        this.Yellow = "Yellow Dice Roll: " + yellow.ToString();
        this.Event = "Event Dice Roll: " + ((EventKind)eventValue).ToString();

    }
    
    public void setToBuildRoads()
    {
        buildShip = false;
		moveShip = false;
        MenuWindow.transform.GetChild(4).GetComponent<Image>().color = new Color32(121, 240, 121, 240);
        MenuWindow.transform.GetChild(5).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
    }

    public void setToBuildShips()
    {
		
		if (buildShip == false) {
			buildShip = true;
			moveShip = false;
			MenuWindow.transform.GetChild (4).GetComponent<Image> ().color = new Color32 (255, 255, 255, 255);
			MenuWindow.transform.GetChild (5).GetComponent<Image> ().color = new Color32 (121, 240, 121, 240);
		} else if (buildShip == true && moveShip == false) {
			buildShip = false;
			moveShip = true;
			MenuWindow.transform.GetChild (4).GetComponent<Image> ().color = new Color32 (255, 255, 255, 255);
			MenuWindow.transform.GetChild (5).GetComponent<Image> ().color = new Color32 (121, 121, 240, 240); //button becomes blue
		} 
   
    }
    public void setToInteractWithSettlements()
    {
        interactKnight = false;
        MenuWindow.transform.GetChild(3).GetComponent<Image>().color = new Color32(121, 240, 121, 240);
        MenuWindow.transform.GetChild(8).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
    }

    public void setToInteractWithKnights()
    {
        interactKnight = true;
		MenuWindow.transform.GetChild(3).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
		if (!activateKnight && !upgradeKnight) {
			activateKnight = true;
			moveKnight = false;
			MenuWindow.transform.GetChild (8).GetComponent<Image> ().color = new Color32 (121, 240, 121, 240);
		} else if (!moveKnight && !upgradeKnight) {
			upgradeKnight = true;
			activateKnight = false;
			MenuWindow.transform.GetChild (8).GetComponent<Image> ().color = new Color32 (121, 121, 240, 240);
		} else {
			upgradeKnight = false;
			moveKnight = true;
			MenuWindow.transform.GetChild (8).GetComponent<Image> ().color = new Color32 (121, 240, 240, 121);
		}
        
    }

	/*
	 * Called when clicked the Green Confirm Button on the P2P trade panel. 
	 * 
	 * The order of resource stored in the List is
	 * Brick, Ore, Wool, Coin, Wheat, Cloth, Lumber, Paper, Gold
	 * 
	 * Send Trade Request to server
	 * 
	 */
	public void confirmP2PTradeStatus(){
		//InputField i = this.P2PTrade_PlayerGives.transform.Find ("Brick").transform.GetComponentInChildren<InputField> ();
		string txt = "";
		foreach (Transform child in this.P2PTrade_PlayerGives.transform){
			//print (child.name);
			string input = child.transform.GetComponentInChildren<InputField> ().text;
			int number = 0;
			if (int.TryParse (input, out number) && number > 0) {
				//txt += child.name + ": " + child.transform.GetComponentInChildren<InputField> ().text + "\n";
				assignNumberToVariable(child.name,number,false);
			} else {
				assignNumberToVariable(child.name,0,false);
			}
		}

		foreach (Transform child in this.P2PTrade_PlayerWants.transform){
			//print (child.name);
			string input = child.transform.GetComponentInChildren<InputField> ().text;
			int number = 0;
			if (int.TryParse (input, out number) && number != 0) {
				//txt += child.name + ": " + child.transform.GetComponentInChildren<InputField> ().text + "\n";
				assignNumberToVariable(child.name,number,true);
			} else {
				assignNumberToVariable(child.name,0,true);
			}
		}

		gameState.GetComponent<Game> ().P2PTradeOffer (gameObject, giveBrick, giveOre, giveWool, giveCoin, giveWheat, giveCloth, giveLumber, givePaper, giveGold, wantsBrick, wantsOre, wantsWool, wantsCoin, wantsWheat, wantsCloth, wantsLumber, wantsPaper, wantsGold);
	}

	/* Brick, Ore, Wool, Coin, Wheat, Cloth, Lumber, Paper, Gold */
	private void assignNumberToVariable(string name, int quantity, bool wants){
		if (name == "Brick") {
			if (wants) {
				this.wantsBrick = quantity;
			} else {
				this.giveBrick = quantity;
			}
		} else if (name == "Ore") {
			if (wants) {
				this.wantsOre = quantity;
			} else {
				this.giveOre = quantity;
			}
		} else if (name == "Wool") {
			if (wants) {
				this.wantsWool = quantity;
			} else {
				this.giveWool = quantity;
			}
		} else if (name == "Coin") {
			if (wants) {
				this.wantsCoin = quantity;
			} else {
				this.giveCoin = quantity;
			}
		} else if (name == "Wheat") {
			if (wants) {
				this.wantsWheat = quantity;
			} else {
				this.giveWheat = quantity;
			}
		} else if (name == "Cloth") {
			if (wants) {
				this.wantsCloth = quantity;
			} else {
				this.giveCloth = quantity;
			}
		} else if (name == "Lumber") {
			if (wants) {
				this.wantsLumber = quantity;
			} else {
				this.giveLumber = quantity;
			}
		} else if (name == "Paper") {
			if (wants) {
				this.wantsPaper = quantity;
			} else {
				this.givePaper = quantity;
			}
		} else if (name == "Gold") {
			if (wants) {
				this.wantsGold = quantity;
			} else {
				this.giveGold = quantity;
			}
		}
	}
    #endregion

    #region Retrieve Client Info
    void detectClickedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        if (hit && !EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.gameObject.CompareTag("Intersection"))
            {
                if (pickMetropolis)
                {
                    CmdSetMetropole(gameObject, hit.collider.gameObject);
                }
                else if (interactKnight)
                {
                    CmdBuildKnight(hit.collider.gameObject);
                }
                else
                {
                    CmdBuildOnIntersection(hit.collider.gameObject);
                }
                
            }
			if (hit.collider.gameObject.CompareTag("Edge") && moveShip != true)
            {
                CmdBuildOnEdge(gameObject, hit.collider.gameObject);
            }
            if (hit.collider.gameObject.CompareTag("TerrainHex"))
            {
                if(hit.collider.gameObject.GetComponent<TerrainHex>().myTerrain == TerrainKind.Sea)
                {
                    CmdMovePirate(gameObject, hit.collider.gameObject);
                }
                else
                {
                    CmdMoveRobber(gameObject, hit.collider.gameObject);
                }
                
            }
			if (hit.collider.gameObject.CompareTag ("Edge") && moveShip == true && movedShipThisTurn == false) {
				CmdMoveShip (gameObject, hit.collider.gameObject, shipSelected);
			}
        }
    }

    public void getNameToSend()
    {
        string playerName = nameWindow.transform.GetChild(0).GetChild(2).GetComponent<Text>().text;
        if (!playerName.Equals("") && playerName != null)
        {
            CmdValidateName(playerName);
            if (!isValidName) return;
            //CmdSendName(playerName);
            ////open the menus
            //resourcesWindow.gameObject.SetActive(true);
            //MenuWindow.gameObject.SetActive(true);
            //DiceWindow.gameObject.SetActive(true);
            //ChatWindow.gameObject.SetActive(true);
            //CardPanel.gameObject.SetActive(true);
            ////closet the window
            //nameWindow.SetActive(false);  
        }

    }

    [Command]
    private void CmdValidateName(string name)
    {
        gameState.GetComponent<Game>().ValidateName(gameObject, name);
    }

    public void validateName(bool result)
    {
        this.isValidName = result;
    }
    public void getTradeValue()
    {
        int toGive, wanted;
        toGive = transform.GetChild(3).GetChild(2).GetComponent<Dropdown>().value;
        wanted = transform.GetChild(3).GetChild(3).GetComponent<Dropdown>().value;
        CmdSendNpcTrade(gameObject, toGive, wanted);
    }
    
    public void GetTradeBuyValue()
    {
        var toBuy = goldShopPanel.transform.GetChild(1).GetComponent<Dropdown>().value;
        CmdBuyWithGold(gameObject, toBuy);
    }

    public void getDiscardValues()
    {
        int[] values = new int[8];
        int sum = 0;
        for(int i = 0; i< values.Length; i++)
        {
            //i = 0 is wool like enum 0 = wool etc...
            if (discardPanel.transform.GetChild(i).GetChild(2).GetComponent<InputField>().text.Equals(""))
            {
                values[i] = 0;
                sum += 0;
            }
            else
            {
                
                values[i] = int.Parse(discardPanel.transform.GetChild(i).GetChild(2).GetComponent<InputField>().text);
                sum += values[i];
            }
        }
        int needed = int.Parse(discardPanel.transform.GetChild(10).GetComponent<Text>().text);
        //loop to check if he has enough of all the resources
        if (sum == needed)
        {
            CmdSendDiscards(gameObject, values);
            discardPanel.SetActive(false);
        }
        else if(sum > needed)
        {
            discardPanel.transform.GetChild(9).GetComponent<Text>().text = "You need to discard : " + needed.ToString() + " you want to discard : " + sum.ToString() + " please remove : " + (sum - needed);
        }
        else
        {
            discardPanel.transform.GetChild(9).GetComponent<Text>().text = "You need to discard : " + needed.ToString() + " you want to discard : " + sum.ToString() + " please add : " + (needed - sum);
        }
        
    }

    public void SaveGame()
    {
        var savePath = FileHelper.SanitizePath(inGameMenuPanel.transform.Find("FilePath").GetComponent<InputField>().text);
        if (!string.IsNullOrEmpty(savePath))
        {
            //CmdGetGameData();
            if (this.saveGameData != null)
            {
                File.WriteAllBytes(Application.persistentDataPath + "/" + savePath + ".dat", this.saveGameData.ToArray());
            }
        }
    }
    #endregion

    #region Commands
    [Command]
    public void CmdStartUp()
    {
        if (SceneManager.GetSceneByName("In-Game") != SceneManager.GetActiveScene()) return;
        getGameStateOnServer();
        if (MainMenuBehaviour.isLoaded)
        {
            if (!gameState.GetComponent<Game>().isLoaded)
            {
                // Load the game from a file
                gameState.GetComponent<Game>().Load(MainMenuBehaviour.loadedGameData);
                gameState.GetComponent<Game>().isLoaded = true;
            }
        }
        else
        {
            gameState.GetComponent<Game>().setPlayer(gameObject);
        }
        base.OnStartServer();
    }
    [Command]
    public void CmdSendName(string name)
    {
        gameState.GetComponent<Game>().setPlayerName(gameObject, name);
    }
    [Command]
    void CmdBuildOnIntersection(GameObject intersection)
    {
        gameState.GetComponent<Game>().buildOnIntersection(gameObject, intersection);
    }
	[Command]
	void CmdMoveShip(GameObject player, GameObject edge, bool selected){
		if (!selected) {
			bool temp = gameState.GetComponent<Game> ().removeShipCheck (player, edge);
			if (temp == true) {
				shipSelected = true;
				selectedEdge = edge;
				SpriteRenderer shipColor = selectedEdge.GetComponent<SpriteRenderer> ();
				oldEdgeColor = shipColor.color;
				shipColor.color = new Color32 (121, 121, 240, 240);
			}
		} else {
			bool temp = gameState.GetComponent<Game> ().placeShipCheck (player, edge, selectedEdge);
			shipSelected = false;
			if (temp == true) {
				movedShipThisTurn = true;
				SpriteRenderer shipColor = selectedEdge.GetComponent<SpriteRenderer> ();
				shipColor.color = new Color32 (255, 255, 255, 255);
			} else {
				SpriteRenderer shipColor = selectedEdge.GetComponent<SpriteRenderer> ();
				shipColor.color = oldEdgeColor;
			}
		}
	}
    [Command]
    void CmdBuildOnEdge(GameObject player, GameObject edge)
    {
        if (buildShip)
        {
            gameState.GetComponent<Game>().buildShip(player, edge);
        }
        else
        {
            gameState.GetComponent<Game>().buildRoad(player, edge);
        }
        
    }
    [Command]
    void CmdRollDice(GameObject player)
    {
        gameState.GetComponent<Game>().rollDice(player);
    }
    [Command]
    void CmdSendSelection(GameObject player, int value)
    {
        gameState.GetComponent<Game>().updateSelection(player, value);
    }
    [Command]
    void CmdEndTurn(GameObject player)
    {
        gameState.GetComponent<Game>().endTurn(player);
    }
    [Command]
    void CmdSendNpcTrade(GameObject player, int toGive, int wanted)
    {

        //obviously not going to trade 4 brick -> 1 brick
        if (toGive != wanted)
        {
            gameState.GetComponent<Game>().NpcTrade(player, toGive, wanted);
        }

    }
    [Command]
    void CmdBuyWithGold(GameObject player, int toBuy)
    {
        gameState.GetComponent<Game>().BuyWithGold(player, toBuy);
    }
    [Command]
    void CmdSendMessage(GameObject player, string message)
    {
        gameState.GetComponent<Game>().chatOnServer(player, message);
    }
    [Command]
    void CmdMoveRobber (GameObject player, GameObject tile)
    {
        gameState.GetComponent<Game>().moveRobber(player, tile);
    }
    [Command]
    void CmdMovePirate (GameObject player, GameObject tile)
    {
        gameState.GetComponent<Game>().movePirate(player, tile);
    }
    [Command]
    void CmdSendDiscards (GameObject player, int[] values)
    {
        gameState.GetComponent<Game>().discardResources(player, values);
    }
    [Command]
    public void CmdUseCard (ProgressCardKind k)
    {
        gameState.GetComponent<Game>().playCard(gameObject,k);
    }
    [Command]
    public void CmdBuildKnight(GameObject intersection)
    {
        gameState.GetComponent<Game>().buildKnightOnIntersection(gameObject, intersection);
    }

    [Command]
    public void CmdSetMetropole(GameObject player, GameObject intersection)
    {
        gameState.GetComponent<Game>().setMetropole(player, intersection);
    }

    [Command]
    public void CmdCityUpgrade(int kind)
    {
        gameState.GetComponent<Game>().improveCity(gameObject, kind);
    }

    [Command]
    public void CmdGetGameData()
    {
        gameState.GetComponent<Game>().SaveGameData(this);
    }

    [ClientRpc]
    public void RpcGetGameData(byte[] data, int offset)
    {
        if (this.saveGameData == null)
        {
            this.saveGameData = new List<byte>();
        }
        for (int i = offset; i < offset + data.Length; i++)
        {
            this.saveGameData.Add(data[i - offset]);
        }

    }
    #endregion

    #region Sync Hooks
    void OnChangedBrick(string value)
    {
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>().text = value;
    }
    void OnChangedOre(string value)
    {
        transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<Text>().text = value;
    }
    void OnChangedWool(string value)
    {
        transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<Text>().text = value;
    }
    void OnChangedCoin(string value)
    {

        transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<Text>().text = value;
    }
    void OnChangedGrain(string value)
    {
        transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<Text>().text = value;
    }
    void OnChangedCloth(string value)
    {
        transform.GetChild(0).GetChild(6).GetChild(0).GetComponent<Text>().text = value;
    }
    void OnChangedLumber(string value)
    {
        transform.GetChild(0).GetChild(7).GetChild(0).GetComponent<Text>().text = value;
    }
    void OnChangedPaper(string value)
    {
        transform.GetChild(0).GetChild(8).GetChild(0).GetComponent<Text>().text = value;
    }
    void OnChangedGold(string value)
    {
        transform.GetChild(0).GetChild(9).GetChild(0).GetComponent<Text>().text = value;
    }
    void OnChangedVictory(string value)
    {
        transform.GetChild(0).GetChild(10).GetChild(0).GetComponent<Text>().text = value;
    }
    void OnChangedRed(string value)
    {
        DiceWindow.transform.GetChild(2).GetComponent<Text>().text = value;
    }
    void OnChangedYellow(string value)
    {
        DiceWindow.transform.GetChild(3).GetComponent<Text>().text = value;
    }
    void OnChangedEvent(string value)
    {
        DiceWindow.transform.GetChild(1).GetComponent<Text>().text = value;
    }
    void OnNameValidated(bool value)
    {
        this.isValidName = value;
    }
    #endregion

    #region ClientRPC
    [ClientRpc]
    public void RpcUpdateChat(string message)
    {
        ChatWindow.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text += message;
        Canvas.ForceUpdateCanvases();
        ChatWindow.transform.GetChild(0).GetChild(2).GetComponent<Scrollbar>().value = 0f;
        Canvas.ForceUpdateCanvases();
    }

    // the value to get if he has aquaduct and doesnt receive anything
    [ClientRpc]
    public void RpcAskDesiredAquaResource()
    {
        isSeletionOpen = true;
        SelectionWindow.gameObject.SetActive(true);
        int selectedValue = 0;
        while (isSeletionOpen)
        {
            selectedValue = SelectionWindow.transform.GetChild(1).GetComponent<Dropdown>().value;
        }
        SelectionWindow.gameObject.SetActive(false);
        CmdSendSelection(gameObject, selectedValue);
    }
    [ClientRpc]
    public void RpcCloseTrade(bool accepted)
    {
        if (accepted)
        {
            MaritimeWindow.gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    public void RpcCloseGoldShop(bool accepted)
    {
        if (accepted)
        {
            goldShopPanel.gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    public void RpcBeginMetropoleChoice()
    {
        this.improvementPanel.SetActive(false);
        this.pickMetropolis = true;
    }

    [ClientRpc]
    public void RpcEndMetropoleChoice()
    {
        this.pickMetropolis = false;
    }

    [ClientRpc]
    public void RpcUpdateTurn(string value)
    {
        transform.GetChild(8).GetComponent<Text>().text = value;
    }
    
    [ClientRpc]
    public void RpcAddProgressCard(ProgressCardKind value)
    {
        //adds a card to panel when received;
        GameObject tempCard = Instantiate(cardPrefab);
        //set the card value and it will change its sprite accordingly
        tempCard.GetComponent<CardControl>().setCard(new Card(value));
        //put it in the view
        tempCard.transform.SetParent(CardPanel.transform.GetChild(0).GetChild(0).GetChild(0).transform,false);
    }

    [ClientRpc]
    public void RpcRemoveProgressCard(ProgressCardKind value)
    {
        CardControl[] tempCards = CardPanel.transform.GetChild(0).GetChild(0).GetChild(0).GetComponentsInChildren<CardControl>();
        foreach(CardControl card in tempCards)
        {
            if(card.getCard().k == value)
            {
                card.removeCard();
                break;
            }
        }
    }

    [ClientRpc]
    public void RpcDiscardTime(int discardAmount, string ExtraInfo)
    {
        if (!isLocalPlayer) return;
        discardPanel.SetActive(true);
        //in order of enums for easy for looping later
        discardPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = Wool;
        discardPanel.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = Lumber;
        discardPanel.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = Ore;
        discardPanel.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = Brick;
        discardPanel.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = Grain;
        discardPanel.transform.GetChild(5).GetChild(0).GetComponent<Text>().text = Coin;
        discardPanel.transform.GetChild(6).GetChild(0).GetComponent<Text>().text = Cloth;
        discardPanel.transform.GetChild(7).GetChild(0).GetComponent<Text>().text = Paper;

        discardPanel.transform.GetChild(9).GetComponent<Text>().text = "You need to discard a total of : " + discardAmount.ToString() + "\n" + ExtraInfo;
        discardPanel.transform.GetChild(10).GetComponent<Text>().text = discardAmount.ToString();
    }

    [ClientRpc]
    public void RpcUpdateSliders(int level,int kind)
    {
        if(level == 1)
        {
            improvementPanel.transform.GetChild(kind).GetChild(0).GetChild(1).GetChild(0).gameObject.SetActive(true);
        }
        improvementPanel.transform.GetChild(kind).GetChild(0).GetComponent<Slider>().value = level;
    }

    [ClientRpc]
    public void RpcVictoryPanel(string message)
    {
        if (!isLocalPlayer) return;
        this.victoryPanel.SetActive(true);
        this.victoryPanel.transform.Find("VictoryMessage").GetComponent<Text>().text = message;
    }

    [ClientRpc]
    public void RpcNameCheck(bool result)
    {
        if (!isLocalPlayer) return;
        if (result)
        {
            //open the menus
            resourcesWindow.gameObject.SetActive(true);
            MenuWindow.gameObject.SetActive(true);
            DiceWindow.gameObject.SetActive(true);
            ChatWindow.gameObject.SetActive(true);
            CardPanel.gameObject.SetActive(true);
            //closet the window
            nameWindow.SetActive(false);
        }
        else
        {
            nameWindow.transform.GetChild(0).GetComponent<InputField>().text = "";
        }
    }
	
	/**
	 * @author xingwei
	 * P2P trade UI text upgrading RPC functions
	 */
	[ClientRpc]
	public void RpcLogP2PTradeDebugText(string txt, bool red){
		if (red) {
			P2PTrade_DebugText.color = Color.red;
		} else {
			P2PTrade_DebugText.color = Color.black;
		}
		P2PTrade_DebugText.text = txt;
	}

	[ClientRpc]
	public void RpcSetP2PTradeOfferedDescriptionText(string txt){
		P2PTradeOfferedDescriptionText.text = txt;
	}

	[ClientRpc]
	public void RpcSetP2PTradeGivingDescriptionText(string txt){
		P2PTradeGivingDescriptionText.text = txt;
	}

	[ClientRpc]
	public void RpcSetP2PTradeOfferPanelActive(bool active){
		P2PTradeOfferPanel.SetActive (active);
	}

	[ClientRpc]
	public void RpcSetP2PTradePanelActive(bool active){
		P2PTradePanel.SetActive (active);





	}

	/**
	 * Reset the input field of P2P Trade Panel 
	 */
	[ClientRpc]
	public void RpcResetP2PTradeInput(){
		foreach (Transform child in this.P2PTrade_PlayerGives.transform) {
















			child.transform.GetComponent<InputField> ().text = "";
		}

	}
    #endregion

    public void testCard()
    {
        //adds a card to panel when received;
        GameObject tempCard = Instantiate(cardPrefab);
        //set the card value and it will change its sprite accordingly
        tempCard.GetComponent<CardControl>().setCard(new Card(ProgressCardKind.PrinterCard));
        //put it in the view
        tempCard.transform.SetParent(CardPanel.transform.GetChild(0).GetChild(0).GetChild(0).transform, false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
