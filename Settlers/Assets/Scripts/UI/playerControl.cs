﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;

public class playerControl : NetworkBehaviour {

    private bool resourcesShown = true;
    private bool rollsShown = true;
    private GameObject gameState;
    private bool isSeletionOpen = false;
    private GameObject resourcesWindow, ChatWindow, MenuWindow, MaritimeWindow, MapSelector, DiceWindow, SelectionWindow, nameWindow;

    //synced with server values (player attributes
    //resource panel values;
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

    // Use this for initialization
    void Start () {
        if (SceneManager.GetSceneByName("In-Game") != SceneManager.GetActiveScene()) return;
        //open all menu for the client
        if (!isLocalPlayer) return;
        resourcesWindow = transform.GetChild(0).gameObject;
        ChatWindow = transform.GetChild(1).gameObject;
        MenuWindow = transform.GetChild(2).gameObject;
        MaritimeWindow = transform.GetChild(3).gameObject;
        MapSelector = transform.GetChild(4).gameObject;
        DiceWindow = transform.GetChild(5).gameObject;
        SelectionWindow = transform.GetChild(6).gameObject;
        nameWindow = transform.GetChild(7).gameObject;
        nameWindow.SetActive(true);


    }
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer) return;
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Pressed left click, casting ray.");
            detectClickedObject();
        }
        if (Input.GetButtonDown("Submit"))
        {
            string message = ChatWindow.transform.GetChild(1).GetChild(2).GetComponent<Text>().text;
            if(!message.Equals("") && message != null)
            {
                ChatWindow.transform.GetChild(1).GetComponent<InputField>().text = "";
                CmdSendMessage(gameObject, message);
            }
        }
	}
    
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
        Animation rollsAnimation = transform.GetChild(6).GetComponent<Animation>();
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

    public void closeSelectView()
    {
        isSeletionOpen = false;
    }
    void detectClickedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        if(hit)
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

    [Command]
    void CmdGiveName()
    {
        string playerName =  nameWindow.transform.GetChild(0).GetChild(2).GetComponent<Text>().text;
        if(!playerName.Equals("") && playerName != null)
        {
            gameState.GetComponent<Game>().setPlayerName(gameObject, playerName);
            //open the menus
            resourcesWindow.gameObject.SetActive(true);
            MenuWindow.gameObject.SetActive(true);
            DiceWindow.gameObject.SetActive(true);
            ChatWindow.gameObject.SetActive(true);
            //closet the window
            nameWindow.SetActive(false);
        }
        
    }
    [Command]
    void CmdBuildOnIntersection(GameObject player, GameObject intersection)
    {
        gameState.GetComponent<Game>().buildSettlement(player, intersection);
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
        gameState.GetComponent<Game>().updateSelection(player,value);
    }
    [Command]
    void CmdEndTurn(GameObject player)
    {
        gameState.GetComponent<Game>().endTurn(player);
    }
    [Command]
    void CmdSendNpcTrade(GameObject player)
    {
        int toGive, wanted;
        toGive = transform.GetChild(4).GetChild(2).GetComponent<Dropdown>().value;
        wanted = transform.GetChild(4).GetChild(3).GetComponent<Dropdown>().value;
        //obviously not going to trade 4 brick -> 1 brick
        if(toGive != wanted)
        {
            gameState.GetComponent<Game>().NpcTrade(player, toGive, wanted);
        }
        
    }

    [Command]
    void CmdSendMessage(GameObject player,string message)
    {
        gameState.GetComponent<Game>().chatOnServer(player, message);
    }


    //server ran only
    void getGameStateOnServer()
    {
        if (!isServer) return;
        gameState = GameObject.Find("GameState");

    }

    public override void OnStartServer()
    {
        if (SceneManager.GetSceneByName("In-Game") != SceneManager.GetActiveScene()) return;
        getGameStateOnServer();
        gameState.GetComponent<Game>().setPlayer(gameObject);
        base.OnStartServer();
    }

    //syncing function so that the cleitn can see his proper resources
    public void setTextValues(Dictionary<ResourceKind,int> resources, Dictionary<CommodityKind, int> commodities)
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
}