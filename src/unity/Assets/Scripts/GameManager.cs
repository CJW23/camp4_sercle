﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    [Header("Characters")]
    public List<GameObject> characterPrefabs;
    public List<Character> myCharacters;
    public List<Character> enemyCharacters;

    [Tooltip("초당 CP 증가량")]
    [SerializeField] private float cps;
    [SerializeField] private float myCP;

    [SerializeField] private Character curCharacter;
    public Character CurCharacter { get { return curCharacter; } }

    [Header("Display")]
    [SerializeField] private RectTransform moveCircle;
    
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        bool is1P = GameObject.Find("NetworkManager").GetComponent<NetworkManager>().clientID;

        #region 캐릭터 생성 및 번호/ID 부여
        // 현재는 캐릭터가 3개 밖에 없으므로 자동으로 생성
        // 나중에는 서버에서 받아와야 함
        myCharacters = new List<Character>();
        for (int i = 0; i < 3; i++)
        {
            Character _character = Instantiate(characterPrefabs[i]).GetComponent<Character>();
            _character.name = characterPrefabs[i].name;
            _character.index = i;
            _character.isFriend = true;

            myCharacters.Add(_character); ;
        }

        enemyCharacters = new List<Character>();
        for (int i = 0; i < 3; i++)
        {
            Character _character = Instantiate(characterPrefabs[i]).GetComponent<Character>();
            _character.name = "Enemy_" + characterPrefabs[i].name;
            _character.index = i;
            _character.isFriend = false;

            enemyCharacters.Add(_character);
        }
        #endregion

        #region 캐릭터 배치
        // 회전도 해줘야 할까?
        if (is1P)
        {
            myCharacters[0].transform.position = new Vector3(0, 0, 0);
            myCharacters[1].transform.position = new Vector3(1, 0, -1);
            myCharacters[2].transform.position = new Vector3(-1, 0, -1);

            enemyCharacters[0].transform.position = new Vector3(12, 0, 18);
            enemyCharacters[1].transform.position = new Vector3(13, 0, 19);
            enemyCharacters[2].transform.position = new Vector3(11, 0, 19);
        }
        else
        {
            enemyCharacters[0].transform.position = new Vector3(0, 0, 0);
            enemyCharacters[1].transform.position = new Vector3(1, 0, -1);
            enemyCharacters[2].transform.position = new Vector3(-1, 0, -1);

            myCharacters[0].transform.position = new Vector3(12, 0, 18);
            myCharacters[1].transform.position = new Vector3(13, 0, 19);
            myCharacters[2].transform.position = new Vector3(11, 0, 19);
        }
        #endregion

        #region 시작 세팅
        // 자신의 첫번째 캐릭터로 고정
        ChangeCurrentCharacter(myCharacters[0]);

        myCP = 50;
        #endregion
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeCurrentCharacter(myCharacters[0]);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeCurrentCharacter(myCharacters[1]);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeCurrentCharacter(myCharacters[2]);

        if (!curCharacter) return;

        if (Input.GetKeyDown(KeyCode.Z))
            Upgrade();
        if (Input.GetMouseButtonDown(1))
            ClickToMove();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("First Skill Input");
            curCharacter.UseSkill(1);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("Second Skill Input");
            curCharacter.UseSkill(2);
        }
    }

    private void FixedUpdate()
    {
        // 매초 cps만큼 CP 충전
        myCP += cps * Time.fixedDeltaTime;
    }

    public void ChangeCurrentCharacter(Character character)
    {
        if (curCharacter) curCharacter.ChooseToCurrent(false);
        curCharacter = character;
        Camera.main.GetComponent<CameraController>().FocusCharacter(CurCharacter);
        curCharacter.ChooseToCurrent(true);
    }

    public Vector3? GetDirection(Character caster, ref bool isValid)
    {
        Vector3? dir = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (curCharacter != caster)
        {
            isValid = false;
            return null;
        }

        if (Physics.Raycast(ray, out hit, 100))
        {
            Vector3 casterPos = caster.transform.position;
            Vector3 rawDir = hit.point - casterPos;

            if (Input.GetMouseButtonDown(0))
            {
                // Debug.Log("마우스 위치 : " + hit.point + ", 시전자 위치 : " + casterPos + ", 계산된 방향 : " + rawDir);
                dir = rawDir.normalized;
                //caster.ShowSkillDirection(false);
            }
        }

        return dir;
    }

    /// <summary>
    /// 선택된 캐릭터가 있고 마우스 오른쪽 클릭을 했을 때 해당 캐릭터의 목표지점을 설정
    /// </summary>
    private void ClickToMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100))
        {
            Animation anim = moveCircle.GetComponent<Animation>();
            anim.Stop();
            moveCircle.anchoredPosition = new Vector2(hit.point.x, hit.point.z);
            anim.Play();

            MoveCharacter(curCharacter.index, hit.point);
        }
    }

    /// <summary>
    /// characters[index]의 목표지점을 destination으로 설정해주고
    /// 이 정보를 서버로 전송
    /// </summary>
    /// <param name="index">캐릭터 번호</param>
    /// <param name="destination">목표 지점</param>
    public void MoveCharacter(int index, Vector3 destination)
    {
        myCharacters[index].SetDestination(destination);
        MovingManager.instance.SendLocalMovingInfo(index, destination);
    }

    // 2020 02 01 수신한 원격 캐릭터 이동 패킷을 원격캐릭터에 적용 한다.
    public void MoveEnemyCharacter(int index, Vector3 destination)
    {
        enemyCharacters[index].SetDestination(destination);
    }

    public void FireProjectile(int index, int num, Vector3 dir)
    {
        myCharacters[index].FireProjectile(num, dir);
        SkillManager.instance.SendLocalSkillInfo(index, num, dir);
    }

    // 2020 02 01 원격 캐릭터가 투사체 발사하도록 한다.
    public void FireRemoteProjectile(int index, int num, Vector3 dir)
    {
        enemyCharacters[index].FireProjectile(num, dir);
    }

    /// <summary>
    /// 임시로 만들어둔 캐릭터를 강화시키는 함수. 현재는 Z키를 누르면 CP를 20 소모하고 현재 선택된 캐릭터의 공격력이 20 증가.
    /// </summary>
    private void Upgrade()
    {
        if (myCP < 20) return;

        curCharacter.status.ChangeStat(StatusType.ATK, 20);
        myCP -= 20;
    }

    /// <summary>
    /// 계산된 효과를 target 캐릭터에게 적용하는 함수.
    /// 모든 공격, 효과는 이 함수를 거쳐가도록 할 예정.
    /// </summary>
    /// <param name="target">효과를 적용할 대상</param>
    /// <param name="effects">적용할 효과의 리스트</param>
    public void ApplySkill(Character target, List<EffectResult> effects)
    {
        target.Apply(effects);
    }
}