using System;
using System.Collections.Generic;
using UnityEngine;

public delegate T best<T>(T a, T b);

public class Constants : MonoBehaviour
{
    public const int targetFrameRate = 120;
    public const int secondsUntilConsiderDC = 60; //was 60
    public const int portClient = 7271;
    public const int portServer = 7270; //need to be diff cause nginx is throwing address in use errors?
    public const string remoteServer = "bobserver.greygods.com"; //"bobserver.greygods.com"; // ip is "206.189.205.115"
    public const string localServer = "localhost";
    public const int maxNameLength = 15;
    public const int maxScoreBoardRank = 2;
    public const float charMoveSpeed = 10;
    public const float startHP = 100;
    public const int baseScore = 100;
    public const int startScore = 0;
    public const float damageScoreFactor = .2f;
    public const float scoreToLifeSteal = .002f;
    public const float maxLifeSteal = .5f;
    public const int inputLifetimeMS = 400;
    public const string canMoveState = "defAnim";
    public const string getHitState = "getHit";
    public const string pickUpState = "pickup";
    public const string deathState = "die";
    public const float timeNeededToCounterAttack = .1f;
    public const int healthItemPickUpAmount = 15;
    public const float
        itemToPlayerRatio =
            .5f; // TODO: This is kinda dumb right now, ideally it should count weapons being help by players as well!
    public const int secondsBeforeDestroyNetworkObject = 10;
    public const float playerWidth = .5f; // used for physics to check if wall in the way
    public const int maxBotCharacterState = 20;
    public const int maxInputBuffer = 100;
    public const int maxBots = 3;
    public const float spawnXRange = 20;
    public const float spawnZRange = 11;
    public const bool inspectorDebugging = false;
    public const bool testing = false;
    public static readonly string[] charUserControlledStateNames =
    {
        "attack1", "attack2", "attack3", "dodge", pickUpState, deathState
    }; // TODO Change to enum... and also use that enum to index CopyMovement ButtonsDown
    public static readonly string[] dodgeFromStates = {getHitState, canMoveState, pickUpState};
    public static readonly Dictionary<WeaponType, RuntimeAnimatorController> weaponToAnimator = new();
    public static readonly WeaponInfo swordInfo = new(7, 7, 25, 40);
    public static RuntimeAnimatorController spear_animator;
    public static RuntimeAnimatorController sword_animator;
    public static RuntimeAnimatorController greatsword_animator;
    public static GameObject playerCharacterPrefab;
    public static Dictionary<Type, GameObject> prefabsFromType = new();
    public static List<Type> worldItemTypes = new() {typeof(HealthItem), typeof(SpearItem), typeof(GreatSwordItem)};
    public static AnimationInfo attackAnimationInfo;
    public static int blockMovementMask;
    public RuntimeAnimatorController _spear_animator;
    public RuntimeAnimatorController _sword_animator;
    public RuntimeAnimatorController _greatsword_animator;
    public AnimationInfo _attackAnimationInfo; // set in inspector

    public static int findBest<T>(List<T> li, best<T> f)
    {
        if (li.Count > 0)
        {
            var best = li[0];
            var ret = 0;
            for (var i = 0; i < li.Count; i++)
            {
                var t = li[i];
                best = f(best, t);
                ret = i;
            }
            return ret;
        }
        return -1;
    }

    public static T getComponentInParentOrChildren<T>(GameObject g) where T : Component
    {
        T r = null;
        r = g.GetComponentInChildren<T>();
        if (r == null)
        {
            r = g.GetComponentInParent<T>();
        }
        return r;
    }

    public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant) =>
        potentialDescendant.IsSubclassOf(potentialBase)
        || potentialDescendant == potentialBase;

    public static float scoreToLifesteal(int score) =>
        Mathf.Min(scoreToLifeSteal * score, maxLifeSteal);

    public static TimeSpan timeDiff(DateTime timenow, DateTime timepast) =>
        timenow.Subtract(timepast);

    private void Awake()
    {
        playerCharacterPrefab = Resources.Load<GameObject>("controlledPlayer");

        prefabsFromType.Add(typeof(HealthItem), Resources.Load<GameObject>("HealthPickUp"));
        prefabsFromType.Add(typeof(SpearItem), Resources.Load<GameObject>("SpearPickUp"));
        prefabsFromType.Add(typeof(GreatSwordItem), Resources.Load<GameObject>("GreatSwordPickUp"));

        blockMovementMask = LayerMask.GetMask("wall", "player");
        attackAnimationInfo = _attackAnimationInfo;
        spear_animator = _spear_animator;
        sword_animator = _sword_animator;
        greatsword_animator = _greatsword_animator;

        weaponToAnimator.Add(WeaponType.sword, sword_animator);
        weaponToAnimator.Add(WeaponType.spear, spear_animator);
        weaponToAnimator.Add(WeaponType.greatsword, greatsword_animator);
    }
}