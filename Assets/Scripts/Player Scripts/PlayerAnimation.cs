﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void IdleAnimation()
    {
        anim.Play(AnimationTags.IDLE_ANIMATION);
    }

    public void PullingItemAnimation()
    {
        anim.Play(AnimationTags.ROPE_WRAP_ANIMATION);
    }
    public void ChearAnimation()
    {
        anim.Play(AnimationTags.CHEAR_ANIMATION);
    }


}


