public class GotMessage
{
    public string uid;
    public Message m;

    public GotMessage(string uid, Message m)
    {
        this.uid = uid;
        this.m = m;
    }
}