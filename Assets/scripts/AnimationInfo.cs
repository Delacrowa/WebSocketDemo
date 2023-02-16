using System.Collections.Generic;
using UnityEngine;

public class AnimationInfo : MonoBehaviour
{
    public List<AnimationClip> animations = new();
    public Dictionary<string, AnimationClip> nameToAnimation = new();

    private void Start()
    {
        animations.ForEach(anim => nameToAnimation.Add(anim.name, anim));
    }
}