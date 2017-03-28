using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TerrainHex : NetworkBehaviour
{
    public Sprite[] terrainSprites;
    public Sprite[] tokensSprites;
    public Sprite robberSprite;
    public Sprite pirateSprite;

    [SyncVar(hook = "OnChangeKind")]
    public TerrainKind myTerrain = TerrainKind.None;

    [SyncVar(hook = "OnSetToken")]
    public int numberToken = 1;

    [SyncVar(hook = "OnChangeRobber")]
    public bool isRobber = false;

    [SyncVar(hook = "OnChangePirate")]
    public bool isPirate = false;


    public Intersection[] corners;
    public Edges[] myEdges;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    void OnChangeKind(TerrainKind value)
    {
        myTerrain = value;
        transform.GetComponent<SpriteRenderer>().sprite = terrainSprites[(int)value];

    }
    void OnSetToken(int value)
    {
        numberToken = value;
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = tokensSprites[value - 1];
    }
    void OnChangeRobber(bool value)
    {
        isRobber = value;
        if (value)
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = robberSprite;
        }
        else
        {
            if (numberToken == 1)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
            }
            else
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = tokensSprites[numberToken - 1];
            }
        }
    }

    void OnChangePirate(bool value)
    {
        isPirate = value;
        if (value)
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = pirateSprite;
        }
        else
        {
            if (numberToken == 1)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
            }
            else
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = tokensSprites[numberToken - 1];
            }
        }
    }
    public void setTile(int terrainKind, int tokenValue)
    {
        myTerrain = (TerrainKind)terrainKind;
        if((TerrainKind)terrainKind != TerrainKind.Desert && (TerrainKind)terrainKind != TerrainKind.Sea )
        {
            numberToken = tokenValue;
        }
    }

    public void Load(TerrainHexData data)
    {
        this.myTerrain = data.myTerrain;
        this.numberToken = data.numberToken;
        this.isRobber = data.isRobber;
        this.isPirate = data.isPirate;
    }
}

[Serializable]
public class TerrainHexData
{
    public string name { get; set; }
    public string[] edges { get; set; }
    public string[] corners { get; set; }
    public TerrainKind myTerrain { get; set; }
    public int numberToken { get; set; }
    public bool isRobber { get; set; }
    public bool isPirate { get; set; }
    public float[] position { get; set; }

    public TerrainHexData(TerrainHex source)
    {
        this.name = source.name;
        this.myTerrain = source.myTerrain;
        this.numberToken = source.numberToken;
        this.isRobber = source.isRobber;
        this.isPirate = source.isPirate;
        this.edges = new string[source.myEdges.Length];
        for (int i = 0; i < source.myEdges.Length; i++)
        {
            this.edges[i] = source.myEdges[i].name;
        }

        this.corners = new string[source.corners.Length];
        for (int i = 0; i < source.corners.Length; i++)
        {
            this.corners[i] = source.corners[i].name;
        }

        this.position = new float[3] { source.transform.position.x, source.transform.position.y, source.transform.position.z };
    }
}
