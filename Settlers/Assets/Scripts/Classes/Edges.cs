using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Edges : NetworkBehaviour {

    public bool isBuilt = false;
    public Intersection[] endPoints { get; private set; }
    public TerrainHex[] neighbourHexes { get; private set; }
    public EdgeUnit positionedUnit { get; private set; }
    public int edgeXPos { get; private set; }
    public int edgeYPos { get; private set; }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void initialize(int x, int y){
        edgeXPos = x;
        edgeYPos = y;
    }

    void OnMouseDown()
    {
        var selectorPanel = GameObject.FindObjectOfType<MapSelectorPanel>();
        if (selectorPanel != null)
        {
            selectorPanel.setSelectedObject(this.gameObject);
        }
    }

    public void PlaceUnit(EdgeUnit unit)
    {
        this.positionedUnit = unit;
    }

    public void setHexNeighbours (TerrainHex[] list){
        neighbourHexes = list;
    }

    public void setIntersectionNeighbours (Intersection[] list){
        endPoints = list;
    }

}
