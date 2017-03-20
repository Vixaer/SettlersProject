using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : IntersectionUnit {

    KnightLevel level;
    bool isActive = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public Knight(Player p,KnightLevel level) : base(p)
    {
        this.level = level;
    }
    public void upgradeKnight()
    {
        level = (KnightLevel)(((int)level) + 1);
    }
    public void activateKnight()
    {
        isActive = true;
    }
    public void deactivateKnight()
    {
        isActive = false;
    }
}
