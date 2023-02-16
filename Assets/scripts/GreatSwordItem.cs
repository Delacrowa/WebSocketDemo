using System;

[Serializable]
public class GreatSwordItem : WeaponItem
{
    public GreatSwordItem() =>
        weapon = WeaponType.greatsword;
}