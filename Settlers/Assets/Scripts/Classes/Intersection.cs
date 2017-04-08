using System;
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
	public bool knightRemoved = false;

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
	void Awake () {
        positionedUnit = null;
	}

    public Color getColor()
    {
        return color;
    }

    public int getType()
    {
        return type;
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
        player.ownedUnits.Add(positionedUnit);
		player.ownedKnights.Add ((Knight) positionedUnit);
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

	public void MoveKnight(Player player, Knight knightToMove){
		Knight temp = knightToMove;
		temp.deactivateKnight ();
		positionedUnit = temp;

		player.ownedUnits.Add(positionedUnit);
		player.ownedKnights.Add ((Knight) positionedUnit);
		owned = true;
		knight = temp.level;

		switch (positionedUnit.Owner.myColor)
		{
		case 0: color = Color.red; break;
		case 1: color = Color.blue; break;
		case 2: color = Color.green; break;
		case 3: color = new Color(255, 128, 0); break;
		}
	}

	public void RemoveKnight (Player player){
		owned = false;
		Knight temp = (Knight)positionedUnit;
		temp.deactivateKnight ();
		knightActive = false;
		knight = KnightLevel.None;
		knightRemoved = true;
		player.ownedUnits.Remove (positionedUnit);
		player.ownedKnights.Remove ((Knight) positionedUnit);
		positionedUnit = null;
		color = new Color(255, 255, 255);
	}


    #region Sync Hooks
    public void OnOwned(Color value)
    {
        gameObject.GetComponent<SpriteRenderer>().color = value;
		if (knightRemoved) {
			knightRemoved = false;
		} else {
			owned = true;
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
        else if (knight == KnightLevel.None)
        {
            transform.GetComponent<SpriteRenderer>().sprite = intersection;
            transform.GetComponent<SpriteRenderer>().color = Color.white;
            transform.GetComponent<CircleCollider2D>().radius = 0.2f;
        }
        else if (!knightActive)
        {
            transform.GetComponent<SpriteRenderer>().sprite = knightSprites[(int)knight];
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
            if (knight != KnightLevel.None)
                transform.GetComponent<SpriteRenderer>().sprite = knightSprites[(int)knight];
        }
    }
    #endregion

    #region Loading
    public void Load(IntersectionData data, IntersectionUnit u)
    {
        this.harbor = data.harbourKind;
        if (u != null)
        {
            if (u is Village)
            {
                var village = (Village)u;
                LoadVillage(village, data);
            }
            else if (u is Knight)
            {
                var knight = (Knight)u;
                LoadKnight(knight);
            }
        }
    }

    private void LoadVillage(Village village, IntersectionData data)
    {
        positionedUnit = village;
        type = data.type;
        owned = true;
        switch (positionedUnit.Owner.myColor)
        {
            case 0: color = Color.red; break;
            case 1: color = Color.blue; break;
            case 2: color = Color.green; break;
            case 3: color = new Color(255, 128, 0); break;
        }
    }

    private void LoadKnight(Knight k)
    {
        positionedUnit = k;
        owned = true;
        knight = k.level;
        switch (positionedUnit.Owner.myColor)
        {
            case 0: color = Color.red; break;
            case 1: color = Color.blue; break;
            case 2: color = Color.green; break;
            case 3: color = new Color(255, 128, 0); break;
        }
    }
    #endregion
}

[Serializable]
public class IntersectionData
{
    public string name { get; set; }
    public string[] linkedHexes { get; set; }
    public string[] paths { get; set; }
    public float[] color { get; set; }
    public Guid positionedUnit { get; set; }
    public float[] position { get; set; }
    public HarbourKind harbourKind { get; set; }
    public KnightLevel knight { get; set; }
    public bool knightActive { get; set; }
    public int type { get; set; }
    
    public IntersectionData(Intersection source)
    {
        this.name = source.name;
        this.color = source.getColor() != null ? 
            new float[] { source.getColor().r, source.getColor().g, source.getColor().b, source.getColor().a } : 
            null;
        this.knight = source.knight;
        this.knightActive = source.knightActive;
        this.type = source.getType();
        this.positionedUnit = source.positionedUnit == null ? 
            Guid.Empty : 
            source.positionedUnit.id;
        this.harbourKind = source.harbor;

        this.linkedHexes = new string[source.linked.Length];
        for (int i = 0; i < source.linked.Length; i++)
        {
            this.linkedHexes[i] = source.linked[i].name;
        }

        this.paths = new string[source.paths.Length];
        for (int i = 0; i < source.paths.Length; i++)
        {
            this.paths[i] = source.paths[i].name;
        }

        this.position = new float[3] { source.transform.position.x, source.transform.position.y, source.transform.position.z };
    }
}
