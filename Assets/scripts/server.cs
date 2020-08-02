﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System.Text;
using WebSocketSharp.Server;

using BehaviorList = System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>;
using ConditionalBehavior = System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>;
using Conditionals = System.Collections.Generic.Dictionary<Condition, bool>;
using ConditionalBehaviorList = System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>>;
using AIPriorityList = System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>>>;


// IDEAS: 
// - Life steal % gained per kill up to 50% or something. Incentive to get "bigger" like in slither.io. People love getting stronger.
// - Maybe Your score also influences your basic movement speed (up to a max)?

/* GUIDES:
How to Add a new Weapon:
First How To Add Any World Item:
- Add a pickup item. Can be any model, just add a trigger collider, and the script "PickUp". ALSO CHANGE LAYER TO "wall"
- Add WeaponType enum
- Add WeaponItem subclass
- Add to Constants.prefabsFromType
- Add to Constants.worldItemTypes
- Add case in tryPickUpItem. For weapon just add to PlayerObject.pickUpWeapon

How to add weapon to PlayerObject
- Add game object for the weapon (public and linked in inspector)
- Add to weaponObjects and weapons
- Add case in weaponTypeToWeapon

How to add in actual controlledPlayer prefab
- First add an empty gameobject we will call "parent"
- Then nest inside of that empty game object, with actual model of the weapon and "hitbox"
  - Move the model around so that the parent is in a location that makes sense as the weapons "handle" so that rotations and animations are easier
  - SET hitbox OBJECT TO LAYER: weapon!
- Add a box collider and a weapon script to the inner hitbox
- move it to its default walking around location
- attach the parent game object to the PlayerObject script.

Now create animations for the weapon
- create a new "override controller" for the weapon
- copy all the animations from default weapon into new files. I believe they must have same name as original animation clips!
- attach the override controller to the original weapon controller (controllerPlayerAnimator) in inspector for the file
  - attach all the copied animations to this new weapon controller in inspector for the file
- then override the new copys of the aniamtions one by one until the weapon is complete
  - To mess with the animations goto a instance of controlledPlayer, and change its animation controller to the new one and edit animations normally
  - move around the new weapon game object at its parent otherwise shit can get weird
- Actually add the override controller to Constants.weaponToAnimator. Will need to link it via inspector and make a static copy like the others.
  - Link it in the inspector too in the Server object

Bot helping:
- add the weapon animations to Constants.attackAnimationInfo so that bots can understand the timing of diff weapons
- TODO: actually have the AnimationInfo script have code to handle different weapons somehow (need to know enemies current weapon? or just use the animation clip thats playing on the enemy object?)
*/

public struct GotMessage
{
    public string uid;
    public Message m;
    public GotMessage(string uid, Message m)
    {
        this.uid = uid;
        this.m = m;
    }
}

public class StoreMessages : WebSocketBehavior
{
    public static List<GotMessage> newMsgs = new List<GotMessage>();

    protected override void OnMessage(MessageEventArgs e)
    {
        // TODO: Eventually have login and this will prob be username->usermanager and there will be ID -> username or something
        // For now treat every new connection as a completely new user
        

        //NetDebug.printBoth("Server Got msg " + e.Data + " Raw " + Encoding.UTF8.GetString(e.RawData));

        //Send(e.Data + " t: " + System.DateTime.Now.ToString("h:mm:ss tt"));
        
        // TODO: Add try catch here in case its not a serializable msg
        try
        {
            Message deser = (Message)BinarySerializer.Deserialize(e.RawData);
            if (deser == null)
            {
                Debug.LogWarning("Got null msg????" + deser + " raw: " + e.RawData);
            }
            else
            {
                newMsgs.Add(new GotMessage(ID, deser));
            }
        }
        catch (System.Runtime.Serialization.SerializationException ex)
        {
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
        newMsgs.Add(new GotMessage(ID, new OpenMessage()));
    }

    protected override void OnClose(CloseEventArgs e)
    {
        newMsgs.Add(new GotMessage(ID, new CloseMessage()));
    }
}

public class DebugLogWriter : System.IO.TextWriter
{
    public override void Write(string value)
    {
        base.Write(value);
        Debug.LogError(value);
        NetDebug.printBoth(value);
    }
    public override void WriteLine(string value)
    {
        base.WriteLine();
        Debug.LogError(value);
        NetDebug.printBoth(value);
    }

