using System;

[Serializable]
public class DeleteMessage : Message
{
    public string uid;
    public string objId;

    public DeleteMessage()
    {
        //msgType = 7;
    }

    public DeleteMessage(string uid, string objId)
    {
        this.uid = uid;
        this.objId = objId;
    }
}