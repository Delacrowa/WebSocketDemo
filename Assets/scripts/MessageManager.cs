using System;
using System.Collections.Generic;
using UnityEngine;

// Using these 2 for... silly reasons basically a way to communicate from the websocket to the unity thread

// combine messages into one big list to make traffic less crazy

// meant to hold msgs that will be READ not sent
public class MessageManager
{
    public DateTime LastMessageTime { get; private set; }
    private readonly Dictionary<Type, List<Message>> msgs = new();

    public static void debugMsg(Message deser)
    {
        if (deser.GetType() == typeof(StringMessage)) //deser.msgType == 3
        {
            NetDebug.printBoth("Got stringmsg: " + ((StringMessage) deser).str);
        }
        else if (deser.GetType() == typeof(UserInput)) // deser.msgType == 2
        {
            NetDebug.printBoth("Got UserInput: " + (UserInput) deser);
        }
        else if (deser.GetType() == typeof(CopyMovement)) // deser.msgType == 1
        {
            NetDebug.printBoth("Got CopyMovement: " + (CopyMovement) deser);
        }
    }

    public void addMessage(Message msg)
    {
        if (msg != null)
        {
            var msgType = msg.GetType();
            if (msgType == typeof(ListMessage))
            {
                var lmsg = (ListMessage) msg;
                lmsg.messageArray.ForEach(addSingleMsg);
            }
            else
            {
                addSingleMsg(msg);
            }
        }
        else
        {
            Debug.Log("Got null msg!");
        }
    }

    public void clearAllMessages()
    {
        msgs.Clear();
    }

    public int countAllMessages()
    {
        var total = 0;
        foreach (var lmsgs in msgs.Values)
        {
            total += lmsgs.Count;
            if (lmsgs.Count > 0)
            {
                Debug.Log("Leftover: " + lmsgs[0].GetType());
            }
        }
        return total;
    }

    public List<T> popAllMessages<T>() where T : Message
    {
        var msgType = typeof(T);
        if (!msgs.ContainsKey(msgType) || msgs[msgType].Count <= 0)
        {
            return null;
        }
        var ret = new List<T>();
        // Casting forces manually casting each element
        while (msgs[msgType].Count > 0)
        {
            ret.Add((T) msgs[msgType][0]);
            msgs[msgType].RemoveAt(0);
        }
        return ret;
    }

    public T popMessage<T>() where T : Message
    {
        var msgType = typeof(T);
        if (!msgs.ContainsKey(msgType) || msgs[msgType].Count <= 0)
        {
            return default;
        }
        var ret = (T) msgs[msgType][0];
        msgs[msgType].RemoveAt(0);
        return ret;
    }

    private void addSingleMsg(Message msg)
    {
        var msgType = msg.GetType();
        if (!msgs.ContainsKey(msgType))
        {
            msgs[msgType] = new List<Message>();
        }
        msgs[msgType].Add(msg);
        LastMessageTime = DateTime.Now;
    }
}