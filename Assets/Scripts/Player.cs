using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapon;
    public Camera followCamera;

    public int ammo;
    public int maxAmmo;
    public int health;
    public int maxhealth;


    float hAxis;
    float vAxis;
    bool WDown;
    bool jDown;
    bool fDown; // 키를 누르고
    bool rDown; // 장전
    bool iDown;


    bool isJump;
    bool isDodge;
    bool isFireReady = true; //딜레이가 준비되면
    bool isBorder;
    bool isReload;
    bool isDamage;

    Vector3 moveVec;
    Vector3 dodgeVec;
    Rigidbody rigid;

    Animator anim;

    MeshRenderer[] meshs;

    GameObject nearObject;
    Weapon equipWeapon;
    float fireDelay;


    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Reload();
        Dodge();
        Interation();
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
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
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
            if (WDown)
            {
                transform.position += moveVec * speed * 0.3f * Time.deltaTime;
            }
            else
            {
                transform.position += moveVec * speed * Time.deltaTime;
            }
        }

        //밑에 if문하고 같음 삼항 연산자
        //transform.position += moveVec * speed * (WDown ? 0.3f : 1f) * Time.deltaTime;
        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", WDown);
    }

    void Turn()
    {
        // 키보드로 회전
        transform.LookAt(transform.position + moveVec);

        // 마우스로 회전
        if (fDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
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
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
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
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;

        if (ammo == 0)
            return;

        if (rDown && !isJump && !isDodge && isFireReady)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 1f); // 장전 시간 세팅은 1초로해둠
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
        }
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
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                if (weaponIndex >= 0 && weaponIndex < weapons.Length)
                {
                    hasWeapon[weaponIndex] = true;
                    weapons[weaponIndex].SetActive(true); // 무기 즉시 활성화
                    equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();


                    Destroy(nearObject);
                }
            }
        }
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
        if(other.tag == "EnemyBullet"){
            if(!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
            health -= enemyBullet.damage;
            if(other.GetComponent<Rigidbody>() != null)
            {
                Destroy(other.gameObject);
            }
            StartCoroutine(OnDamage());
            }
        }
    }

    IEnumerator OnDamage()
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }

        yield return new WaitForSeconds(1f); // 맞고나서 무적시간 1초

        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

        isDamage = false;
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

}
