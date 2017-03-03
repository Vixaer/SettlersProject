using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

public abstract class NetworkCommand
{
    public NetworkIdentity sender { get; private set; }

    public NetworkCommand(NetworkIdentity s)
    {
        this.sender = s;
    }

    public virtual void Execute()
    {

    }
}
