using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Intersection : NetworkBehaviour {
    public TerrainHex[] linked;
    public Edges[] paths;
    public Sprite settlement, city;

    [SyncVar(hook = "OnOwned")]
    Color color;
    
    public bool owned;

    [SyncVar(hook = "OnBuild")]
    int type = 0;
    public IntersectionUnit positionedUnit { get; private set; }
	// Use this for initialization
	void Start () {
        positionedUnit = null;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    [Command]
    public void CmdBuildSettlement(Player player)
    {
        positionedUnit = new Village(player);
        type = 1;
        owned = true;
        switch (positionedUnit.Owner.myColor)
        {
            case 0: color = Color.red; break;
            case 1: color = Color.blue; break;
            case 2: color = Color.green; break;
            case 3: color = new Color(255, 128, 0); break;
        }
    }

    public void OnOwned(Color value)
    {
        owned = true;
        gameObject.GetComponent<SpriteRenderer>().color = value;
    }
    public void CmdBuildCity(Player player)
    {
        positionedUnit = new Village(player);
        ((Village)positionedUnit).setVillageType(VillageKind.City);
        owned = true;
        type = 2;
        switch (positionedUnit.Owner.myColor)
        {
            case 0: color = Color.red; break;
            case 1: color = Color.blue; break;
            case 2: color = Color.green; break;
            case 3: color = new Color(255, 128, 0); break;
        }
    }

    public void OnBuild(int value)
    {
        type = value;
        if(value == 1)
        {
            transform.GetComponent<SpriteRenderer>().sprite = settlement;
        }
        else if( value == 2)
        {
            transform.GetComponent<SpriteRenderer>().sprite = city;
        }
        transform.GetComponent<CircleCollider2D>().radius = 0.6f;
    }
}