    public override System.Text.Encoding Encoding
    {
        get { return System.Text.Encoding.UTF8; }
    }
}

public static class BetterDict
{
    public static void AddOrCreate<TKey, TCollection, TValue>(
    this Dictionary<TKey, TCollection> dictionary, TKey key, TValue value)
    where TCollection : ICollection<TValue>, new()
    {
        TCollection collection;
        if (!dictionary.TryGetValue(key, out collection))
        {
            collection = new TCollection();
            dictionary.Add(key, collection);
        }
        collection.Add(value);
    }
}

public class PlayerCollision
{
    public PlayerObject player;
    public GameObject other;
    public float distance;

    public PlayerCollision(PlayerObject player, GameObject other, float distance)
    {
        this.player = player;
        this.other = other;
        this.distance = distance;
    }
}

public class Server : MonoBehaviour
{
    private static Dictionary<string, UserManager> uidToUserM = new Dictionary<string, UserManager>();
    private static Dictionary<string, List<Message>> uidToMessageQueue = new Dictionary<string, List<Message>>();
    private static List<Message> broadcastMessageQueue = new List<Message>();
    static WebSocketServer wssv = null;
    public bool autoStartServer;
    public static bool isOn = false;

    static Dictionary<string, Bot> uidToBot = new Dictionary<string, Bot>();
    static Dictionary<string, WorldItem> objToItems = new Dictionary<string, WorldItem>();

    public static InspectorDebugger inspectorDebugger;

    public static Dictionary<string, List<PlayerCollision>> playerCollisionsThisFrame = new Dictionary<string, List<PlayerCollision>>();

