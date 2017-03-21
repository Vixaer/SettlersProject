using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Intersection : NetworkBehaviour {
    public TerrainHex[] linked;
    public Edges[] paths;
    public Sprite settlement, city, intersection;
    public Sprite[] knightSprites, activeKnightSprites;
    public bool owned;
    
    public IntersectionUnit positionedUnit { get; private set; }

    [SyncVar(hook ="OnHarbour")]
    public HarbourKind harbor = HarbourKind.None;
    [SyncVar(hook = "OnKnight")]
    public KnightLevel knight = KnightLevel.None;
    [SyncVar(hook = "OnActivateKnight")]
    public bool knightActive = false;
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
        player.ownedUnits.Add(positionedUnit);
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
        player.ownedUnits.Add(positionedUnit);
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
    public void BuildKnight(Player player)
    {
        positionedUnit = new Knight(player, KnightLevel.Basic);
        owned = true;
        knight = KnightLevel.Basic;
        switch (positionedUnit.Owner.myColor)
        {
            case 0: color = Color.red; break;
            case 1: color = Color.blue; break;
            case 2: color = Color.green; break;
            case 3: color = new Color(255, 128, 0); break;
        }
    }
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
    public void OnKnight(KnightLevel value)
    {
        knight = value;
        if (knight == KnightLevel.Basic)
        {
            transform.GetComponent<SpriteRenderer>().sprite = knightSprites[(int)knight];
            transform.GetComponent<CircleCollider2D>().radius = 0.6f;
        }
        else if (knightActive)
        {
            transform.GetComponent<SpriteRenderer>().sprite = activeKnightSprites[(int)knight];
        }
        else if (!knightActive)
        {
            transform.GetComponent<SpriteRenderer>().sprite = knightSprites[(int)knight];
        }
        else if( knight == KnightLevel.None)
        {
            transform.GetComponent<SpriteRenderer>().sprite = intersection;
            transform.GetComponent<SpriteRenderer>().color = Color.white;
            transform.GetComponent<CircleCollider2D>().radius = 0.2f;
        }        
    }
    public void OnActivateKnight(bool value)
    {
        knightActive = value;
        if (knightActive)
        {
            transform.GetComponent<SpriteRenderer>().sprite = activeKnightSprites[(int)knight];
        }
        else
        {
            transform.GetComponent<SpriteRenderer>().sprite = knightSprites[(int)knight];
        }
    }
    #endregion
}
