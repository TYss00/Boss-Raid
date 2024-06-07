using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // 싱글톤 인스턴스 추가

    public GameObject gameCam;
    public Player player;
    public Boss boss;
    public int Stage;
    public bool isBattle; // 싸우고 있는지 아닌지 판단하는 변수
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC; // 몹이 있는가

    public GameObject gamePanel;
    public TextMeshProUGUI HPCostText;
    public TextMeshProUGUI itemHPCostText;
    public TextMeshProUGUI AmmoText;
    public Image weapon1Img;
    public Image weapon2Img;

    public RectTransform PlayerHealthBar;
    public RectTransform bossHealthGroup;
    public RectTransform bossHealthBar;

    void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void StageStart()
    {
        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void StageEnd()
    {
        isBattle = false;
    }

    IEnumerator InBattle()
    {
        yield return new WaitForSeconds(5);
        StageEnd();
    }

    void LateUpdate()
    {
        // 플레이어 체력 UI
        PlayerHealthBar.localScale = new Vector3((float)player.health / player.maxhealth, 1, 1);

        // 플레이어 UI
        HPCostText.text = player.health.ToString();
        itemHPCostText.text = "x" + player.currentItems.ToString(); // 아이템 수량 업데이트
        if (player.equipWeapon == null)
        {
            AmmoText.transform.parent.gameObject.SetActive(false);
        }
        else if (player.equipWeapon.type == Weapon.Type.Melee)
        {
            AmmoText.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            AmmoText.transform.parent.gameObject.SetActive(true);
            AmmoText.text = player.equipWeapon.curAmmo + " /" + player.ammo;
        }

        // 무기 UI
        weapon1Img.color = new Color(1, 1, 1, player.hasWeapon[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapon[1] ? 1 : 0);

        if (boss != null && boss.gameObject.activeInHierarchy && boss.curHealth > 0)
        {
            bossHealthGroup.gameObject.SetActive(true);
            // 보스 체력 UI
            bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
        }
        else
        {
            bossHealthGroup.gameObject.SetActive(false);
        }
    }

    public void UpdateItemUI(int currentItems)
    {
        itemHPCostText.text = "x" + currentItems.ToString();
    }
}

