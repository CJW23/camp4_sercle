﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillEffect
{
    public StatusType statusType;
    public CCType ccType;
    public int amount;
    public float duration;

    public SkillEffect(StatusType statusType = StatusType.None, CCType ccType = CCType.None, int amount = 0, float duration = 0)
    {
        this.statusType = statusType;
        this.ccType = ccType;
        this.amount = amount;
        this.duration = duration;
    }

    public enum Type { Heal, Attack, Buff, Debuff, Stun }
    public Type GetSkillType()
    {
        if (ccType == CCType.Stun) return Type.Stun;
        if (statusType == StatusType.CHP && amount > 0) return Type.Heal;
        if (statusType == StatusType.CHP && amount < 0) return Type.Attack;
        if (statusType != StatusType.CHP && amount > 0) return Type.Buff;
        if (statusType != StatusType.CHP && amount < 0) return Type.Debuff;

        Debug.LogError("알맞은 스킬 타입이 없습니다. Attack 타입으로 반환합니다.");
        return Type.Attack;
    }
}