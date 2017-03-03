using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Intersection : NetworkBehaviour {
    public TerrainHex[] linked;
    public Edges[] paths;
    public IntersectionUnit positionedUnit { get; private set; }
	// Use this for initialization
	void Start () {
        positionedUnit = null;
	}
	
	// Update is called once per frame
	void Update () {
		
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
}
