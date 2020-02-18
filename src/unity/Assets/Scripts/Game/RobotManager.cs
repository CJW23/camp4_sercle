﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotManager : MonoBehaviour
{
    [Header("Line Pos")]
    [SerializeField] private List<Transform> line1;
    [SerializeField] private List<Transform> line2;
    [SerializeField] private List<Transform> line3;

    [Header("Wave Info")]
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private int waveSize;
    [SerializeField] private float waveTerm;

    private List<GameObject> myRobots = new List<GameObject>();
    private List<GameObject> enemyRobots = new List<GameObject>();
    private int robotNum = 0;

    private NetworkManager networkManager;

    private void Awake()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }

    private void Start()
    {
        // 로봇 생성 신호 수신함수 등록.
        networkManager.RegisterReceiveNotification(PacketId.SpawnRobotsData, OnReceiveRobotSpawnPacket);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StartCoroutine(SpawnRobots());
        }
    }

    private IEnumerator SpawnRobots()
    {
        for (int i = 0; i < waveSize; i++)
        {
            //SpawnRobotPair(LinePos(line1));
            SpawnRobotPair(LinePos(line2));
            //SpawnRobotPair(LinePos(line3));

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void SpawnRobotPair(List<Vector3> linePos)
    {
        int count = linePos.Count;

        GameObject myRobot = Instantiate(robotPrefab, linePos[0], Quaternion.identity);
        myRobot.GetComponent<Robot>().InitialSetting(robotNum, true, linePos);
        myRobots.Add(myRobot);

        GameObject enemyRobot = Instantiate(robotPrefab, linePos[count - 1], Quaternion.identity);
        enemyRobot.GetComponent<Robot>().InitialSetting(robotNum, false, linePos);
        enemyRobots.Add(enemyRobot);
        
        robotNum += 1;
    }

    private List<Vector3> LinePos(List<Transform> line)
    {
        List<Vector3> linePos = new List<Vector3>();
        for (int i = 0; i < line.Count; i++)
        {
            linePos.Add(line[i].position);
        }

        return linePos;
    }

    public void MyRobotFire(int index, Vector3 pos, Vector3 dir)
    {
        Robot caster = myRobots[index].GetComponent<Robot>();
        caster.transform.position = pos;
        StartCoroutine(caster.MyAttack.Fire(caster, dir));
    }

    public void EnemyRobotFire(int index, Vector3 pos, Vector3 dir)
    {
        Robot caster = enemyRobots[index].GetComponent<Robot>();
        caster.transform.position = pos;
        StartCoroutine(caster.MyAttack.Fire(caster, dir));
    }

    // 로봇 생성 신호를 서버로부터 수신하는 함수
    public void OnReceiveRobotSpawnPacket(PacketId id, byte[] data)
    {
        // 서버는, 주기적으로 Packet ID만 붙혀서 송신한다.
        // 로봇 스폰 코드
        StartCoroutine(SpawnRobots());
    }

    public Robot MyRobot(int i)
    {
        return myRobots[i].GetComponent<Robot>();
    }

    public Robot EnemyRobot(int i)
    {
        return enemyRobots[i].GetComponent<Robot>();
    }
}
