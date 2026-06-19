using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SObjStateEnum
{
    Idle=0,
    Open=1,
    Close=2,
}

[RequireComponent(typeof(Animator))]
public class SceneObjectState : MonoBehaviour
{
    public int ID;
    
    public SObjStateEnum StateEnum;

    private Animator mAnimator;

     void Awake()
     {
         mAnimator = GetComponent<Animator>();
     }

    [ContextMenu("动画")]
    public void SetAnimation()
    {
        mAnimator.Play(StateEnum.ToString());
    }

}
