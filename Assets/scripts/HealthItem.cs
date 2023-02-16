using System;

[Serializable]
public class HealthItem : ItemInfo
{
    public float healthBonus;

    public HealthItem(float healthBonus) =>
        this.healthBonus = healthBonus;

    public HealthItem() =>
        healthBonus = Constants.healthItemPickUpAmount;
}