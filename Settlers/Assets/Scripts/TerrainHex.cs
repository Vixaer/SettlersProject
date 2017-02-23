using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHex : MonoBehaviour {

    public TerrainKind myTerrain { get; private set; }
    public int numberToken { get; private set; }
    public Intersection[] corners { get; private set; }

	// Use this for initialization
	void Start () {
        corners = new Intersection[6];	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
