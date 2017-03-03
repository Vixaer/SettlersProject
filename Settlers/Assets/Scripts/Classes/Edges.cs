using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Edges : MonoBehaviour {

    public bool isBuilt = false;
    public Intersection[] endPoints;
    public EdgeUnit positionedUnit { get; private set; }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
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
}
