using System;

[Serializable]
public class StringMessage : Message
{
    public string str;

    public StringMessage()
    {
        //msgType = 3;
    }

    public StringMessage(string toSend) =>
        //msgType = 3;
        str = toSend;
}