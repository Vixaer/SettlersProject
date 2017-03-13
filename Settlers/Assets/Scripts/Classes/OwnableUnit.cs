using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OwnableUnit {

    public Player Owner { get; protected set; } 

    public OwnableUnit(Player owner)
    {
        this.Owner = owner;
    }
}
