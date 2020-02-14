﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Werewolf.StatusIndicators.Components;

public enum RangeType { Around, Direction }
public enum TargetType { Friend, Enemy }
public enum TargetNum { One, All }

[System.Serializable]
public class Skill
{
    public enum State { Idle, Ready, CoolDown }

    [Header("Basic Info")]
    public string skillName;
    public string description;
    public Sprite image;
    public State state;

    [Header("Time")]
    public float preDelay;
    public float postDelay;
    public float coolDown;
    private float remainCool;

    [Header("Projectile Info")]
    public Projectile proj;
    public float speed;
    public float range;
    public float size;
    public TargetType targetType;
    public TargetNum targetNum;
    public List<SkillEffect> skillEffects;

    public float RemainCool { get { return remainCool; } }
    public Sprite Image { get { return image; } }


    public void Initialize()
    {
        state = State.Idle;
        remainCool = 0f;
    }
}