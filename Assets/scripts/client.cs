using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using HybridWebSocket;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//using WebSocketSharp;
// Have to use this because C# websocket libraries dont work with WEBGL.

public class Client : MonoBehaviour
{
    public static bool dead;
    public static Dictionary<string, NetworkObjectClient> objIDToObject = new();
    public static MessageManager clientMsgMan = new();
    public static string myUID = "";
    public static Dictionary<string, List<string>> myobjsByType = new();
    public static WebSocket ws;
    public static bool canPickup;
    public static PrivatePlayerInfo privateInfo = new(WeaponType.none, WeaponType.none);
    public static bool equipedSlot1 = true;
    public static float lastPingDiff;
    private static Text _displayAlert;
    private static bool shouldDisplayAlert;
    private static float distanceToAlert = -1;
    public bool autoStartClient;
    public Text displayAlert;
    public Text slot1;
    public Text slot2;
    public GameObject pickslot1;
    public GameObject pickslot2;
    public TextMeshProUGUI lifesteal;
    public TextMeshProUGUI pingDisplay;
    public Text nameInput;
    public Dictionary<string, CopyMovement> scoreDict = new();
    public TextMeshProUGUI scoreBoardDisplay;
    public GameObject damagePopUp;
    public GameObject getHurtPopUp;
    public GameObject healPopUp;
    public UIController uIController;
    public bool useLocal;
    // set via alert:
    private bool started;
    private float timeSincePing;

    public static void displayAlertThisFrame(string toDisplay, float distance)
    {
        if (_displayAlert != null && (distanceToAlert < 0 || distance < distanceToAlert))
        {
            if (!dead)
            {
                _displayAlert.text = toDisplay;
            }
            shouldDisplayAlert = true;
            canPickup = true;
            distanceToAlert = distance;
        }
    }

    public static void swapWeapon()
    {
        if (privateInfo.slot2 != WeaponType.none)
        {
            equipedSlot1 = !equipedSlot1;
        }
    }

    public void processCopyMovement(CopyMovement cp)
    {
        scoreDict[cp.objectInfo.uid] = cp;
        var k = cp.objectInfo.objectID;
        if (cp.objectInfo.uid == myUID)
        {
            if (cp.anim_state == Constants.canMoveState)
            {
                cp.ignoreRotation = true;
            }
            if (cp.anim_state == Constants.deathState)
            {
                dead = true;
                equipedSlot1 = true;
            }
            else
            {
                dead = false;
            }
            myobjsByType.AddOrCreate(Enum.GetName(typeof(NetworkObjectType), NetworkObjectType.playerCharacter), k);
            lifesteal.text = $"LifeSteal: {100 * Constants.scoreToLifesteal(cp.score)}%";
        }
        if (objIDToObject.ContainsKey(k))
        {
            objIDToObject[k].gameObject.GetComponent<copyFromStruct>()
                .setMovement(cp); // TODO: make this a list of copyFromStruct instead of game object so its faster
            objIDToObject[k].timeSinceHeartbeat = DateTime.Now;
        }
        else
        {
            // create new game object of type blah
            var ng = Instantiate(Constants.playerCharacterPrefab);
            var po = ng.GetComponent<PlayerObject>();
            po.uid = cp.objectInfo.uid;
            po.isClientObject = true;
            ng.name = "CLIENT" + ng.name;

            Debug.Log("Adding obj k:" + k);
            // add dictionary entry
            objIDToObject.Add(k, new NetworkObjectClient(ng, cp.objectInfo, DateTime.Now));
            objIDToObject[k].gameObject.GetComponent<copyFromStruct>()
                .setMovement(cp); // TODO: make this a list of copyFromStruct instead of game object so its faster
            objIDToObject[k].timeSinceHeartbeat = DateTime.Now;
        }
    }

    public void processWorldItem(WorldItem wi)
    {
        var k = wi.objectInfo.objectID;
        if (objIDToObject.ContainsKey(k))
        {
            // TODO: Do I need to do more stuff here for items on the ground?
            objIDToObject[k].timeSinceHeartbeat = DateTime.Now;
        }
        else
        {
            var ng = Instantiate(Constants.prefabsFromType[wi.itemInfo.GetType()]);
            ng.GetComponent<PickUp>().myObjId = wi.objectInfo.uid;
            ng.name = "CLIENT" + ng.name;
            ng.transform.position = wi.localPosition;
            ng.transform.localRotation = wi.localRotation;
            // add dictionary entry
            objIDToObject.Add(k, new NetworkObjectClient(ng, wi.objectInfo, DateTime.Now));
        }
    }

