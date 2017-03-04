using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class TerrainHex : NetworkBehaviour
{

    public TerrainKind myTerrain { get; private set; }
    public int numberToken { get; private set; }
    public Intersection[] corners { get; private set; }
    public int hexXPos { get; private set; }
    public int hexYPos { get; private set; }
    public bool robber { get; private set; }
    public bool merchant { get; private set; }
    public bool pirate { get; private set; }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void initialize(int x, int y, TerrainKind terrain, int number){
        hexXPos = x;
        hexYPos = y;
        myTerrain = terrain;
        numberToken = number;
        robber = false;
        merchant = false;
        pirate = false;
    }

    public void setRobber (bool value){
        robber = value;
    }

    public void setMerchant (bool value){
        merchant = value;
    }

    public void setPirate (bool value){
        pirate = value;
    }

    public void setInterNeighbours (Intersection[] list){
        corners = list;
    }
    
}