    private void Awake()
    {
        inspectorDebugger = gameObject.GetComponent<InspectorDebugger>();
    }

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        Debug.Log("Unity Editor");
#elif UNITY_STANDALONE_LINUX
        autoStartServer = true;
#endif
        if (autoStartServer)
        {
            startServer();
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (wssv != null && wssv.IsListening)
        {
            // TODO: If bots are inactive, check if UserManager exists, if it does, send the OnClose message to it
            // TODO: Bots generate artificial messages here. Probably just UserInput msgs.
            // Add the msgs to StoreMessages.newMsgs
            foreach (Bot bot in uidToBot.Values)
            {
                if (!Bots.botAlive(bot.state, wssv.WebSocketServices["/"].Sessions.Count, Constants.maxBots))
                {
                    if (uidToUserM.ContainsKey(bot.state.uid))
                    {
                        StoreMessages.newMsgs.Add(new GotMessage(bot.state.uid, new CloseMessage()));
                        bot.reset();
                    }
                } else
                {
                    if (bot.aiList == null)
                    {
                        // Bot is just starting up. So set its name
                        StoreMessages.newMsgs.Add(new GotMessage(bot.state.uid, new NameSetMessage(bot.state.playerName)));
                    }
                    System.Tuple<AIPriorityList, AIMemory> result = bot.ai(bot.aiList, bot.state);
                    bot.state.extraState = result.Item2;
                    UserInput uinp = Bots.getBotAction(result.Item1, bot.state);
                    if (result.Item1 == null)
                    {
                        Debug.Log("null retai");
                    }
                    bot.aiList = result.Item1;
                    //NetDebug.printBoth("Got: uinp " + ((uinp !=null) ? uinp.ToString() : "null") + " for: " + bot.state.uid);
                    inspectorDebugger.addPair(new StringPair(bot.state.uid, bot.ToString()));
                    if (uinp != null)
                    {
                        StoreMessages.newMsgs.Add(new GotMessage(bot.state.uid, uinp));
                    }

                    // clear msgs because guaranteed to get a fresh state because this runs at the same time as a "server tick"
                    // msgs is the NEW MESSAGES this tick.
                    bot.state.msgs.Clear();
                }
            }

            // Add Items
            // First count each item type
            Dictionary<System.Type, int> currentItems = new Dictionary<System.Type, int>();
            foreach(var itemT in Constants.worldItemTypes)
            {
                currentItems[itemT] = 0;
            }
            foreach (var item in objToItems.Values)
            {
                System.Type t = item.itemInfo.GetType();
                if (currentItems.ContainsKey(t))
                {
                    currentItems[t] += 1;
                }
            }
            // Now for each type, get the number of more to spawn and spawn them
            int numItems = (int)(Mathf.Max(wssv.WebSocketServices["/"].Sessions.Count, Constants.maxBots) * Constants.itemToPlayerRatio);
            foreach(var itemType in currentItems.Keys)
            {
                int toSpawn = numItems - currentItems[itemType];
                for (int i = 0; i < toSpawn; i++)
                {
                    // TODO: Maybe put 3 health items on top of each other?
                    spawnItem(itemType, 1);
                }
            }

            // Use while loop and remove 1 at a time so that its more thread safe.
            // If you clear whole list, maybe a message was added right before you cleared.
            while (StoreMessages.newMsgs.Count > 0)
            {
                transferNewMessage(StoreMessages.newMsgs[0]);
                StoreMessages.newMsgs.RemoveAt(0);
            }

            // Call update on all UserManagers.
            UserManager[] copyUM = new UserManager[uidToUserM.Values.Count];
            uidToUserM.Values.CopyTo(copyUM, 0);
            // Since could call deleteSelf, make a copy of the list to iterate through so don't modify list as you loop
            foreach (var um in copyUM)
            {
                um.customUpdate();
            }

            // Send out worldItem messages. MAKE SURE AFTER CUSTOM UPDATE OF USER SO PICKUPS ARE DONE FIRST
            foreach (var item in objToItems.Values)
            {
                broadcastMessageQueue.Add(item);
            }

            // Send all messages out at once in a big list
            foreach (var msgs in uidToMessageQueue)
            {
                try
                {
                    wssv.WebSocketServices["/"].Sessions.SendTo(BinarySerializer.Serialize(new ListMessage(msgs.Value)), msgs.Key);
                } catch (System.InvalidOperationException)
                {
                    Debug.Log("Couldnt send msg probably dced player");
                }
                msgs.Value.Clear();
            }
            wssv.WebSocketServices["/"].Sessions.Broadcast(BinarySerializer.Serialize(new ListMessage(broadcastMessageQueue)));
            broadcastMessageQueue.Clear();
        }
    }

    private void FixedUpdate()
    {
        playerCollisionsThisFrame.Clear();
    }

    public static void tryPickUpItem(GameObject pickup, PlayerObject player)
    {
        string objID = pickup.GetInstanceID() + "";
        if (objToItems.ContainsKey(objID))
        {
            var item = objToItems[objID];
            if (item.quantity > 0)
            {
                item.quantity -= 1;

                // actually process the item:
                if (item.itemInfo.GetType() == typeof(HealthItem))
                {
                    var hi = (HealthItem)item.itemInfo;
                    var hp = player.GetComponent<Health>();
                    hp.changeHealth(hi.healthBonus);
                }
                if (Constants.IsSameOrSubclass(typeof(WeaponItem), item.itemInfo.GetType()))
                {
                    var weapon = (WeaponItem)item.itemInfo;
                    player.pickUpWeapon(weapon.weapon, uidToUserM[player.uid].equipedSlot1);
                }

                if (item.quantity <= 0)
                {
                    broadcastMessageQueue.Add(new DeleteMessage(null, item.objectInfo.objectID));
                    objToItems.Remove(objID);
                    Destroy(pickup);
                }
            }
        }
    }

    public static void removeUserManager(string uid)
    {
        uidToUserM.Remove(uid);
        uidToMessageQueue.Remove(uid);
    }

