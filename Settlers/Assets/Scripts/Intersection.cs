using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour {

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
}
