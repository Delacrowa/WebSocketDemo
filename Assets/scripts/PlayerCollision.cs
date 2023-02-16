using UnityEngine;

public class PlayerCollision
{
    public PlayerObject player;
    public GameObject other;
    public float distance;

    public PlayerCollision(PlayerObject player, GameObject other, float distance)
    {
        this.player = player;
        this.other = other;
        this.distance = distance;
    }
}