    void transferNewMessage(GotMessage gm)
    {
        // handle pings simply by replying
        if (gm.m != null && gm.m.GetType() == typeof(PingMessage))
        {
            sendToSpecificUser(gm.uid, gm.m);
        }
        else
        {
            if (gm.uid == null)
            {
                if (gm.m != null)
                    Debug.LogWarning("Got null conn id msg: " + gm.m.GetType());
                else
                {
                    Debug.LogWarning("Got null conn id msg: " + gm.m);
                }
                return;
            }
            UserManager um = getUserManager(gm.uid);

            if (um == null)
            {
                addUserManager(gm.uid);
                um = Server.getUserManager(gm.uid);
            }
            um.addMessage(gm.m);
        }
    }

    public static void sendToSpecificUser(string uid, Message m)
    {
        if (uid.Contains(BotState.BOTUIDPREFIX))
        {
            // TODO: if uid is server (bot), then send to them directly
            uidToBot[uid].state.msgs.Add(m);
            tryAddToBotCharState(uid, m);
        }
        else
        {
            string connID = uidToUserM[uid].currentConnID;
            uidToMessageQueue.AddOrCreate<string, List<Message>, Message>(connID, m);
        }
    }

    public static void sendToAll(Message m)
    {
        byte[] serializedMsg = BinarySerializer.Serialize(m);
        //wssv.WebSocketServices["/"].Sessions.Broadcast(serializedMsg);
        broadcastMessageQueue.Add(m);

        // TODO: Send msg directly to all things with UID that has server in it
        foreach (var bot in uidToBot.Values)
        {
            bot.state.msgs.Add(m);
            tryAddToBotCharState(bot.state.uid, m);
        }
    }

    public static void tryAddToBotCharState(string uid, Message m)
    {
        if (uidToBot.ContainsKey(uid))
        {
            if (m.GetType() == typeof(CopyMovement)) //m.msgType == 1 && 
            {
                CopyMovement cp = (CopyMovement)m;
                if (cp.objectInfo.uid == uid)
                {
                    uidToBot[uid].state.charState.Insert(0, new CharacterState(cp));
                }
            }
        }
    }

    public static UserManager getUserManager(string uid)
    {
        if (uidToUserM.ContainsKey(uid))
        {
            return uidToUserM[uid];
        } else
        {
            return null;
        }
    }

    public static Vector3 getSpawnLocation()
    {
        float x = Random.Range(-1 * Constants.spawnXRange, Constants.spawnXRange);
        float z = Random.Range(-1 * Constants.spawnZRange, Constants.spawnZRange);
        return new Vector3(x, 0, z);
    }

    public void addUserManager(string uid)
    {
        UserManager newum = gameObject.AddComponent<UserManager>();
        uidToUserM.Add(uid, newum);
        
        newum.startup(uid, uid, Constants.playerCharacterPrefab, getSpawnLocation());
    }

