using System;
using UnityEngine;

public class NetworkObjectClient
{
    public GameObject gameObject;
    public NetworkObjectInfo objectInfo;
    public DateTime
        timeSinceHeartbeat; // delete after secondsBeforeDestroyNetworkObject if you don't see any messages about the gobj anymore

    public NetworkObjectClient(GameObject gameObject, NetworkObjectInfo objectInfo, DateTime timeSinceHeartbeat)
    {
        this.gameObject = gameObject;
        this.objectInfo = objectInfo;
        this.timeSinceHeartbeat = timeSinceHeartbeat;
        if (Server.isOn)
        {
            gameObject.SetActive(false);
        }
    }
}