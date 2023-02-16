using System.Collections.Generic;
using UnityEngine;

public static class BotHelpers
{
    public static CopyMovement getClosest(List<CopyMovement> enemies, Vector3 loc)
    {
        float minDistance = 0;
        CopyMovement enemyClosest = null;
        enemies.ForEach(e =>
        {
            var distance = Vector3.Distance(e.localPosition, loc);
            if (enemyClosest == null || distance < minDistance)
            {
                minDistance = distance;
                enemyClosest = e;
            }
        });

        return enemyClosest;
    }

    public static List<CopyMovement> getEnemies(BotState bot)
    {
        var cpMsgs = bot.msgs.FindAll(m => m.GetType() == typeof(CopyMovement)); // m.msgType == 1
        var copyMovements = new List<CopyMovement>();
        cpMsgs.ForEach(cp => copyMovements.Add((CopyMovement) cp)); // cast
        // Get direction away from them
        var enemies =
            copyMovements.FindAll(cp => cp.objectInfo.uid != bot.uid && cp.anim_state != Constants.deathState);
        return enemies;
    }

    // Need to keep track of state of bot...
    public static CharacterState getMyCharacterState(BotState b, List<Message> currentGameState) =>
        null;

    public static Vector2 getSafestDirection(Vector3 location, List<CopyMovement> enemies)
    {
        var runTo = new Vector2(0, 0);
        var myLoc = new Vector2(location.x, location.z);
        enemies.ForEach(e =>
        {
            var loc2d = new Vector2(e.localPosition.x, e.localPosition.z);
            loc2d = loc2d * -1;
            loc2d = loc2d * Vector2.Distance(loc2d, myLoc);
            runTo += loc2d;
        });
        return runTo;
    }

    public static CopyMovement getSpecificEnemy(BotState bot, string uid)
    {
        //TODO: this could get a "bullet" or spawned thingy of a player that isnt the player itself in the future unless changed.
        var enemies = getEnemies(bot);
        var lookingfor = enemies.FindAll(cp => cp.objectInfo.uid == uid);
        if (lookingfor.Count > 0)
        {
            return lookingfor[0];
        }
        return null;
    }

    public static Vector2 positionToInputDirections(Vector2 start, Vector2 runTo)
    {
        var closestDir = new Vector2(0, 0);
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                var dir = new Vector2(x, y);
                if (Vector2.Distance(dir + start, runTo) < Vector2.Distance(closestDir + start, runTo))
                {
                    closestDir = dir;
                }
            }
        }
        return closestDir;
    }

    public static Vector2 positionToInputDirectionsFrom0_0(Vector2 runTo)
    {
        var closestDir = new Vector2(0, 0);
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                var xy = new Vector2(x, y);
                if (Vector2.Distance(xy, runTo) < Vector2.Distance(closestDir, runTo))
                {
                    closestDir = xy;
                }
            }
        }
        return closestDir;
    }
}