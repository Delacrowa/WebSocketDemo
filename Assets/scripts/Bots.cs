using System;
using System.Collections.Generic;
using UnityEngine;

public static class Bots
{
    // put init as extraState is null
    public static
        Tuple<List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>>, AIMemory>
        AggroLowHealth(
            List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>> ai,
            BotState bot)
    {
        if (bot.extraState == null)
        {
            // initialize bot

            var retAI =
                new List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>>();
            //:
            // First BehaviorList is: if high hp > 50%
            // 1. AttackTarget Behavior, if: CanAttackRange is true, and chasing memory
            // 2. ChaseTarget Behavior, if: chasing memory
            // 3. Dodge, if: enemy in attack range.
            // 4. Do nothing

            var highHealth = new List<Tuple<Dictionary<Condition, bool>, BotBehavior>>();

            var attackIfChasingConditions = new Dictionary<Condition, bool>();
            attackIfChasingConditions.Add(Conditions.CanAttackRangeTarget, true);
            attackIfChasingConditions.Add(Conditions.memoryIsChasing, true);
            var attackIfChasing =
                new Tuple<Dictionary<Condition, bool>, BotBehavior>(attackIfChasingConditions,
                    Behaviors.AttackTarget(0));

            var runAtIfChasingConditions = new Dictionary<Condition, bool>();
            runAtIfChasingConditions.Add(Conditions.memoryIsChasing, true);
            var runAtIfChasing =
                new Tuple<Dictionary<Condition, bool>, BotBehavior>(runAtIfChasingConditions, Behaviors.chaseTarget);

            var dodgeConditions = new Dictionary<Condition, bool>();
            dodgeConditions.Add(Conditions.EnemiesCouldAttackRange, true);
            var dodge = new Tuple<Dictionary<Condition, bool>, BotBehavior>(dodgeConditions, Behaviors.dodgeAway);

            var nothingCond = new Dictionary<Condition, bool>();
            var nothing = new Tuple<Dictionary<Condition, bool>, BotBehavior>(nothingCond, Behaviors.standStill);

            highHealth.Add(attackIfChasing);
            highHealth.Add(runAtIfChasing);
            highHealth.Add(dodge);
            highHealth.Add(nothing);

            var highHealthCondition = new Dictionary<Condition, bool>();
            //highHealthCondition.Add(Conditions.selfHealthGreaterThan(Constants.startHP / 2), true);

            retAI.Add(
                new Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>(
                    highHealthCondition, highHealth));

            // Second BehaviorList is similar except, default behavior is to run away not do nothing.
            // AI should use 2nd behavior list if lower hp than nearest enemy
            //BehaviorList lowHealth = new BehaviorList();

            //Conditionals runAwayCond = new Conditionals();
            //ConditionalBehavior runAway = new ConditionalBehavior(runAwayCond, Behaviors.RunAway);

            //lowHealth.Add(attackIfChasing);
            //lowHealth.Add(runAtIfChasing);
            //lowHealth.Add(dodge);
            //lowHealth.Add(runAway);

            //Conditionals lowHealthConds = new Conditionals();
            //lowHealthConds.Add(Conditions.selfHealthGreaterThan(Constants.startHP / 2), false);

            //retAI.Add(new ConditionalBehaviorList(lowHealthConds, lowHealth));
            //// so basically dont need to use BehaviorAttributes really...
            //// Just set chasing to when enemy misses, or enemy has low HP.

            var ret = Tuple.Create(retAI, new AIMemory());
            return ret;
        }
        var retMem = bot.extraState.Clone();

        if (!retMem.chasingTarget && !Conditions.selfAttacking(bot))
        {
            var enemies = BotHelpers.getEnemies(bot);
            if (enemies.Count > 0)
            {
                enemies.Sort((e1, e2) =>
                {
                    if (e1.health == e2.health)
                    {
                        return 0;
                    }
                    if (e1.health < e2.health)
                    {
                        return -1;
                    }
                    if (e1.health > e2.health)
                    {
                        return 1;
                    }
                    return 0;
                });

                Debug.Assert(enemies.Count <= 1 || enemies[0].health <= enemies[enemies.Count - 1].health);
                if (enemies.Count == 1)
                {
                    retMem.targetUID = enemies[0].objectInfo.uid;
                }
                else if (enemies[0].health == enemies[enemies.Count - 1].health)
                {
                    retMem.targetUID = BotHelpers
                        .getClosest(BotHelpers.getEnemies(bot), bot.getCharacterState(0).myState.localPosition)
                        .objectInfo.uid;
                }
                else
                {
                    var lowEnemies = enemies.FindAll(e => e.health <= enemies[0].health);
                    retMem.targetUID = BotHelpers.getClosest(lowEnemies, bot.getCharacterState(0).myState.localPosition)
                        .objectInfo.uid;
                }
                retMem.chasingTarget = true;
            }
        }

        if (retMem.chasingTarget && Conditions.selfAttacking(bot))
        {
            // retMem.targetUID = null; // Not sure if good? Maybe need to keep target but not chase?
            retMem.chasingTarget = false;
        }

        return Tuple.Create(ai, retMem);
    }

