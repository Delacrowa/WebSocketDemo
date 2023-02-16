using System.Collections.Generic;
using UnityEngine;

public static class Behaviors
{
    public static BotBehavior RunAway = bot =>
    {
        var ret = new UserInput();

        var enemies = BotHelpers.getEnemies(bot);

        var runTo = BotHelpers.getSafestDirection(bot.getCharacterState(0).myState.localPosition, enemies);

        //Find closest direction of the 8 a player can move to runTo
        var closestDir = BotHelpers.positionToInputDirectionsFrom0_0(runTo);

        ret.x = closestDir.x;
        ret.y = closestDir.y;

        ret.buttonsDown = defaultButtons();
        if (Conditions.EnemiesCouldAttackRange(bot))
        {
            ret.buttonsDown[3] = true; //dodge away
        }

        return ret;
    };
    public static BotBehavior dodgeAway = bot =>
    {
        var ret = new UserInput();

        var enemies = BotHelpers.getEnemies(bot);

        var runTo = BotHelpers.getSafestDirection(bot.getCharacterState(0).myState.localPosition, enemies);

        //Find closest direction of the 8 a player can move to runTo
        var closestDir = BotHelpers.positionToInputDirectionsFrom0_0(runTo);

        ret.x = closestDir.x;
        ret.y = closestDir.y;

        ret.buttonsDown = defaultButtons();

        ret.buttonsDown[3] = true; //dodge away

        return ret;
    };
    public static BotBehavior standStill = bot =>
    {
        var ret = new UserInput();
        ret.x = 0;
        ret.y = 0;
        ret.buttonsDown = defaultButtons();

        return ret;
    };
    public static BotBehavior pickUpItem = bot =>
    {
        var ret = new UserInput();
        ret.x = 0;
        ret.y = 0;
        ret.buttonsDown = defaultButtons();
        ret.buttonsDown[4] = true;
        return ret;
    };

    // attacks and chases CONTINOUSLY FOREVER, up to AI func to put a condition to stop this
    public static BotBehavior chaseTarget = bot =>
    {
        var ret = new UserInput();

        var target = BotHelpers.getSpecificEnemy(bot, bot.extraState.targetUID);
        if (target != null)
        {
            var dir = BotHelpers.positionToInputDirections(
                new Vector2(bot.getCharacterState(0).myState.localPosition.x,
                    bot.getCharacterState(0).myState.localPosition.z),
                new Vector2(target.localPosition.x, target.localPosition.z));
            ret.x = dir.x;
            ret.y = dir.y;
            if (Constants.inspectorDebugging)
            {
                Server.inspectorDebugger.addPair(new StringPair(bot.uid + "chase", "x:" + ret.x + "y" + ret.y));
            }
            ret.buttonsDown = defaultButtons();
        }
        else
        {
            //Do nothing
            ret.x = 0;
            ret.y = 0;
            ret.buttonsDown = defaultButtons();
        }

        return ret;
    };

    // This is a function that returns a BotBehavior function
    public static BotBehavior AttackTarget(int buttonIndex)
    {
        return bot =>
        {
            var ret = new UserInput();

            var target = BotHelpers.getSpecificEnemy(bot, bot.extraState.targetUID);
            if (target != null)
            {
                var dir = BotHelpers.positionToInputDirections(
                    new Vector2(bot.getCharacterState(0).myState.localPosition.x,
                        bot.getCharacterState(0).myState.localPosition.z),
                    new Vector2(target.localPosition.x, target.localPosition.z));
                ret.x = dir.x;
                ret.y = dir.y;
                if (Constants.inspectorDebugging)
                {
                    Server.inspectorDebugger.addPair(new StringPair(bot.uid + "attack", "x:" + ret.x + "y" + ret.y));
                }

                ret.buttonsDown = defaultButtons();
                ret.buttonsDown[buttonIndex] = true;
                ret.target = target.localPosition;
            }
            else
            {
                //Do nothing
                ret.x = 0;
                ret.y = 0;
                ret.buttonsDown = defaultButtons();
            }

            return ret;
        };
    }

    public static List<bool> defaultButtons() =>
        new() {false, false, false, false, false};
}