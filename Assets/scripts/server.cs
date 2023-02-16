﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WebSocketSharp.Server;
using BehaviorList =
    System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>;
using ConditionalBehavior = System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>;
using Conditionals = System.Collections.Generic.Dictionary<Condition, bool>;
using ConditionalBehaviorList =
    System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, System.Collections.Generic.List<
        System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>>;
using AIPriorityList =
    System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>,
        System.Collections.Generic.List<
            System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>>>;
using Random = UnityEngine.Random;

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
= Add case to Server.weaponTypeToItemType
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
- add weaponinfo like Constants.swordInfo so they know enemy ranges and stuff
- TODO: actually have the AnimationInfo script have code to handle different weapons somehow (need to know enemies current weapon? or just use the animation clip thats playing on the enemy object?)
*/

public class Server : MonoBehaviour
{
    private static readonly Dictionary<string, UserManager> uidToUserM = new();
    private static readonly Dictionary<string, List<Message>> uidToMessageQueue = new();
    private static readonly List<Message> broadcastMessageQueue = new();
    private static readonly Dictionary<string, Bot> uidToBot = new();
    private static readonly Dictionary<string, WorldItem> objToItems = new();
    public static bool isOn;
    public static InspectorDebugger inspectorDebugger;
    public static Dictionary<string, List<PlayerCollision>> playerCollisionsThisFrame = new();
    private static WebSocketServer wssv;
    private static bool dontGC;
    public bool autoStartServer;
    public GameObject sceneCamera;

    public static void dropWeaponAt(WeaponType wt, Vector3 spawnLocation)
    {
        spawnLocation.y = 0;
        var wtype = weaponTypeToItemType(wt);
        if (wtype != null)
        {
            spawnItem(wtype, 1, spawnLocation);
        }
    }

    public static Vector3 getSpawnLocation()
    {
        var x = Random.Range(-1 * Constants.spawnXRange, Constants.spawnXRange);
        var z = Random.Range(-1 * Constants.spawnZRange, Constants.spawnZRange);
        return new Vector3(x, 0, z);
    }

    public static UserManager getUserManager(string uid)
    {
        if (uidToUserM.ContainsKey(uid))
        {
            return uidToUserM[uid];
        }
        return null;
    }

    public static void removeUserManager(string uid)
    {
        uidToUserM.Remove(uid);
        uidToMessageQueue.Remove(uid);
    }

    public static void sendToAll(Message m)
    {
        if (Constants.testing)
        {
            ServerTest.sendToAll(m);
        }
        var serializedMsg = BinarySerializer.Serialize(m);
        //wssv.WebSocketServices["/"].Sessions.Broadcast(serializedMsg);
        broadcastMessageQueue.Add(m);

        // TODO: Send msg directly to all things with UID that has server in it
        foreach (var bot in uidToBot.Values)
        {
            bot.state.msgs.Add(m);
            tryAddToBotCharState(bot.state.uid, m);
        }
    }

    public static void sendToSpecificUser(string uid, Message m)
    {
        if (Constants.testing)
        {
            ServerTest.sendToSpecificUser(uid, m);
        }
        if (uid.Contains(BotState.BOTUIDPREFIX))
        {
            // TODO: if uid is server (bot), then send to them directly
            uidToBot[uid].state.msgs.Add(m);
            tryAddToBotCharState(uid, m);
        }
        else
        {
            var connID = uidToUserM[uid].currentConnID;
            uidToMessageQueue.AddOrCreate(connID, m);
        }
    }

    public static void tryAddToBotCharState(string uid, Message m)
    {
        if (uidToBot.ContainsKey(uid))
        {
            if (m.GetType() == typeof(CopyMovement)) //m.msgType == 1 && 
            {
                var cp = (CopyMovement) m;
                if (cp.objectInfo.uid == uid)
                {
                    uidToBot[uid].state.addCharacterState(new CharacterState(cp));
                }
            }
        }
    }

