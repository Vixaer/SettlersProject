﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MainMenuBehaviour : MonoBehaviour {
    GameObject createButton,joinButton,quitButton;
    public GameObject gameState;
    public NetworkManager networkManage;
    private bool serverConnection = false;
	// Use this for initialization
	void Start () {
        //get references to the button object in the main menu
        createButton = transform.GetChild(1).GetChild(1).gameObject;
        joinButton = transform.GetChild(1).GetChild(2).gameObject;
        quitButton = transform.GetChild(1).GetChild(3).gameObject;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButton("Cancel"))
        {
            closeMenus();
        }
        if (serverConnection)
        {
            transform.GetChild(3).GetChild(4).GetChild(2).GetComponent<Text>().text = "Players Connected: " + NetworkManager.singleton.numPlayers;
        }
        

    }
    public void buttonPressed(GameObject pressed)
    {

        if(pressed == createButton)
        {
            GameObject window = transform.GetChild(3).gameObject;
            if (!window.activeSelf)
            {
                networkManage.StartHost();
            }
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(false);
            window.SetActive(true);
            transform.GetChild(4).gameObject.SetActive(false);

            serverConnection = true;
            window.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "IP Address: " + Network.player.ipAddress;
            window.transform.GetChild(4).GetChild(1).GetComponent<Text>().text = "Port: " + networkManage.networkPort;

        }
        if(pressed == joinButton)
        {
            //open correct window
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(false);
            transform.GetChild(4).gameObject.SetActive(true);
            serverConnection = false;
            networkManage.StopHost();
        }
        if(pressed == quitButton)
        {
            Application.Quit();
        }
        if(pressed.transform.GetChild(0).GetComponent<Text>().text.Equals("Exit Lobby"))
        {
            networkManage.StopHost();
            serverConnection = false;
            transform.GetChild(3).gameObject.SetActive(false);
        }
        if(pressed.transform.GetChild(0).GetComponent<Text>().text.Equals("Join Game"))
        {
            GameObject window = transform.GetChild(4).gameObject;
            Text ipAddress = window.transform.GetChild(0).GetChild(4).GetChild(2).GetComponent<Text>();
            Text port = window.transform.GetChild(0).GetChild(3).GetChild(2).GetComponent<Text>();
            //thse 2 for faster testing but need to be removed;
            ipAddress.text = "192.168.2.21";
            port.text = "7777";
            if(port.text != null && ipAddress.text != null)
            {
                networkManage.networkAddress = ipAddress.GetComponent<Text>().text;
                networkManage.networkPort = int.Parse(port.text);
            }
            networkManage.StartClient();
            transform.GetChild(4).gameObject.SetActive(false);
            transform.GetChild(5).gameObject.SetActive(true);
        }
        if (pressed.transform.GetChild(0).GetComponent<Text>().text.Equals("Start"))
        {
            networkManage.ServerChangeScene("In-Game");
        }
    }

    public void closeMenus()
    {
        transform.GetChild(2).gameObject.SetActive(false);
        transform.GetChild(3).gameObject.SetActive(false);
        transform.GetChild(4).gameObject.SetActive(false);
        networkManage.StopHost();
    }
}
