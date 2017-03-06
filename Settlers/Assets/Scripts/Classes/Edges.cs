using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Edges : NetworkBehaviour {

    [SyncVar(hook = "OnOwned")]
    Color color;
    public bool owned = false;
    public Player belongsTo;
    public Intersection[] endPoints;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [Command]
    public void CmdBuildRoad(Player player)
    {
        belongsTo = player;
        owned = true;
        switch (belongsTo.myColor)
        {
            case 0: color = Color.red; break;
            case 1: color = Color.blue; break;
            case 2: color = Color.green; break;
            case 3: color = new Color(255, 128, 0); break;
        }
    }
    public void OnOwned(Color value)
    {
        gameObject.GetComponent<SpriteRenderer>().color = value;
        owned = true;

    }
}
