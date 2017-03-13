using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class playerControl : NetworkBehaviour {

    private bool resourcesShown = true;
    private bool rollsShown = true;
    private bool cardsShown = true;
    private GameObject gameState;
    private bool isSeletionOpen = false;
    public GameObject resourcesWindow, ChatWindow, MenuWindow, MaritimeWindow, MapSelector, DiceWindow, SelectionWindow, nameWindow, CardPanel;
    public GameObject cardPrefab;
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

    //dice panel Values
    [SyncVar(hook = "OnChangedRed")]
    string Red;
    [SyncVar(hook = "OnChangedYellow")]
    string Yellow;
    [SyncVar(hook = "OnChangedEvent")]
    string Event;
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
            string message = ChatWindow.transform.GetChild(1).GetChild(2).GetComponent<Text>().text;
            if (!message.Equals("") && message != null)
            {
                ChatWindow.transform.GetChild(1).GetComponent<InputField>().text = "";
                CmdSendMessage(gameObject, message);
            }
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
    public void setTextValues(Dictionary<ResourceKind, int> resources, Dictionary<CommodityKind, int> commodities)
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

    }
    public void setDiceValues(int red, int yellow, int eventValue)
    {
        if (!isServer) return;
        this.Red = "Red Dice Roll: " + red.ToString();
        this.Yellow = "Yellow Dice Roll: " + yellow.ToString();
        this.Event = "Event Dice Roll: " + ((EventKind)eventValue).ToString();

    }
    public void setStatisticValues(int[] values)
    {
        for (int i = 0; i < 32; i++)
        {
            if (i < 8)
            {

            }
            else if (i < 16)
            {

            }
            else if (i < 24)
            {

            }
            else
            {

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
                CmdBuildOnIntersection(gameObject, hit.collider.gameObject);
            }
            if (hit.collider.gameObject.CompareTag("Edge"))
            {
                CmdBuildOnEdge(gameObject, hit.collider.gameObject);
            }
        }
    }

    public void getNameToSend()
    {
        if (!isLocalPlayer) return;
        string playerName = nameWindow.transform.GetChild(0).GetChild(2).GetComponent<Text>().text;
        if (!playerName.Equals("") && playerName != null)
        {

            //open the menus
            resourcesWindow.gameObject.SetActive(true);
            MenuWindow.gameObject.SetActive(true);
            DiceWindow.gameObject.SetActive(true);
            ChatWindow.gameObject.SetActive(true);
            CardPanel.gameObject.SetActive(true);
            //closet the window
            nameWindow.SetActive(false);
            CmdSendName(playerName);
        }

    }

    public void getTradeValue()
    {
        int toGive, wanted;
        toGive = transform.GetChild(3).GetChild(2).GetComponent<Dropdown>().value;
        wanted = transform.GetChild(3).GetChild(3).GetComponent<Dropdown>().value;
        CmdSendNpcTrade(gameObject, toGive, wanted);
    }
    #endregion

    #region Commands
    [Command]
    public void CmdStartUp()
    {
        if (SceneManager.GetSceneByName("In-Game") != SceneManager.GetActiveScene()) return;
        getGameStateOnServer();
        gameState.GetComponent<Game>().setPlayer(gameObject);
        base.OnStartServer();
    }
    [Command]
    public void CmdSendName(string name)
    {
        gameState.GetComponent<Game>().setPlayerName(gameObject, name);
    }
    [Command]
    void CmdBuildOnIntersection(GameObject player, GameObject intersection)
    {
        gameState.GetComponent<Game>().buildOnIntersection(player, intersection);
    }
    [Command]
    void CmdBuildOnEdge(GameObject player, GameObject edge)
    {
        gameState.GetComponent<Game>().buildRoad(player, edge);
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
    void CmdSendMessage(GameObject player, string message)
    {
        gameState.GetComponent<Game>().chatOnServer(player, message);
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
    public void RpcUpdateTurn(string value)
    {
        transform.GetChild(8).GetComponent<Text>().text = value;
    }
    
    [ClientRpc]
    public void RpcAddProgressCard(int value)
    {
        //adds a card to panel when received;
        GameObject tempCard = Instantiate(cardPrefab);
        //set the card value and it will change its sprite accordingly
        tempCard.GetComponent<CardControl>().setCard(new Card((ProgressCardKind)value));
        //put it in the view
        Instantiate(cardPrefab).transform.SetParent(CardPanel.transform.GetChild(0).GetChild(0).GetChild(0).transform);

    }
    #endregion

    public void testCardAdd()
    {
        //adds a card to panel when received;
        GameObject tempCard = Instantiate(cardPrefab);
        tempCard.GetComponent <CardControl>().setCard(new Card(ProgressCardKind.DeserterCard));
        //put it in the view
        tempCard.transform.SetParent(CardPanel.transform.GetChild(0).GetChild(0).GetChild(0).transform);
        
    }
}
