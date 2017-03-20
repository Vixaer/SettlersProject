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
        isRobber = value;
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
}
