using System.Collections.Generic;
using UnityEngine;

public class BotState
{
    // Somethinbg for my history of anims
    // something for 
    public const string BOTUIDPREFIX = "SERVERBOT:";
    public readonly string uid;
    public readonly int botNumber;
    public readonly string playerName;
    private readonly List<CharacterState> charState = new(); // TODO :make sure new stuff is added at 0
    public List<Message> msgs = new();
    public AIMemory extraState = null; // is null when first ran

    public BotState(int botNumber)
    {
        uid = BOTUIDPREFIX + botNumber;
        playerName = "BigBoty" + botNumber; // TODO: Put in random names that seem like realistic players
        this.botNumber = botNumber;
        charState.Add(new CharacterState(new CopyMovement(null, new Vector3(0, 0, 0), new Quaternion(),
            Constants.canMoveState, 0, false, 0, WeaponType.sword, 0,
            playerName))); // add one so dont have to do length > 0 all the time
    }

    public void addCharacterState(CharacterState c)
    {
        charState.Insert(0, c);
        if (charState.Count > Constants.maxBotCharacterState)
        {
            charState.RemoveRange(Constants.maxBotCharacterState, charState.Count - Constants.maxBotCharacterState);
        }
    }

    public CharacterState getCharacterState(int i) =>
        charState[i];

    public override string ToString()
    {
        var ret = "";
        ret += " uid:" + uid;
        ret += " msgs: " + msgs.Count;
        ret += " charstates: " + msgs.Count;
        ret += " extra state: " + extraState;
        return ret;
    }
}