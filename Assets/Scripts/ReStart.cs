using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReStart : MonoBehaviour
{
    public GameObject player;   //player 프리팹
    public GameObject playerFirst;  //게임 첫시작시 이미 존재하는 player

    public Canvas mainCanvas;
    public Canvas tooltipCanvas;
    public GameObject enemySpawn;
    public Canvas reStartCanvas;

    public bool isDie = false;

    GameObject playerPrefab;
    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (isDie)
        {   //플레이어 사망시 게임오버 캔버스 보여줌
            Camera.main.transform.position = new Vector3(0, 2.305f, -10);
            reStartCanvas.gameObject.SetActive(true);
            if (playerPrefab == null)
            {   //playerFirst가 플레이어였다면 그 플레이어의 coin을 받아옴
                reStartCanvas.GetComponentInChildren<TextMeshProUGUI>().text =
                "Score: " + playerFirst.GetComponent<PlayerController>().coin;
            }
            else
            {
                reStartCanvas.GetComponentInChildren<TextMeshProUGUI>().text =
                "Score: " + playerPrefab.GetComponent<PlayerController>().coin;
            }
            Camera.main.transform.position = new Vector3(0, 2.305f, -10); //mainCamera 위치 초기화
            isDie = false;
        }

    }

    //------Buttton Onclick Function--------
    public void StartAtFirstTime()
    {   //메인 화면에서 진입한 게임시작 - 툴팁화면을 보여줌
        mainCanvas.gameObject.SetActive(false);
        tooltipCanvas.gameObject.SetActive(true);
    }
    public void NextBtn()
    {
        tooltipCanvas.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text =
            "Magnet : 공격 3번 성공시 이용 가능, 주변 적을 Player 근처로 모아준다. 이때 공격 버튼을 누르면 끌려온 적 모두 처치 가능\n\n" +
            "Enemy : 한번에 3마리씩 침입하며 시간이 지날수록 침입 속도와 침입 마리수가 커진다. 최대 침입 마리수는 한번에 5마리";
        tooltipCanvas.transform.GetChild(0).GetChild(3).gameObject.SetActive(false);
        tooltipCanvas.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);

    }
    public void StartFirstGame()
    {   //첫 게임시작
        tooltipCanvas.gameObject.SetActive(false);
        //첫 시작시는 플레이어가 존재하므로 플레이어의 자식 캔버스만 켜주는 것으로 플레이어 초기화
        playerFirst.transform.GetChild(3).gameObject.SetActive(true);
        //적 초기화
        enemySpawn.SetActive(true);
    }

    public void ReStartGame()
    {   //게임오버 후 재시작 - 바로 게임시작
        reStartCanvas.gameObject.SetActive(false);

        //플레이어 초기화
        playerPrefab = Instantiate(player, transform.position, transform.rotation);
        playerPrefab.transform.GetChild(3).gameObject.SetActive(true);

        //적 초기화
        //enemySpawn의 코루틴 재시작을 위해 한번 비활성화 후 활성화
        enemySpawn.SetActive(false);
        enemySpawn.SetActive(true);
        
    }
    //----------------------------------------
}
