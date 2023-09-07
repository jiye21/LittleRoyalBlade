using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReStart : MonoBehaviour
{
    public GameObject player;   //player ������
    public GameObject playerFirst;  //���� ù���۽� �̹� �����ϴ� player

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
        {   //�÷��̾� ����� ���ӿ��� ĵ���� ������
            Camera.main.transform.position = new Vector3(0, 2.305f, -10);
            reStartCanvas.gameObject.SetActive(true);
            if (playerPrefab == null)
            {   //playerFirst�� �÷��̾�ٸ� �� �÷��̾��� coin�� �޾ƿ�
                reStartCanvas.GetComponentInChildren<TextMeshProUGUI>().text =
                "Score: " + playerFirst.GetComponent<PlayerController>().coin;
            }
            else
            {
                reStartCanvas.GetComponentInChildren<TextMeshProUGUI>().text =
                "Score: " + playerPrefab.GetComponent<PlayerController>().coin;
            }
            Camera.main.transform.position = new Vector3(0, 2.305f, -10); //mainCamera ��ġ �ʱ�ȭ
            isDie = false;
        }

    }

    //------Buttton Onclick Function--------
    public void StartAtFirstTime()
    {   //���� ȭ�鿡�� ������ ���ӽ��� - ����ȭ���� ������
        mainCanvas.gameObject.SetActive(false);
        tooltipCanvas.gameObject.SetActive(true);
    }
    public void NextBtn()
    {
        tooltipCanvas.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text =
            "Magnet : ���� 3�� ������ �̿� ����, �ֺ� ���� Player ��ó�� ����ش�. �̶� ���� ��ư�� ������ ������ �� ��� óġ ����\n\n" +
            "Enemy : �ѹ��� 3������ ħ���ϸ� �ð��� �������� ħ�� �ӵ��� ħ�� �������� Ŀ����. �ִ� ħ�� �������� �ѹ��� 5����";
        tooltipCanvas.transform.GetChild(0).GetChild(3).gameObject.SetActive(false);
        tooltipCanvas.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);

    }
    public void StartFirstGame()
    {   //ù ���ӽ���
        tooltipCanvas.gameObject.SetActive(false);
        //ù ���۽ô� �÷��̾ �����ϹǷ� �÷��̾��� �ڽ� ĵ������ ���ִ� ������ �÷��̾� �ʱ�ȭ
        playerFirst.transform.GetChild(3).gameObject.SetActive(true);
        //�� �ʱ�ȭ
        enemySpawn.SetActive(true);
    }

    public void ReStartGame()
    {   //���ӿ��� �� ����� - �ٷ� ���ӽ���
        reStartCanvas.gameObject.SetActive(false);

        //�÷��̾� �ʱ�ȭ
        playerPrefab = Instantiate(player, transform.position, transform.rotation);
        playerPrefab.transform.GetChild(3).gameObject.SetActive(true);

        //�� �ʱ�ȭ
        //enemySpawn�� �ڷ�ƾ ������� ���� �ѹ� ��Ȱ��ȭ �� Ȱ��ȭ
        enemySpawn.SetActive(false);
        enemySpawn.SetActive(true);
        
    }
    //----------------------------------------
}
