using System;

[Serializable]
public class DamageDealtMessage : Message
{
    public int damage;
    public int healthStolen;
    public string uidVictim;
    public string uidAttacker;
    public SerializableVector3 damageLocation;
    public SerializableVector3 healLocation;

    public DamageDealtMessage(int damage, int healthStolen, string uidVictim, string uidAttacker,
        SerializableVector3 damageLocation, SerializableVector3 healLocation)
    {
        this.damage = damage;
        this.healthStolen = healthStolen;
        this.uidVictim = uidVictim;
        this.uidAttacker = uidAttacker;
        this.damageLocation = damageLocation;
        this.healLocation = healLocation;
    }
}