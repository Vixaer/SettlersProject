using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OwnableUnit {

    public Player Owner { get; protected set; } 
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public OwnableUnit(Player owner)
    {
        this.Owner = owner;
    }
}
