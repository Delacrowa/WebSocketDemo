using System;

[Serializable]
public class NetworkObjectInfo
{
    public string objectID; //gameobject id on server usually
    public NetworkObjectType objectType;
    public string uid; // user id of the "owner" sometimes blank for npc style objects

    public NetworkObjectInfo(string objectID, NetworkObjectType objectType, string uid)
    {
        this.objectID = objectID;
        this.objectType = objectType;
        this.uid = uid;
    }
}