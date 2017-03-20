using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Intersection : NetworkBehaviour {
    public TerrainHex[] linked;
    public Edges[] paths;
    public Sprite settlement, city;  
    public bool owned;
    public IntersectionUnit positionedUnit { get; private set; }

    [SyncVar(hook ="OnHarbour")]
    public HarbourKind harbor = HarbourKind.None;
    [SyncVar(hook = "OnOwned")]
    Color color;
    [SyncVar(hook = "OnBuild")]
    int type = 0;

    
	// Use this for initialization
	void Start () {
        positionedUnit = null;
	}

    #region Actions
    public void BuildSettlement(Player player)
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
        //add the harbour to the list of owned harbour for when he trades it knows
        if (harbor != HarbourKind.None)
        {
            player.ownedHarbour.Add(harbor);
        }
        //remove one from the pool
        player.RemoveSettlement();
    }
    public void BuildCity(Player player)
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
        //add the harbour to the list of owned harbour for when he trades it knows
        if (harbor != HarbourKind.None)
        {
            player.ownedHarbour.Add(harbor);
        }
        player.RemoveCity();
    }
    public void UpgradeSettlement(Player player)
    {
        ((Village)positionedUnit).setVillageType(VillageKind.City);
        type = 2;
        switch (positionedUnit.Owner.myColor)
        {
            case 0: color = Color.red; break;
            case 1: color = Color.blue; break;
            case 2: color = Color.green; break;
            case 3: color = new Color(255, 128, 0); break;
        }
        player.RemoveCity();
        player.AddSettlement();
    }
    #endregion

    #region Sync Hooks
    public void OnOwned(Color value)
    {
        owned = true;
        gameObject.GetComponent<SpriteRenderer>().color = value;
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

    public void OnHarbour(HarbourKind value)
    {
        harbor = value;
    }
    #endregion
}
