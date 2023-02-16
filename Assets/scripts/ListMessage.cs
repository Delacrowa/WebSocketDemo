using System;
using System.Collections.Generic;

[Serializable]
public class ListMessage : Message
{
    public List<Message> messageArray;

    public ListMessage()
    {
        //msgType = 6;
    }

    public ListMessage(List<Message> messageArray) =>
        //msgType = 6;
        this.messageArray = messageArray;
}