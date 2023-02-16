using System;

[Serializable]
public class WorldItem : Message
{
    public NetworkObjectInfo objectInfo;
    public ItemInfo itemInfo;
    public SerializableVector3 localPosition;
    public SerializableQuaternion localRotation;
    public int quantity;

    public WorldItem(NetworkObjectInfo objectInfo, ItemInfo itemInfo, SerializableVector3 localPosition,
        SerializableQuaternion localRotation, int quantity)
    {
        this.objectInfo = objectInfo;
        this.itemInfo = itemInfo;
        this.localPosition = localPosition;
        this.localRotation = localRotation;
        this.quantity = quantity;
    }

    public override string ToString() =>
        "World Item: " + itemInfo + " x:" + localPosition.x + " z:" + localPosition.z + " #:" + quantity;
}