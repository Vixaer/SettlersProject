using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TerrainHex : NetworkBehaviour
{
    [SyncVar(hook ="OnChangeColor")]
    Color terrainColor;
    public TerrainKind myTerrain { get; private set; }
    public int numberToken { get; private set; }
    public Intersection[] corners;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    [Command]
    public void CmdChangeColor()
    {
        if (!isServer) return;
        terrainColor = Color.red;
    }
    void OnChangeColor(Color color)
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
    }
}