    public void setPrivateUI(PrivatePlayerInfo pi)
    {
        slot1.text = pi.slot1.ToString();
        slot2.text = pi.slot2.ToString();
        pickslot1.SetActive(equipedSlot1);
        pickslot2.SetActive(!equipedSlot1);
    }

    public void startClient()
    {
        // WSS won't work because fucking websocket sharp doesn't support it ffs!
        var useWSS = !useLocal && false;
        var websocket = useWSS ? "wss" : "ws";
        var port = useWSS ? "" : $":{Constants.portClient}";

        var connectionTo = $"{websocket}://{(useLocal ? Constants.localServer : Constants.remoteServer)}{port}";
        NetDebug.printBoth($"Connection to: {connectionTo}");
        ws = WebSocketFactory
            .CreateInstance(
                connectionTo); // NOTE FOR SOME INSANO REASON 127.0.0.1 wont work but localhost will with hybridsocket

        //UserInput testInp = new UserInput();
        //testInp.x = 1;
        //testInp.y = -1;
        //List<bool> buts = new List<bool>();
        //buts.Add(true);
        //buts.Add(false);
        //buts.Add(true);
        //testInp.buttonsDown = buts;

        ws.OnMessage += msg =>
        {
            Debug.Log("Got msg!");
            //NetDebug.printBoth("Client Received: " + (msg));
            try
            {
                var deser = (Message) BinarySerializer.Deserialize(msg);
                clientMsgMan.addMessage(deser);
            }
            catch (SerializationException ex)
            {
                Debug.Log("Error:" + ex);
                var smsg = "";
                foreach (var b in msg)
                {
                    smsg += b;
                }
                Debug.Log("Couldnt serialize msg:" + smsg);
            }

            //NetDebug.printBoth("Client got msg type: " + deser.msgType);
            //MessageManager.debugMsg(deser);
        };
        ws.OnOpen += () =>
        {
            var newName =
                new NameSetMessage(nameInput.text.Substring(0,
                    Mathf.Min(nameInput.text.Length, Constants.maxNameLength)));
            ws.Send(BinarySerializer.Serialize(newName));
        };
        ws.OnError += errMsg => NetDebug.printBoth("got on error " + errMsg);
        ws.OnClose += code => NetDebug.printBoth("got on close " + code);

        NetDebug.printBoth("About to start client");
        ws.Connect();
    }

    private void closeStuff()
    {
        if (ws != null && (ws.GetState() == WebSocketState.Open || ws.GetState() == WebSocketState.Connecting))
        {
            NetDebug.printBoth("Closing client");
            ws.Close();
            NetDebug.printBoth("Closed client: " + ws.GetState());
        }
    }

    private void createPopUp(DamageDealtMessage dmg)
    {
        var attacker = myUID == dmg.uidAttacker;
        var newObj = Instantiate(attacker ? damagePopUp : getHurtPopUp);
        newObj.transform.position = dmg.damageLocation;
        var textmesh = newObj.GetComponentInChildren<TextMeshPro>();
        textmesh.text = $"{dmg.damage}";

        if (attacker)
        {
            var newHeal = Instantiate(healPopUp);
            newHeal.transform.position = dmg.healLocation;
            var healtextmesh = newHeal.GetComponentInChildren<TextMeshPro>();
            healtextmesh.text = $"{dmg.healthStolen}";
        }
    }

    private void deleteNetObject(string objID)
    {
        Destroy(objIDToObject[objID].gameObject);
        objIDToObject.Remove(objID);
    }

    private void FixedUpdate()
    {
        if (!shouldDisplayAlert && !dead)
        {
            displayAlert.enabled = false;
        }
        else
        {
            displayAlert.enabled = true;
            if (dead)
            {
                _displayAlert.text = "Press E to respawn";
            }
        }
        shouldDisplayAlert = false;
        canPickup = false;
        distanceToAlert = -1;
    }