    // put init as extraState is null
    public static
        Tuple<List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>>, AIMemory>
        AttackAndChaseOrRunawayBot(
            List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>> ai,
            BotState bot)
    {
        if (bot.extraState == null)
        {
            // initialize bot

            var retAI =
                new List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>>();
            //TODO:
            // First BehaviorList is: if high hp > 50%
            // 1. AttackTarget Behavior, if: CanAttackRange is true, and chasing memory
            // 2. ChaseTarget Behavior, if: chasing memory
            // 3. Dodge, if: enemy in attack range.
            // 4. Do nothing
            // AI should set chasing in memory to true, if enemy ever misses an attack (cause dodged)
            // AI should set chasing to false again when we do an attack.

            var highHealth = new List<Tuple<Dictionary<Condition, bool>, BotBehavior>>();

            var attackIfChasingConditions = new Dictionary<Condition, bool>();
            attackIfChasingConditions.Add(Conditions.CanAttackRangeTarget, true);
            attackIfChasingConditions.Add(Conditions.memoryIsChasing, true);
            var attackIfChasing =
                new Tuple<Dictionary<Condition, bool>, BotBehavior>(attackIfChasingConditions,
                    Behaviors.AttackTarget(0));

            var runAtIfChasingConditions = new Dictionary<Condition, bool>();
            runAtIfChasingConditions.Add(Conditions.memoryIsChasing, true);
            var runAtIfChasing =
                new Tuple<Dictionary<Condition, bool>, BotBehavior>(runAtIfChasingConditions, Behaviors.chaseTarget);

            var dodgeConditions = new Dictionary<Condition, bool>();
            dodgeConditions.Add(Conditions.EnemiesCouldAttackRange, true);
            var dodge = new Tuple<Dictionary<Condition, bool>, BotBehavior>(dodgeConditions, Behaviors.dodgeAway);

            var nothingCond = new Dictionary<Condition, bool>();
            var nothing = new Tuple<Dictionary<Condition, bool>, BotBehavior>(nothingCond, Behaviors.standStill);

            highHealth.Add(attackIfChasing);
            highHealth.Add(runAtIfChasing);
            highHealth.Add(dodge);
            highHealth.Add(nothing);

            var highHealthCondition = new Dictionary<Condition, bool>();
            highHealthCondition.Add(Conditions.selfHealthGreaterThan(Constants.startHP / 2), true);

            retAI.Add(
                new Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>(
                    highHealthCondition, highHealth));

            // Second BehaviorList is similar except, default behavior is to run away not do nothing.
            // AI should use 2nd behavior list if lower hp than nearest enemy
            var lowHealth = new List<Tuple<Dictionary<Condition, bool>, BotBehavior>>();

            var runAwayCond = new Dictionary<Condition, bool>();
            var runAway = new Tuple<Dictionary<Condition, bool>, BotBehavior>(runAwayCond, Behaviors.RunAway);

            lowHealth.Add(attackIfChasing);
            lowHealth.Add(runAtIfChasing);
            lowHealth.Add(dodge);
            lowHealth.Add(runAway);

            var lowHealthConds = new Dictionary<Condition, bool>();
            lowHealthConds.Add(Conditions.selfHealthGreaterThan(Constants.startHP / 2), false);

            retAI.Add(
                new Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>(
                    lowHealthConds, lowHealth));
            // so basically dont need to use BehaviorAttributes really...
            // Just set chasing to when enemy misses, or enemy has low HP.
            var ret = Tuple.Create(retAI, new AIMemory());
            return ret;
        }
        var retMem = bot.extraState.Clone();

        if (Conditions.closestEnemyMissedInAttackRange(bot) && !Conditions.selfAttacking(bot))
        {
            retMem.targetUID = BotHelpers
                .getClosest(BotHelpers.getEnemies(bot), bot.getCharacterState(0).myState.localPosition).objectInfo.uid;
            retMem.chasingTarget = true;
        }
        else if (Conditions.nearByEnemyHealthLessThan(20, 40)(bot))
        {
            var enemiesClose = BotHelpers.getEnemies(bot).FindAll(cp =>
                Vector3.Distance(cp.localPosition, bot.getCharacterState(0).myState.localPosition) <= 40);
            var enemiesLow = enemiesClose.FindAll(cp => cp.health <= 20);
            retMem.targetUID = BotHelpers.getClosest(enemiesLow, bot.getCharacterState(0).myState.localPosition)
                .objectInfo.uid;
            retMem.chasingTarget = true;
        }

        if (retMem.chasingTarget && Conditions.selfAttacking(bot))
        {
            // retMem.targetUID = null; // Not sure if good? Maybe need to keep target but not chase?
            retMem.chasingTarget = false;
        }

        return Tuple.Create(ai, retMem);
    }

