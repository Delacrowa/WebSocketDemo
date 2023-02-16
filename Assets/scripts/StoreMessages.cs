using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class StoreMessages : WebSocketBehavior
{
    private static readonly List<GotMessage> newMsgs = new();

    public static void addMsg(GotMessage m)
    {
        lock (newMsgs)
        {
            newMsgs.Add(m);
        }
    }

    public static GotMessage popMsg()
    {
        lock (newMsgs)
        {
            if (newMsgs.Count > 0)
            {
                var ret = newMsgs[0];
                newMsgs.RemoveAt(0);
                return ret;
            }
            return null;
        }
    }

    protected override void OnClose(CloseEventArgs e)
    {
        addMsg(new GotMessage(ID, new CloseMessage()));
        if (Constants.testing)
        {
            ServerTest.connIds.Remove(ID);
        }
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        // TODO: Eventually have login and this will prob be username->usermanager and there will be ID -> username or something
        // For now treat every new connection as a completely new user

        //NetDebug.printBoth("Server Got msg " + e.Data + " Raw " + Encoding.UTF8.GetString(e.RawData));

        //Send(e.Data + " t: " + System.DateTime.Now.ToString("h:mm:ss tt"));

        // TODO: Add try catch here in case its not a serializable msg\
        try
        {
            var deser = (Message) BinarySerializer.Deserialize(e.RawData);
            if (deser == null)
            {
                Debug.LogWarning("Got null msg????" + deser + " raw: " + e.RawData);
            }
            else
            {
                addMsg(new GotMessage(ID, deser));
            }
        }
        catch (SerializationException ex)
        {
            Debug.Log("Error:" + ex);
            Debug.Log("Couldnt serialize msg:" + e.RawData);
        }

        /*
        Send(BinarySerializer.Serialize(new StringMessage(" Server got your msgtype: " + deser.msgType)));
        NetDebug.printBoth("Server got msg type: " + deser.msgType);
        MessageManager.debugMsg(deser);
        CopyMovement cptest = new CopyMovement();
        cptest.anim_state = "attack2";
        cptest.ignoreRotation = false;
        cptest.localPosition = new Vector3(1, 2, 3);
        cptest.localRotation = Quaternion.Euler(10, 20, 30);
        cptest.normalizedTime = .2f;
        Send(BinarySerializer.Serialize(cptest));
        */
    }

    protected override void OnOpen()
    {
        addMsg(new GotMessage(ID, new OpenMessage()));
        if (Constants.testing)
        {
            ServerTest.connIds.Add(ID);
        }
    }
}