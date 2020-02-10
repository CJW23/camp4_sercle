﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogInManager : MonoBehaviour
{
    [SerializeField] private InputField idInput;
    [SerializeField] private InputField pwInput;
    [SerializeField] private GameObject infoText;

    private UserInfo userInfo;
    private HTTPManager httpManager;

    private void Start()
    {
        userInfo = GameObject.Find("UserInfoObject").GetComponent<UserInfo>();
        httpManager = new HTTPManager();

        infoText.SetActive(false);
    }
    
    // 로그인 버튼 클릭시 이벤트 발생 함수
    public void SignInBtn()
    {
        userInfo.userData = JsonUtility.FromJson<UserData>(httpManager.LoginReq(idInput.text, pwInput.text));
        //로그인 성공시
        if (userInfo.userData.login == "true")
        {
            SceneManager.LoadScene("Lobby");
        }
        else
        {
            infoText.SetActive(true);
        }
    }

    // 회원가입 버튼 클릭시 이벤트 발생 함수
    public void SignUpBtn()
    {
        // 회원가입 페이지 or 씬으로 이동.
    }
}