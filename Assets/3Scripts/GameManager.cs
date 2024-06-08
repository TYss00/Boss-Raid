using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance; // 싱글톤 인스턴스 추가

    public GameObject gameCam;
    public GameObject playerPrefab;  // 플레이어 프리팹
    public Boss boss;
    public int Stage;
    public bool isBattle; // 싸우고 있는지 아닌지 판단하는 변수

    public GameObject gamePanel;
    public TextMeshProUGUI HPCostText;
    public TextMeshProUGUI itemHPCostText;
    public TextMeshProUGUI AmmoText;
    public Image weapon1Img;
    public Image weapon2Img;

    public RectTransform PlayerHealthBar;
    public RectTransform bossHealthGroup;
    public RectTransform bossHealthBar;

    private Player player; // 플레이어 인스턴스 참조
    private Cinemachine.CinemachineVirtualCamera virtualCamera; // 시네머신 가상 카메라

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

    void Start()
    {
        virtualCamera = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();

        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Attempting to instantiate player in GameManager...");
            // Player 생성
            GameObject playerObj = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
            player = playerObj.GetComponent<Player>(); // 생성된 Player 인스턴스 참조
            Debug.Log("Player instantiated");
        }
    }

    public void SetPlayer(Player playerInstance)
    {
        player = playerInstance;
        if (player != null && virtualCamera != null)
            {
                virtualCamera.Follow = player.transform;
                virtualCamera.LookAt = player.transform;
            }
    }

    void LateUpdate()
    {
        if (player == null) return; // 플레이어가 null인지 확인

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



