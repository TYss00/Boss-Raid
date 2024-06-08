using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missile;
    public Transform missilePoarA;
    public Transform missilePoarB;

    Vector3 lookVec;
    Vector3 tauntVec;
    public bool isLook;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }

        nav.isStopped = true;
    }

    void Start()
    {
        if (target != null)
        {
            StartCoroutine(FindPlayer());
        }
    }


    void Update()
    {
        if (isDead)
        {
            StopAllCoroutines();
            return;
        }

        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookVec);
        }
        else
        {
            nav.SetDestination(tauntVec);
        }
    }

    IEnumerator FindPlayer()
    {
        while (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                StartCoroutine(Think());
            }
            yield return new WaitForSeconds(0.5f);
        }
    }


    IEnumerator Think() // 보스패턴 구현(확률업을 위해 case를 두개씩 붙여줌 자주나오도록 점프는 case가 하나니까 적게나옴)
    {
        yield return new WaitForSeconds(0.1f);

        int ranAction = Random.Range(0, 5);
        switch (ranAction)
        {
            case 0:
            case 1: //미사일 발사패턴
                StartCoroutine(MissileShot());
                break;
            case 2:
            case 3: // 돌 굴러가는 패턴
                StartCoroutine(RockShot());
                break;
            case 4: // 점프 공격 패턴
                StartCoroutine(Taunt());
                break;
        }
    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f); // 미사일A 소환 지연시간
        GameObject instantMissileA = Instantiate(missile, missilePoarA.position, missilePoarA.rotation); // 미사일A 생성
        BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        bossMissileA.target = target;

        yield return new WaitForSeconds(0.3f); // 미사일B 소환 지연시간
        GameObject instantMissileB = Instantiate(missile, missilePoarB.position, missilePoarB.rotation); // 미사일B 생성
        BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        bossMissileB.target = target;

        yield return new WaitForSeconds(2f);

        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position, transform.rotation); // 돌 소환 돌만 소환하면 알아서 굴러감
        yield return new WaitForSeconds(3f);

        isLook = true;
        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        if (target == null) yield break; // target이 null인지 확인
        tauntVec = target.position + lookVec;

        isLook = false;
        nav.isStopped = false;
        if (boxCollider != null) // boxCollider가 유효한지 확인
        {
            boxCollider.enabled = false;
        }
        anim.SetTrigger("doTaunt");

        yield return new WaitForSeconds(1.5f);
        if (meleeArea != null) // meleeArea가 유효한지 확인
        {
            meleeArea.enabled = true;
        }

        yield return new WaitForSeconds(0.5f);
        if (meleeArea != null) // meleeArea가 유효한지 확인
        {
            meleeArea.enabled = false;
        }

        yield return new WaitForSeconds(1f);  // 애니메이션 총 3초이므로 1.5 + 0.5 + 1 = 3초를 써야한다.
        isLook = true;
        nav.isStopped = true;
        if (boxCollider != null) // boxCollider가 유효한지 확인
        {
            boxCollider.enabled = true;
        }
        StartCoroutine(Think());
    }

}
