using System;
using AIPriorityList =
    System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>,
        System.Collections.Generic.List<
            System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>>>;

public delegate UserInput BotBehavior(BotState bot);

// Includes conditions that are determined by AIMemory like chasingOpponent
// Note also includes "Composite Conditions" which are a combination of OR/AND of other conditions
public delegate bool Condition(BotState bot);

public delegate bool
    BehaviorListAttribute(
        BotBehavior behavior); // PROBABLY not going to be used much because the programmer will already know ATTACK behavior attacks. Only needed for some insane advanced AI that wants to "create itself" and like "modify all my attacking behaviors to be priotized, or even change the attack it uses"

public delegate Tuple<AIPriorityList, AIMemory> BotAI(AIPriorityList ai, BotState bot);

// Clone objects

// Bot state that an AI may use to store stuff like what was my previous HP, oh my current is less, must mean I got his this frame
// Should not be modified by BotBehaviors, only by the higher level BotAIs