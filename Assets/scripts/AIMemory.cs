using System;

[Serializable] // for cloning
public class AIMemory
{
    public float lastRecordedHP;
    public string targetUID;
    public bool chasingTarget;

    public AIMemory()
    {
    }

    public AIMemory(float lastRecordedHP, string targetUID, bool chasingTarget)
    {
        this.lastRecordedHP = lastRecordedHP;
        this.targetUID = targetUID;
        this.chasingTarget = chasingTarget;
    }

    public override string ToString()
    {
        var ret = "";
        ret += " lasthp: " + lastRecordedHP;
        ret += " target: " + targetUID;
        ret += " chasingTarget: " + chasingTarget;
        return ret;
    }
    // ...
}