    private void LateUpdate()
    {
        var scoreList = new List<CopyMovement>(scoreDict.Values);
        scoreList.Sort((s1, s2) =>
        {
            if (s1.score == s2.score)
            {
                return 0;
            }
            if (s1.score < s2.score)
            {
                return 1;
            }
            if (s1.score > s2.score)
            {
                return -1;
            }
            return 0;
        });

        var scoreString = "";
        string myScore = null;
        for (var i = 0; i < scoreList.Count; i++)
        {
            var line = "";
            if (scoreList[i].objectInfo.uid == myUID)
            {
                line += "> ";
            }
            line += $"{i + 1} {scoreList[i].playerName}: {scoreList[i].score} \n";
            if (i < Constants.maxScoreBoardRank)
            {
                scoreString += line;
            }
            else if (scoreList[i].objectInfo.uid == myUID)
            {
                myScore += line;
            }
        }
        if (myScore != null)
        {
            scoreString += "\n";
            scoreString += myScore;
        }

        scoreBoardDisplay.text = scoreString;
    }

    private void OnApplicationQuit()
    {
        NetDebug.printBoth("Quit Client...");
        closeStuff();
    }

    private void OnDestroy()
    {
        NetDebug.printBoth("Destroyed Client...");
        closeStuff();
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (autoStartClient)
        {
            startClient();
        }
        _displayAlert = displayAlert;
        uIController.turnOffInGameUI();
    }

    // Update is called once per frame
    private void Update()
    {
        if (ws != null && ws.GetState() == WebSocketState.Open && Time.time - timeSincePing > 3)
        {
            // send ping msg
            var newPing = new PingMessage(Time.time);
            ws.Send(BinarySerializer.Serialize(newPing));
            timeSincePing = Time.time;
        }
        if (ws != null && ws.GetState() == WebSocketState.Open && !started)
        {
            started = true;
            uIController.inGameMode();
        }

        var sm = clientMsgMan.popMessage<StringMessage>();
        while (sm != null)
        {
            if (sm.str.Contains("userid:"))
            {
                myUID = sm.str.Replace("userid:", "");
                Debug.Log("Got myuid:" + myUID);
            }
            else
            {
                NetDebug.printBoth("Client got str message: " + sm.str);
            }
            sm = clientMsgMan.popMessage<StringMessage>();
        }

        var pm = clientMsgMan.popMessage<PingMessage>();
        while (pm != null)
        {
            lastPingDiff = Time.time - pm.timeSent;
            pingDisplay.text = "Ping: " + Mathf.RoundToInt(lastPingDiff * 1000);
            ;

            pm = clientMsgMan.popMessage<PingMessage>();
        }

        var ddm = clientMsgMan.popMessage<DamageDealtMessage>();
        while (ddm != null)
        {
            createPopUp(ddm);
            ddm = clientMsgMan.popMessage<DamageDealtMessage>();
        }

        var pi = clientMsgMan.popMessage<PrivatePlayerInfo>();
        while (pi != null)
        {
            privateInfo = pi;
            setPrivateUI(pi);
            pi = clientMsgMan.popMessage<PrivatePlayerInfo>();
        }

        // TODO: Scoreboard I believe will keep all usernames of even Disconnected users. Need to clear old ones.
        var cp = clientMsgMan.popMessage<CopyMovement>();
        while (cp != null)
        {
            processCopyMovement(cp);
            cp = clientMsgMan.popMessage<CopyMovement>();
        }

        // TODO: process world items here
        var wi = clientMsgMan.popMessage<WorldItem>();
        while (wi != null)
        {
            processWorldItem(wi);
            wi = clientMsgMan.popMessage<WorldItem>();
        }

        // : Check all the values of dict, if their timeSinceHeartBeat is big, delete the game object and remove from the dict
        var toDelete = new List<string>();
        foreach (var n in objIDToObject.Values)
        {
            if (DateTime.Now.Subtract(n.timeSinceHeartbeat).TotalSeconds > Constants.secondsBeforeDestroyNetworkObject)
            {
                toDelete.Add(n.objectInfo.objectID);
            }
        }
        var dm = clientMsgMan.popMessage<DeleteMessage>();
        while (dm != null)
        {
            if (dm.objId != null)
            {
                if (objIDToObject.ContainsKey(dm.objId))
                {
                    toDelete.Add(objIDToObject[dm.objId].objectInfo.objectID);
                }
            }
            else
            {
                foreach (var o in objIDToObject.Values)
                {
                    if (o.objectInfo.uid == dm.uid)
                    {
                        toDelete.Add(o.objectInfo.objectID);
                    }
                }
            }

            dm = clientMsgMan.popMessage<DeleteMessage>();
        }

        toDelete.ForEach(deleteNetObject);
        toDelete.Clear();
    }
}