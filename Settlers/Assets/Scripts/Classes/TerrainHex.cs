using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TerrainHex : NetworkBehaviour
{
    public Sprite[] terrainSprites;
    public Sprite[] tokensSprites;
    [SyncVar(hook = "OnChangeKind")]
    public TerrainKind myTerrain = TerrainKind.None;

    [SyncVar(hook = "OnSetToken")]
    public int numberToken = 1;

    [SyncVar(hook = "OnChangeRobber")]
    bool isRobber = false;
    public Intersection[] corners;

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
