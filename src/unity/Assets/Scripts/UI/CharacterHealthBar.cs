﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterHealthBar : MonoBehaviour
{
    [SerializeField] private Image bar;

    private Robot robot;
    private float myHpRatio;

    private void Start()
    {
        robot = transform.parent.GetComponent<Robot>();

        if (robot.CampNum != GameManager.instance.MyCampNum)
        {
            bar.color = new Color(1, 0.4f, 0.4f);
        }

        myHpRatio = 1f;
    }

    private void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
        myHpRatio = (float)robot.GetStatus.CHP / robot.GetStatus.MHP;
        if (bar.fillAmount != myHpRatio)
        {
            DOTween.To(() => bar.fillAmount, x => bar.fillAmount = x, myHpRatio, 1f);
        }
    }
}