    public static bool botAlive(BotState b, int activeConnections, int maxBots) =>
        // If get bot number-> botnumber active connections, return true if not many connections and bot number low.
        // Lower the botNumber more likely your alive
        b.botNumber < maxBots - activeConnections;

    public static bool checkConditionals(Dictionary<Condition, bool> c, BotState bot)
    {
        var allCheck = true;
        foreach (var conditional in c)
        {
            if (conditional.Key(bot) != conditional.Value)
            {
                allCheck = false;
            }
        }
        return allCheck;
    }

    public static
        Tuple<List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>>, AIMemory>
        DoNothingBot(List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>> ai,
            BotState bot)
    {
        if (bot.extraState == null)
        {
            return Tuple.Create(
                new List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>>(),
                new AIMemory());
        }
        return Tuple.Create(ai, bot.extraState);
    }

    public static UserInput getBotAction(
        List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>> ai,
        BotState bot)
    {
        // If its the bots first frame alive, just have it stand still.
        if (bot.msgs.Count <= 0)
        {
            return Behaviors.standStill(bot);
        }

        if (bot.getCharacterState(0).myState.anim_state == Constants.deathState)
        {
            return Behaviors.pickUpItem(bot);
        }

        //: return null if BotBehavior is null, Otherwise get BotBehavior by going through ai
        List<Tuple<Dictionary<Condition, bool>, BotBehavior>> blist = null;
        if (ai != null)
        {
            foreach (var condBList in ai)
            {
                if (checkConditionals(condBList.Item1, bot))
                {
                    blist = condBList.Item2;
                    break;
                }
            }
        }

        BotBehavior b = null;
        if (blist != null)
        {
            foreach (var condBehav in blist)
            {
                if (checkConditionals(condBehav.Item1, bot))
                {
                    b = condBehav.Item2;
                    break;
                }
            }
        }

        if (b != null)
        {
            var ret = b(bot);
            return ret;
        }
        return
            null; //TODO NOTE: If a bot returns null long enough it will die from the Server thinking the "player" hasnt sent a message in a long time (dced). // maybe this is fine though, respawn bot in a diff random loc?
    }

    public static string printAIPriorityList(
        List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>> ai,
        BotState bot)
    {
        var ret = "ai:";
        // If its the bots first frame alive, just have it stand still.
        if (bot.msgs.Count <= 0)
        {
            return "no msgs ret nothing ";
        }

        //: return null if BotBehavior is null, Otherwise get BotBehavior by going through ai
        List<Tuple<Dictionary<Condition, bool>, BotBehavior>> blist = null;
        if (ai != null)
        {
            foreach (var condBList in ai)
            {
                ret += "conditional Blist :\n";
                foreach (var conditional in condBList.Item1)
                {
                    ret += "c:" + conditional.Key.Method.Name;
                    ret += " " + (conditional.Key(bot) == conditional.Value);
                }
                if (checkConditionals(condBList.Item1, bot) && blist == null)
                {
                    blist = condBList.Item2;
                    ret += "\n PICKED THIS \n";
                }
            }
        }

        BotBehavior b = null;

        if (blist != null)
        {
            foreach (var condBehav in blist)
            {
                ret += "conditional behave :\n";
                foreach (var conditional in condBehav.Item1)
                {
                    ret += "c:" + conditional.Key.Method.Name;
                    ret += " " + (conditional.Key(bot) == conditional.Value);
                }
                if (checkConditionals(condBehav.Item1, bot))
                {
                    b = condBehav.Item2;
                    ret += "\n PICKED THIS \n";
                }
            }
        }

        //if (b != null)
        //{
        //    return b(bot);
        //}
        return ret;
    }
}