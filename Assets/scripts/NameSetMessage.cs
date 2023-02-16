using System;

[Serializable]
public class NameSetMessage : Message
{
    public string name;

    public NameSetMessage()
    {
    }

    public NameSetMessage(string name) =>
        this.name = name;
}