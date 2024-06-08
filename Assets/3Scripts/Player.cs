using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;


public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapon;
    //public Camera followCamera;
    private new Camera camera;
    private PhotonView pv;
    private CinemachineVirtualCamera virtualCamera;
    public int ammo;
    public int maxAmmo;
    public int health;
    public int maxhealth;
    public int currentItems;

    float hAxis;
    float vAxis;
    bool WDown;
    bool jDown;
    bool fDown; // 키를 누르고
    bool rDown; // 장전
    bool iDown;
    bool oneDown; // 힐 버튼

    bool isJump;
    bool isDodge;
    bool isFireReady = true; // 딜레이가 준비되면
    bool isBorder;
    bool isReload;
    bool isDamage;

    Vector3 moveVec;
    Vector3 dodgeVec;
    Rigidbody rigid;

    Animator anim;

    MeshRenderer[] meshs;

    GameObject nearObject;
    public Weapon equipWeapon;
    float fireDelay;

    private Vector3 receivePos;
    private Quaternion receiveRot;
    public float damping = 10.0f;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();
    }

    void Start()
    {
        camera = Camera.main;

        pv = GetComponent<PhotonView>();
        virtualCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();

        if (pv.IsMine)
        {
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
            // GameManager에 자신을 등록
            GameManager.instance.SetPlayer(this);
        }
    }

    void Update()
    {
        if (pv.IsMine)
        {
            GetInput();
            Move();
            Turn();
            Jump();
            Attack();
            Reload();
            Dodge();
            Interation();
            UseHealItem();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position,
                                                receivePos,
                                                Time.deltaTime * damping);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                    receiveRot,
                                                    Time.deltaTime * damping);
        }
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        WDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        rDown = Input.GetButtonDown("Reload");
        fDown = Input.GetButton("Fire1");
        iDown = Input.GetButtonDown("Interation");
        oneDown = Input.GetKeyDown(KeyCode.Alpha1);
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
        {
            moveVec = dodgeVec;
        }

        if (!isFireReady || isReload)
        {
            moveVec = Vector3.zero;
        }

        if (!isBorder)
        {
            transform.position += moveVec * speed * (WDown ? 0.3f : 1f) * Time.deltaTime;
        }

        bool isRunning = moveVec != Vector3.zero;
        bool isWalking = WDown;

        if (anim.GetBool("isRun") != isRunning)
        {
            pv.RPC("SetRunAnimation", RpcTarget.All, isRunning);
        }

        if (anim.GetBool("isWalk") != isWalking)
        {
            pv.RPC("SetWalkAnimation", RpcTarget.All, isWalking);
        }
    }

    [PunRPC]
    void SetRunAnimation(bool isRunning)
    {
        anim.SetBool("isRun", isRunning);
    }

    [PunRPC]
    void SetWalkAnimation(bool isWalking)
    {
        anim.SetBool("isWalk", isWalking);
    }


    void Turn()
    {
        // 키보드로 회전
        transform.LookAt(transform.position + moveVec);

        // 마우스로 회전
        if (fDown)
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
            pv.RPC("TriggerJump", RpcTarget.All);
            isJump = true;
        }
    }

    [PunRPC]
    void TriggerJump()
    {
        anim.SetBool("isJump", true);
        anim.SetTrigger("doJump");
    }


    void Attack()
    {
        if (equipWeapon == null)
        {
            return;
        }

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge)
        {
            if (equipWeapon.type == Weapon.Type.Melee)
            {
                pv.RPC("SwingWeapon", RpcTarget.All);
            }
            else if (equipWeapon.type == Weapon.Type.Range && equipWeapon.curAmmo > 0)
            {
                pv.RPC("ShootWeapon", RpcTarget.All);
            }
            fireDelay = 0;
        }
    }

    [PunRPC]
    void SwingWeapon()
    {
        equipWeapon.Use();
        anim.SetTrigger("doSwing");
    }

    [PunRPC]
    void ShootWeapon()
    {
        equipWeapon.Use();
        anim.SetTrigger("doShot");
    }


    void Reload()
    {
        if (equipWeapon == null || equipWeapon.type == Weapon.Type.Melee || ammo == 0)
        {
            return;
        }

        if (rDown && !isJump && !isDodge && isFireReady)
        {
            pv.RPC("TriggerReload", RpcTarget.All);
        }
    }

    [PunRPC]
    void TriggerReload()
    {
        anim.SetTrigger("doReload");
        isReload = true;
        Invoke("ReloadOut", 1f); // 장전 시간 세팅은 1초로 해둠
    }

    void ReloadOut()
    {
        int reAmmo = Mathf.Min(ammo, equipWeapon.maxAmmo);
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }


    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            dodgeVec = moveVec;
            pv.RPC("TriggerDodge", RpcTarget.All);
        }
    }

    [PunRPC]
    void TriggerDodge()
    {
        speed *= 2;
        anim.SetTrigger("doDodge");
        isDodge = true;
        StartCoroutine(DodgeRoutine());
    }

    IEnumerator DodgeRoutine()
    {
        isDamage = true; // 구르기 중 무적
        yield return new WaitForSeconds(0.5f); // 구르기 시간
        speed *= 0.5f;
        isDodge = false;
        isDamage = false; // 구르기 끝나면 무적 해제
    }


    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Interation()
    {
        if (iDown && nearObject != null && !isJump && !isDodge)
        {
            if (nearObject.tag == "Weapon" && equipWeapon == null)
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                if (weaponIndex >= 0 && weaponIndex < weapons.Length)
                {
                    pv.RPC("PickupWeapon", RpcTarget.All, weaponIndex);
                    Destroy(nearObject);
                }
            }
        }
    }

    [PunRPC]
    void PickupWeapon(int weaponIndex)
    {
        hasWeapon[weaponIndex] = true;
        weapons[weaponIndex].SetActive(true); // 무기 즉시 활성화
        equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
    }


    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAtk));
            }

            if (other.GetComponent<Rigidbody>() != null)
            {
                Destroy(other.gameObject);
            }
        }
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }

        if (isBossAtk)
        {
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(1f); // 맞고 나서 무적 시간 1초

        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

        if (isBossAtk)
        {
            rigid.velocity = Vector3.zero;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }

    void UseHealItem()
    {
        if (oneDown && currentItems > 0 && health < maxhealth)
        {
            UseItem();
            if (GameManager.instance != null)
            {
                GameManager.instance.UpdateItemUI(currentItems); // 아이템 사용 후 UI 업데이트
            }
        }
    }


    void UseItem()
    {
        if (currentItems > 0)
        {
            currentItems--;
            Heal(20); //아이템 사용 시 20만큼 체력 회복
        }
    }

    void Heal(int amount)
    {
        health += amount;
        if (health > maxhealth)
        {
            health = maxhealth;
        }
    }

    // PhotonView 상태 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 로컬캐릭터 전송
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            // 로컬 플레이어 상태 전송
            stream.SendNext(health);
            stream.SendNext(currentItems);
            stream.SendNext(ammo);
        }
        else
        {
            //로컬캐릭터
            receivePos = (Vector3)stream.ReceiveNext();
            receiveRot = (Quaternion)stream.ReceiveNext();
            // 원격 플레이어 상태 수신
            health = (int)stream.ReceiveNext();
            currentItems = (int)stream.ReceiveNext();
            ammo = (int)stream.ReceiveNext();
        }
    }
}

