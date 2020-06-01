﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public struct CopyMovementStruct
//{
//    public Vector3 localPosition;
//    public Quaternion localRotation;
//    public string anim_state;
//    public float normalizedTime;
//    public bool ignoreRotation;
//}

public class copyFromStruct : MonoBehaviour
{
    public Animator animator;
    public Collider playerHitBox;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setMovement(CopyMovement mv)
    {
        if (Server.isOn)
        {
            RaycastHit hit;
            Vector3 start = transform.position;
            Vector3 dir = (mv.localPosition - transform.localPosition);
            float dist = (mv.localPosition - transform.localPosition).magnitude + Constants.playerWidth; //1 being size of capsule rougly
            int layermask = Constants.blockMovementMask;
            bool gotHit = Physics.Raycast(start, dir, out hit, dist, layermask);
            
            Debug.DrawRay(start, dir * dist, Color.red, 1);

            if (!gotHit)
            {
                transform.localPosition = mv.localPosition;
            } else
            {
                if (hit.collider != null && hit.collider.gameObject != playerHitBox.gameObject)
                {
                    Debug.Log(playerHitBox.gameObject.GetInstanceID() + " " + this.gameObject.GetInstanceID() + " Got hit:" + hit.collider.gameObject.name + " " + hit.collider.gameObject.GetInstanceID());
                }
                else
                {
                    Debug.Log("wthit:" + layermask);
                }
            }
        } else
        {
            transform.localPosition = mv.localPosition;
        }
        
        if (!mv.ignoreRotation)
        {
            transform.localRotation = mv.localRotation;
        }
        if (mv.anim_state != null)
        {
            animator.StopPlayback();
            animator.Play(mv.anim_state, 0, mv.normalizedTime);
        }
    }

    public static IEnumerator SendAfterTime(float time, CopyMovement cp, copyFromStruct obj)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
        obj.setMovement(cp);
    }
}
