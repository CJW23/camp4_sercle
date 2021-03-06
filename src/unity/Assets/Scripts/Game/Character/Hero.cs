﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Hero : MonoBehaviour
{
    public enum State { Idle, Appear, PreDelay, Skill, Disappear, CoolDown }

    [Header("Basic Info")]
    [SerializeField] private int index; // 0 ~ 2.
    [SerializeField] private State state;

    [Header("Skill Info")]
    [SerializeField] private Skill skill;

    [Header("Animation")]
    [SerializeField] private Animator heroAnim;

    [Header("Miscellaneous Effect")]
    [SerializeField] private GameObject exitEffect;
    [SerializeField] private List<GameObject> appearance;
    public int testSkillNumber;

    // 상태 - 부울 딕셔너리
    private Dictionary<string, bool> stateMap = new Dictionary<string, bool>();

    private void OnEnable()
    {
        //// 등장, 스킬 테스트용
        //heroAnim = GetComponent<Animator>();
        //SetSkillInfo(testSkillNumber);
        //InitialPos = new Vector3(10, 10, 10);

        //UseSkill(Vector3.zero, new Vector3(1.2f, 0, -.8f));
        ////UseSkill(Vector3.zero, null);
    }
    public void Initialize(int skill)
    {
        heroAnim = GetComponent<Animator>();
        InitStateMap();
        SetSkillInfo(skill);
    }

    private void LateUpdate()
    {
        AdjustAnimationSpeed();
    }

    public int Index { set { index = value; } get { return index; } }
    public State GetState { get { return state; } }
    public Skill GetSkill { get { return skill; } }
    
    public Vector3 InitialPos { get; set; }

    public void UseSkill(Vector3 pos, Vector3? dir)
    {
        StartCoroutine(Fire(pos, dir));
    }

    private IEnumerator Fire(Vector3 pos, Vector3? dir)
    {
        if(state != State.Idle)
        {
            Debug.Log("영웅의 상태가 Idle이 아닙니다. 스킬 사용 명령을 무시합니다.");
            yield break;
        }

        #region 등장
        state = State.Appear;
        skill.remainCool = skill.coolDown;

        // 등장하는 애니메이션과 효과
        transform.rotation = Quaternion.Euler(Vector3.zero);
        // 20 02 14 영웅의 위치를 pos + 10로 이동, 20 02 20 영웅이 한발자국 뒤로 물러나서 스킬 씀.
        GetComponent<Transform>().position = pos + new Vector3(0, 10, 0) - 1.5f * (dir.HasValue ? dir.Value.normalized : Vector3.forward);
        if (dir.HasValue)
        {
            if(dir.Value != Vector3.zero)
                transform.LookAt(dir.Value + new Vector3(0, 10, 0) + pos);
            else
                transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
        
        SetAnimStateMap("Emerge");

        // 하늘에서 강하.
        Rigidbody rig = GetComponent<Rigidbody>();
        if (rig)
        {
            rig.AddForce(-this.transform.up * 300f, ForceMode.Impulse);
        }
        SoundManager.instance.PlaySound("HeroEmergence");

        // 착지 후 멋져보이게 n초 대기
        yield return new WaitForSeconds(skill.emergeDelay);
        #endregion

        #region 스킬 사용
        int randomSkillMotion = Random.Range(0, 3);
        SetAnimStateMap("PreDelay_" + randomSkillMotion.ToString());
        yield return new WaitForSeconds(skill.preDelay);

        // 스킬 사용
        SetAnimStateMap("Fire_" + randomSkillMotion.ToString());
        SoundManager.instance.IterateEffectSound(skill.skillNum.ToString(), 1f);
        
        state = State.Skill;

        // 스킬이펙트 발동
        if(skill.skillEffectPrefab is null)
        {
            skill.skillEffectPrefab = Resources.Load<GameObject>("SkillEffect/MagicEffect");
        }
        GameObject go = Instantiate(skill.skillEffectPrefab, pos, skill.skillEffectPrefab.transform.rotation);
        if (dir.HasValue)
        {   if(skill.skillNum != 16 && skill.skillNum != 17 && skill.skillNum != 18)
                go.transform.LookAt(dir.Value + pos);
        }

        yield return new WaitForSeconds(skill.projDelay);

        // 투사체 생성
        for (int i = 0; i < skill.tickCount; i++)
        {
            float activeDelay = i * skill.tickDelay;
            ProjectileInfo info = new ProjectileInfo(GameManager.instance.MyCampNum, dir.HasValue ? dir.Value : transform.position, skill.speed, skill.range, skill.size, activeDelay, skill.targetType, skill.targetNum, skill.skillEffects, skill.skillNum);

            // 아래는 테스트용
            // ProjectileInfo info = new ProjectileInfo(1, dir.HasValue ? dir.Value : transform.position, skill.speed, skill.range, skill.size, activeDelay, skill.targetType, skill.targetNum, skill.skillEffects);
            Projectile projectile = Instantiate(skill.proj, pos + new Vector3(0, 0.7f, 0), Quaternion.identity);
            projectile.Initialize(info);
        }
        #endregion

        #region 퇴장

        state = State.Disappear;
        // 퇴장하는 애니메이션과 효과
        Instantiate(exitEffect, transform.position, Quaternion.Euler(-90, 0,0));
        
        yield return new WaitForSeconds(skill.postDelay);

        // 후딜레이 후 영웅을 초기 위치로 되돌림
        transform.position = InitialPos;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        #endregion

        #region 쿨타임
        state = State.CoolDown;
        
        while (skill.remainCool > 0)
        {
            yield return new WaitForFixedUpdate();
            skill.remainCool -= Time.fixedDeltaTime;
        }

        skill.remainCool = 0;
        #endregion

        state = State.Idle;
    }

    // 상태맵 초기화
    private void InitStateMap()
    {
        stateMap.Add("Idle", false);
        stateMap.Add("Emerge", false);
        stateMap.Add("PreDelay_0", false);
        stateMap.Add("PreDelay_1", false);
        stateMap.Add("PreDelay_2", false);
        stateMap.Add("Fire_0", false);
        stateMap.Add("Fire_1", false);
        stateMap.Add("Fire_2", false);
        //stateMap.Add("PostDelay", false);
        //stateMap.Add("Die", false);
    }

    // 상태맵에서 원하는 상태만 True로 전환
    private void SetAnimStateMap(string stateName)
    {
        // Set True할 상태 먼저 Set.
        stateMap[stateName] = true;
        heroAnim.SetBool(stateName, true);

        // 나머지는 False 처리
        foreach (var key in stateMap.Keys.ToList())
        {
            if (key != stateName)
            {
                stateMap[key] = false;
                heroAnim.SetBool(key, false);
            }
        }
    }

    // 선딜, 후딜 애니메이션 재생 속도를 스킬의 선딜, 후딜 시간에 끝나도록 조정
    private void AdjustAnimationSpeed()
    {
        if (heroAnim.GetCurrentAnimatorStateInfo(0).IsName("PreDelay_0") || heroAnim.GetCurrentAnimatorStateInfo(0).IsName("PreDelay_1") || heroAnim.GetCurrentAnimatorStateInfo(0).IsName("PreDelay_2"))
            heroAnim.SetFloat("PreDelayOffset", 1f / (skill.preDelay / heroAnim.GetCurrentAnimatorClipInfo(0)[0].clip.length));
        else if (heroAnim.GetCurrentAnimatorStateInfo(0).IsName("Fire_0") || heroAnim.GetCurrentAnimatorStateInfo(0).IsName("Fire_1") || heroAnim.GetCurrentAnimatorStateInfo(0).IsName("Fire_2"))
            heroAnim.SetFloat("PostDelayOffset", 1f / (skill.postDelay / heroAnim.GetCurrentAnimatorClipInfo(0)[0].clip.length));
    }

    // SkillManager에 저장되어있는 나의 스킬 번호를 토대로 스킬 정보 설정
    private void SetSkillInfo(int num)
    {

        string jsonFile = Resources.Load<TextAsset>("Json/SkillInfoJson").ToString();

        SkillInfoJsonArray skillArray;
        skillArray = JsonUtility.FromJson<SkillInfoJsonArray>(jsonFile);



        // 스킬 이펙트 설정
        skill.skillEffectPrefab = Resources.Load<GameObject>(skillArray.skillInfo[num].skillEffectPath);

        // 스킬 이미지 설정
        skill.image = Resources.Load<Sprite>(skillArray.skillInfo[num].skillImagePath);

        // 스킬 이름 설정
        skill.skillName = skillArray.skillInfo[num].skillName;

        // 스킬 설명 설정
        skill.description = skillArray.skillInfo[num].skillDesc;

        // 스킬 번호 설정
        skill.skillNum = skillArray.skillInfo[num].skillNumber;

        // 스킬 세부정보
        jsonFile = Resources.Load<TextAsset>("Json/SkillDetailJson").ToString();

        SkillDetailJsonArray skillDetailArray;
        skillDetailArray = JsonUtility.FromJson<SkillDetailJsonArray>(jsonFile);
        //Debug.Log(jsonFile);
        // 세부 설정
        skill.emergeDelay = skillDetailArray.skillInfo[num].emergeDelay;
        skill.preDelay = skillDetailArray.skillInfo[num].preDelay;
        skill.projDelay = skillDetailArray.skillInfo[num].projDelay;
        skill.postDelay = skillDetailArray.skillInfo[num].postDelay;
        skill.coolDown = skillDetailArray.skillInfo[num].coolDown;
        skill.remainCool = 0;

        // 투사체 설정
        skill.speed = skillDetailArray.skillInfo[num].speed;
        skill.range = skillDetailArray.skillInfo[num].range;
        skill.size = skillDetailArray.skillInfo[num].size;
        skill.tickCount = skillDetailArray.skillInfo[num].tickCount;
        skill.tickDelay = skillDetailArray.skillInfo[num].tickDelay;
        skill.targetType = skillDetailArray.skillInfo[num].targetType;
        skill.targetNum = skillDetailArray.skillInfo[num].targetNum;
        skill.skillEffects = skillDetailArray.skillInfo[num].skillEffects.ToList();
    }


    public void SetAppearance(int index)
    {
        appearance[index].SetActive(true);
    }
}
