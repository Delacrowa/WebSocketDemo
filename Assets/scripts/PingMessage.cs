using System;

[Serializable]
public class PingMessage : Message
{
    public float timeSent;

    public PingMessage(float timeSent) =>
        this.timeSent = timeSent;
}