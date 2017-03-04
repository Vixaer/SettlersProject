using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Intersection : NetworkBehaviour {
    public TerrainHex[] linked { get; private set; }
    public Edges[] paths { get; private set; }
    public IntersectionUnit positionedUnit { get; private set; }
    public int intersectionXPos { get; private set; }
    public int intersectionYPos { get; private set; }
    public HarbourKind intersectionKind { get; private set; }

	// Use this for initialization
	void Start () {
        positionedUnit = null;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void initialize(int x, int y, HarbourKind k){
        intersectionXPos = x;
        intersectionYPos = y;
        intersectionKind = k;
    }

    public void PlaceUnit(IntersectionUnit unit)
    {
        this.positionedUnit = unit;
    }
    
    void OnMouseDown()
    {
        var selectorPanel = GameObject.FindObjectOfType<MapSelectorPanel>();
        if (selectorPanel != null)
        {
            selectorPanel.setSelectedObject(this.gameObject);
        }
    }

    public void setHexNeighbours (TerrainHex[] list){
        linked = list;
    }

    public void setEdgeNeighbours (Edges[] list){
        paths = list;
    }

}