    public void startServer()
    {
        isOn = true;
        System.Console.SetOut(new DebugLogWriter());
        NetDebug.printBoth("about to start wssv at " + Constants.port);
        wssv = new WebSocketServer(Constants.port); // NEED to just use this format of just putting port or it wont work properly with remote server
        wssv.AddWebSocketService<StoreMessages>("/");

        NetDebug.printBoth("starting wssv ");
        wssv.Start();
        NetDebug.printBoth("started wssv " + wssv.IsListening);

        //initialize Bots
        {
            Bot b1 = new Bot(Bots.AttackAndChaseOrRunawayBot, new BotState(0), null);
            Bot b2 = new Bot(Bots.AggroLowHealth, new BotState(1), null);
            Bot b3 = new Bot(Bots.AggroLowHealth, new BotState(2), null);
            Bot b4 = new Bot(Bots.AggroLowHealth, new BotState(3), null);
            Bot b5 = new Bot(Bots.AttackAndChaseOrRunawayBot, new BotState(4), null);

            uidToBot.Add(b1.state.uid, b1);
            uidToBot.Add(b2.state.uid, b2);
            uidToBot.Add(b3.state.uid, b3);
            uidToBot.Add(b4.state.uid, b4);
            uidToBot.Add(b5.state.uid, b5);
        }

        // TODO initialize items, eventually should spawn them periodically somewhere in the update method?
        {
            //System.Action addHealth = () =>
            //{
            //    var loc = getSpawnLocation();
            //    var he1 = Instantiate<GameObject>(Constants.prefabsFromType[typeof(HealthItem)]);
            //    he1.transform.position = loc;
            //    WorldItem h1 = new WorldItem(new NetworkObjectInfo(he1.GetInstanceID() + "", NetworkObjectType.worldItem, ""), new HealthItem(5), loc, he1.transform.localRotation, 1);
                
            //    objToItems.Add(h1.objectInfo.objectID, h1);
            //};

            //System.Action addSpear = () =>
            //{
            //    var loc = getSpawnLocation();
            //    var w1 = Instantiate<GameObject>(Constants.prefabsFromType[typeof(SpearItem)]);
            //    w1.transform.position = loc;
            //    w1.transform.Rotate(new Vector3(0, 1, 0), Random.Range(0, 360));
            //    WorldItem wi1 = new WorldItem(new NetworkObjectInfo(w1.GetInstanceID() + "", NetworkObjectType.worldItem, ""), new SpearItem(), loc, w1.transform.localRotation, 1);

            //    objToItems.Add(wi1.objectInfo.objectID, wi1);
            //};
            for (int i = 0; i< 1; i++)
            {
                spawnItem(typeof(HealthItem), 1);
                spawnItem(typeof(SpearItem), 1);
                spawnItem(typeof(GreatSwordItem), 1);
            }
        }
    }

    //void addHealth()
    //{
    //    var loc = getSpawnLocation();
    //    var he1 = Instantiate<GameObject>(Constants.prefabsFromType[typeof(HealthItem)]);
    //    he1.transform.position = loc;
    //    WorldItem h1 = new WorldItem(new NetworkObjectInfo(he1.GetInstanceID() + "", NetworkObjectType.worldItem, ""), new HealthItem(5), loc, he1.transform.localRotation, 1);

    //    objToItems.Add(h1.objectInfo.objectID, h1);
    //}

    //void addSpear()
    //{
    //    var loc = getSpawnLocation();
    //    var w1 = Instantiate<GameObject>(Constants.prefabsFromType[typeof(SpearItem)]);
    //    w1.transform.position = loc;
    //    w1.transform.Rotate(new Vector3(0, 1, 0), Random.Range(0, 360));
    //    WorldItem wi1 = new WorldItem(new NetworkObjectInfo(w1.GetInstanceID() + "", NetworkObjectType.worldItem, ""), new SpearItem(), loc, w1.transform.localRotation, 1);

    //    objToItems.Add(wi1.objectInfo.objectID, wi1);
    //}

    void spawnItem(System.Type t, int quantityInOneSpot)
    {
        var loc = getSpawnLocation();
        var w1 = Instantiate<GameObject>(Constants.prefabsFromType[t]);
        w1.transform.position = loc;
        w1.transform.Rotate(new Vector3(0, 1, 0), Random.Range(0, 360));
        WorldItem wi1 = new WorldItem(new NetworkObjectInfo(w1.GetInstanceID() + "", NetworkObjectType.worldItem, ""), (ItemInfo)System.Activator.CreateInstance(t), loc, w1.transform.localRotation, 1);

        objToItems.Add(wi1.objectInfo.objectID, wi1);
    }

    void closeStuff()
    {
        if (wssv != null && wssv.IsListening)
        {
            NetDebug.printBoth("Closing server");
            wssv.Stop();
            NetDebug.printBoth("Closed server listening: " + wssv.IsListening);
        }
    }

    void OnApplicationQuit()
    {
        NetDebug.printBoth("Quit Server...");
        closeStuff();
    }
    void OnDestroy()
    {
        NetDebug.printBoth("Destroyed Server...");
        closeStuff();
    }
}
