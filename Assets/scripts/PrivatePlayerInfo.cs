using System;

[Serializable]
public class PrivatePlayerInfo : Message
{
    public WeaponType slot1 = WeaponType.sword;
    public WeaponType slot2 = WeaponType.none;

    public PrivatePlayerInfo(WeaponType slot1, WeaponType slot2)
    {
        this.slot1 = slot1;
        this.slot2 = slot2;
    }
}