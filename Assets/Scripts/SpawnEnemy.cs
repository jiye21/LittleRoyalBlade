using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject enemy;


    private readonly static int enemyMaxCount = 25;
    private GameObject[] enemyPool = new GameObject[enemyMaxCount];
    private int curEnemyIndex = 0;
    private int enemyNum = 3;

    float gravity = 0.5f;
    int flag = 0;

    //yield return ������ ������ ���� ��쿡�� ���ο� �ν��Ͻ��� ��� ����� ������ ���� �������� ����.
    //���� new�� ����ؼ� ���ο� ��ü�� ��� �����ϴ°� ���� ĳ���ؼ� ����ϴ� ���� ȿ�����Դϴ�.
    //public static readonly WaitForSeconds m_waitForSecond2s = new WaitForSeconds(2f);

    float enemyTime = 5f;

    public bool reStart = false;

    void Awake()   // Awake is called even if the script is disabled. 
    {
        //enemyPool Instantiate
        for (int i = 0; i < enemyMaxCount; i++)
        {
            enemyPool[i] = Instantiate(enemy, transform.position, transform.rotation);
            enemyPool[i].gameObject.SetActive(false);
        }
    }

    void OnEnable()
    //OnEnable : ��ũ��Ʈ�� ��Ȱ��ȭ�� �� �ٽ� Ȱ��ȭ�� �� ���� ȣ��

    //SetActive(false)�ô� �ڷ�ƾ ����, enabled false�� �ڷ�ƾ �ȸ���

    //�ڷ�ƾ �̸��� �̿��� ������� ó������ �����, IEnumerator�� ���� ����� �ش� �ڷ�ƾ�� ���� �������� �ٽ� ����. 
    //�Լ� �̸����� �ڷ�ƾ ���۽� ���� �Ұ�
    {
        for (int i = 0; i < enemyMaxCount; i++)
        {
            enemyPool[i].transform.position = transform.position;
            enemyPool[i].transform.rotation = transform.rotation;
            enemyPool[i].gameObject.SetActive(false);
            gravity = 0.5f;
            enemyTime = 5f;
            flag = 0;
            enemyNum = 3;
        }
        StartCoroutine("SpawnEnemys");
    }

    void Start()
        //��ũ��Ʈ�� Ȱ��ȭ�� �� ȣ��Ǹ�
        //�ѹ� ȣ��� ���ķδ� ��ȣ����� �ʴ´�. 
    {
        
    }


    void Update()
    {
        
    }


    public IEnumerator SpawnEnemys()
    {
        for (; ; )
        {
            yield return null;

            if (curEnemyIndex >= enemyMaxCount )
            {
                curEnemyIndex = 0;
            }
            
            for(int i = 0; i < enemyNum; i++) //�ѹ��� enemyNum������ŭ �� ȣ��
            {   
                if (curEnemyIndex + i >= enemyMaxCount) continue;
                if (enemyPool[curEnemyIndex + i].gameObject.activeSelf)
                {                //���� ���� ����ִٸ� �ٽ� �ҷ����� ����
                    curEnemyIndex++;
                    i--;    //i�� ������Ű�� �ʰ� ���� �ε����� �Ѿ�� ����
                    continue;
                }

                enemyPool[curEnemyIndex + i].transform.position = transform.position + 
                    new Vector3(0, i * enemy.transform.lossyScale.y, 0);

                //���� �����ϸ� ������ ������ ��� �ʱ�ȭ
                enemyPool[curEnemyIndex + i].gameObject.SetActive(true);
                enemyPool[curEnemyIndex + i].gameObject.GetComponent<BoxCollider2D>().enabled = true;
                //gravityScale�� PlayerController��ũ��Ʈ���� magnet���� �ٲٹǷ� �̸� �����ص� gravity �������� ������ �����Ѵ�
                enemyPool[curEnemyIndex + i].gameObject.GetComponent<Rigidbody2D>().gravityScale = gravity;
                enemyPool[curEnemyIndex + i].transform.GetChild(0).gameObject.SetActive(true); //�ڽĵ� ���������Ƿ� ����

            }
            curEnemyIndex += enemyNum;

           
            yield return new WaitForSecondsRealtime(enemyTime);

            //�� ���� ���� ���� ª������ ǥ���ϱ� ���� ���� ���� ���������� �߷� ����, �ð� ����, ������ ����
            flag++;
            if (flag % 2 == 0)
            {   //2���� ħ�Ը��� �ѹ��� ���� ħ���ӵ��� ������
                gravity += 0.2f;
                enemyTime -= 0.5f;
            }
            if(flag % 3 == 0) enemyNum++; //3���� �ѹ� ������ ����
            //�ִ�ġ ����
            if (gravity >= 1) gravity = 1;
            if (enemyTime <= 3f) enemyTime = 3f;
            if (enemyNum > 5) enemyNum = 5;
            for (int i = 0; i < enemyMaxCount; i++)
            {
                enemyPool[i].gameObject.GetComponent<Rigidbody2D>().gravityScale = gravity;
            }
            

        }
    }

    
}
