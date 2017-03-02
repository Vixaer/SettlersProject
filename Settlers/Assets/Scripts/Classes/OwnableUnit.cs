using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OwnableUnit : MonoBehaviour {

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