    public static void tryPickUpItem(GameObject pickup, PlayerObject player)
    {
        var objID = pickup.GetInstanceID() + "";
        if (objToItems.ContainsKey(objID))
        {
            var item = objToItems[objID];
            if (item.quantity > 0)
            {
                item.quantity -= 1;

                // actually process the item:
                if (item.itemInfo.GetType() == typeof(HealthItem))
                {
                    var hi = (HealthItem) item.itemInfo;
                    var hp = player.GetComponent<Health>();
                    hp.changeHealth(hi.healthBonus);
                }
                if (Constants.IsSameOrSubclass(typeof(WeaponItem), item.itemInfo.GetType()))
                {
                    var weapon = (WeaponItem) item.itemInfo;
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

    public static Type weaponTypeToItemType(WeaponType wt)
    {
        switch (wt)
        {
            case WeaponType.none:
                return null;
            case WeaponType.sword:
                return null;
            case WeaponType.spear:
                return typeof(SpearItem);
            case WeaponType.greatsword:
                return typeof(GreatSwordItem);
            default:
                Debug.LogError("Got weapontype unknown! " + wt);
                return null;
        }
    }

    private static void spawnItem(Type t, int quantityInOneSpot, Vector3 spawnLocation)
    {
        var w1 = Instantiate(Constants.prefabsFromType[t]);
        w1.transform.position = spawnLocation;
        w1.transform.Rotate(new Vector3(0, 1, 0), Random.Range(0, 360));
        var wi1 = new WorldItem(new NetworkObjectInfo(w1.GetInstanceID() + "", NetworkObjectType.worldItem, ""),
            (ItemInfo) Activator.CreateInstance(t), spawnLocation, w1.transform.localRotation, 1);

        objToItems.Add(wi1.objectInfo.objectID, wi1);
    }

    private static void spawnItem(Type t, int quantityInOneSpot)
    {
        spawnItem(t, quantityInOneSpot, getSpawnLocation());
    }

    public void addUserManager(string uid)
    {
        var newum = gameObject.AddComponent<UserManager>();
        uidToUserM.Add(uid, newum);

        newum.startup(uid, uid, Constants.playerCharacterPrefab, getSpawnLocation());
    }

    public void startServer()
    {
        isOn = true;
        Console.SetOut(new DebugLogWriter());
        NetDebug.printBoth("about to start wssv at " + Constants.portServer);
        // Don't use the secure bool for WSS, have to do all sorts of cert bs.
        wssv = new WebSocketServer(Constants
            .portServer); // NEED to just use this format of just putting port or it wont work properly with remote server
        wssv.AddWebSocketService<StoreMessages>("/");

        NetDebug.printBoth("starting wssv ");
        wssv.Start();
        NetDebug.printBoth("started wssv " + wssv.IsListening);

        //initialize Bots
        {
            var b1 = new Bot(Bots.AttackAndChaseOrRunawayBot, new BotState(0), null);
            var b2 = new Bot(Bots.AggroLowHealth, new BotState(1), null);
            var b3 = new Bot(Bots.AggroLowHealth, new BotState(2), null);
            var b4 = new Bot(Bots.AggroLowHealth, new BotState(3), null);
            var b5 = new Bot(Bots.AttackAndChaseOrRunawayBot, new BotState(4), null);

            //Bot b1 = new Bot(Bots.DoNothingBot, new BotState(0), null);

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

            for (var i = 0; i < 1; i++)
            {
                spawnItem(typeof(HealthItem), 1);
                spawnItem(typeof(SpearItem), 1);
                spawnItem(typeof(GreatSwordItem), 1);
            }
        }
        if (Application.isBatchMode)
        {
            // Do stuff when in headless mode, turn off all graphics stuff.
            var tmr = (TextMeshPro[]) FindObjectsOfType(typeof(TextMeshPro));
            foreach (var r in tmr)
            {
                Destroy(r);
            }

            var rs = (Renderer[]) FindObjectsOfType(typeof(Renderer));
            foreach (var r in rs)
            {
                r.enabled = false;
                Destroy(r);
            }
            var smr = (SkinnedMeshRenderer[]) FindObjectsOfType(typeof(SkinnedMeshRenderer));
            foreach (Renderer r in smr)
            {
                r.enabled = false;
                Destroy(r);
            }
            var cr = (CanvasRenderer[]) FindObjectsOfType(typeof(CanvasRenderer));
            foreach (var rend in cr)
            {
                rend.gameObject.SetActive(false);
            }
            Debug.Log("BatchMode!    !! !! !   ! ! !");
        }
    }

    private void Awake()
    {
        inspectorDebugger = gameObject.GetComponent<InspectorDebugger>();
        if (Application.isBatchMode && sceneCamera != null)
        {
            Destroy(sceneCamera);
        }
    }

    private void closeStuff()
    {
        if (wssv != null && wssv.IsListening)
        {
            NetDebug.printBoth("Closing server");
            wssv.Stop();
            NetDebug.printBoth("Closed server listening: " + wssv.IsListening);
        }
    }

    private void FixedUpdate()
    {
        playerCollisionsThisFrame.Clear();
    }

    private void OnApplicationQuit()
    {
        NetDebug.printBoth("Quit Server...");
        closeStuff();
    }

    private void OnDestroy()
    {
        NetDebug.printBoth("Destroyed Server...");
        closeStuff();
    }

    // Start is called before the first frame update
    private void Start()
    {
        Application.targetFrameRate = Constants.targetFrameRate;
#if UNITY_EDITOR
        Debug.Log("Unity Editor");
#elif UNITY_STANDALONE_LINUX
        autoStartServer = true;
#endif
        if (Application.isBatchMode)
        {
            autoStartServer = true;
        }
        if (Constants.testing)
        {
            autoStartServer = false;
        }
        if (autoStartServer)
        {
            Debug.Log("Auto starting server");
            startServer();
        }
    }

    private void transferNewMessage(GotMessage gm)
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
                {
                    Debug.LogWarning("Got null conn id msg: " + gm.m.GetType());
                }
                else
                {
                    Debug.LogWarning("Got null conn id msg: " + gm.m);
                }
                return;
            }
            var um = getUserManager(gm.uid);

            if (um == null)
            {
                addUserManager(gm.uid);
                um = getUserManager(gm.uid);
            }
            um.addMessage(gm.m);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // memory leak in server mode
        if (Application.isBatchMode)
        {
            var tmr = (TextMeshPro[]) FindObjectsOfType(typeof(TextMeshPro));
            foreach (var r in tmr)
            {
                Destroy(r);
            }

            var rs = (Renderer[]) FindObjectsOfType(typeof(Renderer));
            foreach (var r in rs)
            {
                r.enabled = false;
                Destroy(r);
            }
            var smr = (SkinnedMeshRenderer[]) FindObjectsOfType(typeof(SkinnedMeshRenderer));
            foreach (Renderer r in smr)
            {
                r.enabled = false;
                Destroy(r);
            }
            var cr = (CanvasRenderer[]) FindObjectsOfType(typeof(CanvasRenderer));
            foreach (var rend in cr)
            {
                rend.gameObject.SetActive(false);
            }
        }

        if (wssv != null && wssv.IsListening)
        {
            // TODO: If bots are inactive, check if UserManager exists, if it does, send the OnClose message to it
            // TODO: Bots generate artificial messages here. Probably just UserInput msgs.
            // Add the msgs to StoreMessages.newMsgs

            foreach (var bot in uidToBot.Values)
            {
                if (!Bots.botAlive(bot.state, wssv.WebSocketServices["/"].Sessions.Count, Constants.maxBots))
                {
                    if (uidToUserM.ContainsKey(bot.state.uid))
                    {
                        StoreMessages.addMsg(new GotMessage(bot.state.uid, new CloseMessage()));
                        bot.reset();
                    }
                }
                else
                {
                    if (bot.aiList == null)
                    {
                        // Bot is just starting up. So set its name
                        StoreMessages.addMsg(new GotMessage(bot.state.uid, new NameSetMessage(bot.state.playerName)));
                    }
                    var result = bot.ai(bot.aiList, bot.state);
                    bot.state.extraState = result.Item2;
                    var uinp = Bots.getBotAction(result.Item1, bot.state);
                    if (result.Item1 == null)
                    {
                        Debug.Log("null retai");
                    }
                    bot.aiList = result.Item1;

                    if (Constants.inspectorDebugging)
                    {
                        inspectorDebugger.addPair(new StringPair(bot.state.uid, bot.ToString()));
                    }
                    if (uinp != null)
                    {
                        StoreMessages.addMsg(new GotMessage(bot.state.uid, uinp));
                    }

                    // clear msgs because guaranteed to get a fresh state because this runs at the same time as a "server tick"
                    // msgs is the NEW MESSAGES this tick.
                }
                bot.state.msgs.Clear();
            }

            // Add Items
            // First count each item type
            var currentItems = new Dictionary<Type, int>();
            foreach (var itemT in Constants.worldItemTypes)
            {
                currentItems[itemT] = 0;
            }
            foreach (var item in objToItems.Values)
            {
                var t = item.itemInfo.GetType();
                if (currentItems.ContainsKey(t))
                {
                    currentItems[t] += 1;
                }
            }
            // get items equiped by players
            foreach (var um in uidToUserM.Values)
            {
                var po = um.playerObject;
                if (po != null)
                {
                    var witype = weaponTypeToItemType(po.privateInfo.slot1);
                    if (witype != null && currentItems.ContainsKey(witype))
                    {
                        currentItems[witype] += 1;
                    }
                    witype = weaponTypeToItemType(po.privateInfo.slot2);
                    if (witype != null && currentItems.ContainsKey(witype))
                    {
                        currentItems[witype] += 1;
                    }
                }
            }

            // Now for each type, get the number of more to spawn and spawn them
            var numItems = (int) (Mathf.Max(wssv.WebSocketServices["/"].Sessions.Count, Constants.maxBots) *
                                  Constants.itemToPlayerRatio);

            foreach (var itemType in currentItems.Keys)
            {
                var toSpawn = numItems - currentItems[itemType];
                for (var i = 0; i < toSpawn; i++)
                {
                    // TODO: Maybe put 3 health items on top of each other?
                    spawnItem(itemType, 1);
                }
            }

            // Use while loop and remove 1 at a time so that its more thread safe.
            // If you clear whole list, maybe a message was added right before you cleared.
            var mm = StoreMessages.popMsg();
            while (mm != null)
            {
                transferNewMessage(mm);
                mm = StoreMessages.popMsg();
            }

            // Call update on all UserManagers.
            var copyUM = new UserManager[uidToUserM.Values.Count];
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
                    var toSend = BinarySerializer.Serialize(new ListMessage(msgs.Value));
                    wssv.WebSocketServices["/"].Sessions.SendTo(toSend, msgs.Key);
                }
                catch (InvalidOperationException)
                {
                    Debug.Log("Couldnt send msg probably dced player");
                }
                msgs.Value.Clear();
            }
            wssv.WebSocketServices["/"].Sessions
                .Broadcast(BinarySerializer.Serialize(new ListMessage(broadcastMessageQueue)));
            broadcastMessageQueue.Clear();
        }

        if (false && (int) Time.time % 10 == 0)
        {
            var maxMem = 15 * 1000000 * Mathf.Max(uidToUserM.Values.Count, 1);
            var getTotalMemory = GC.GetTotalMemory(false);
            if (Time.frameCount % 1000 == 0 &&
                (Constants.testing && Time.frameCount % 100000 == 0 || !Constants.testing))
            {
                Debug.Log("Max: " + maxMem + " Mem: " + getTotalMemory + "frames " + Time.frameCount + " persec: " +
                          Time.frameCount / Time.time);
                var totalMsgs = 0;
                foreach (var li in uidToMessageQueue.Values)
                {
                    totalMsgs += li.Count;
                }
                Debug.Log("Total msgs: " + totalMsgs + " keys: " + uidToMessageQueue.Keys.Count);
            }
            if (getTotalMemory > maxMem && !dontGC)
            {
                Debug.Log(
                    $"Manually triggering GC memory:{getTotalMemory} min: {maxMem} at {DateTime.Now.ToString("h:mm:ss tt")}");
                GC.Collect();
                if (getTotalMemory > maxMem)
                {
                    dontGC = true;
                }
            }
        }
    }
}