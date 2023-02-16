using System;

[Serializable]
public class CopyMovement : Message
{
    public NetworkObjectInfo objectInfo;
    public SerializableVector3 localPosition;
    public SerializableQuaternion localRotation;
    public string anim_state;
    public float normalizedTime;
    public bool ignoreRotation;
    public float
        health; // TODO THIS IS UNUSED NEED TO ACTUALLY DO STUFF! add Health component to copyFromtStruct. Also need to change inputToMovement function
    public int score;
    public WeaponType weapon;
    public string playerName;

    public CopyMovement()
    {
    }

    public CopyMovement(NetworkObjectInfo objectInfo, SerializableVector3 localPosition,
        SerializableQuaternion localRotation, string anim_state, float normalizedTime, bool ignoreRotation,
        float health, WeaponType weapon, int score, string playerName)
    {
        this.objectInfo = objectInfo;
        this.localPosition = localPosition;
        this.localRotation = localRotation;
        this.anim_state = anim_state;
        this.normalizedTime = normalizedTime;
        this.ignoreRotation = ignoreRotation;
        this.health = health;
        this.weapon = weapon;
        this.score = score;
        this.playerName = playerName;
    }

    public override string ToString() =>
        "loc:" + localPosition + " rot: " + localRotation + " anim: " + anim_state + " ntime: " + normalizedTime
        + " ignoreRot: " + ignoreRotation + " health: " + health + " weapon:" + weapon + " scoore:" + score +
        " playerName:" + playerName;
}