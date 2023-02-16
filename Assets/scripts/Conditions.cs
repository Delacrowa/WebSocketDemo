using UnityEngine;

public static class Conditions
{
    public static bool CanAttackRange(BotState bot)
    {
        // TODO: Eventually see what weapon I have equiped and use that ones info;
        var range = Constants.swordInfo.avgRange;
        var closest = BotHelpers.getClosest(BotHelpers.getEnemies(bot), bot.getCharacterState(0).myState.localPosition);
        if (closest == null)
        {
            return false;
        }
        return Vector3.Distance(bot.getCharacterState(0).myState.localPosition, closest.localPosition) <= range;
    }

    public static bool CanAttackRangeTarget(BotState bot)
    {
        // TODO: Eventually see what weapon I have equiped and use that ones info;
        var range = Constants.swordInfo.avgRange;
        if (bot.extraState.targetUID == null)
        {
            return false;
        }
        var target = BotHelpers.getEnemies(bot).Find(cp => cp.objectInfo.uid == bot.extraState.targetUID);
        if (target == null)
        {
            return false;
        }
        return Vector3.Distance(bot.getCharacterState(0).myState.localPosition, target.localPosition) <= range;
    }

    public static bool closestEnemyMissedInAttackRange(BotState bot)
    {
        var closest = BotHelpers.getClosest(BotHelpers.getEnemies(bot), bot.getCharacterState(0).myState.localPosition);
        if (closest != null && closest.anim_state != null &&
            Constants.attackAnimationInfo.nameToAnimation.ContainsKey(closest.anim_state))
        {
            var attack = Constants.attackAnimationInfo.nameToAnimation[closest.anim_state];
            var leftOverTime = attack.length - closest.normalizedTime;
            var distance = Vector3.Distance(bot.getCharacterState(0).myState.localPosition, closest.localPosition);
            var runTime = distance / Constants.charMoveSpeed;
            if (leftOverTime - Constants.timeNeededToCounterAttack > runTime)
            {
                return true;
            }
        }
        return false;
    }

    public static bool EnemiesCouldAttackRange(BotState bot)
    {
        // TODO: Eventually see what weapon they have equiped and use that ones info;
        var range = Constants.swordInfo.avgRange;
        var closest = BotHelpers.getClosest(BotHelpers.getEnemies(bot), bot.getCharacterState(0).myState.localPosition);
        if (closest == null)
        {
            return false;
        }
        return Vector3.Distance(bot.getCharacterState(0).myState.localPosition, closest.localPosition) <= range;
    }

    public static bool memoryIsChasing(BotState bot) =>
        bot.extraState.chasingTarget;

    public static Condition nearByEnemyHealthLessThan(float health, float range)
    {
        return bot =>
        {
            var enemiesClose = BotHelpers.getEnemies(bot).FindAll(cp =>
                Vector3.Distance(cp.localPosition, bot.getCharacterState(0).myState.localPosition) <= range);
            var enemiesLow = enemiesClose.FindAll(cp => cp.health <= health);
            return enemiesLow.Count > 0;
        };
    }

    public static bool selfAttacking(BotState bot)
    {
        if (bot.getCharacterState(0).myState.anim_state == null)
        {
            return false;
        }

        return Constants.attackAnimationInfo.nameToAnimation.ContainsKey(bot.getCharacterState(0).myState.anim_state);
    }

    public static Condition selfHealthGreaterThan(float health)
    {
        return bot => { return bot.getCharacterState(0).myState.health >= health; };
    }
}