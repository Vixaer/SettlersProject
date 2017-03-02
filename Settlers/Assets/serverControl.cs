using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class serverControl : MonoBehaviour {
    private Game gameState;
	// Use this for initialization
	void Start () {
        gameState = GameObject.Find("NetworkManager").GetComponent<myNetworkManager>().getGame();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void testClick()
    {

    }
}
