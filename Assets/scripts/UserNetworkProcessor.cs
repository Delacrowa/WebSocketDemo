public class UserNetworkProcessor
{
    public readonly string uid; //userid
    public MessageManager msgMan = new();

    public UserNetworkProcessor(string userid) =>
        uid = userid;
}