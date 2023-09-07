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

    //yield return 구문이 여러번 사용될 경우에는 새로운 인스턴스를 계속 만들기 때문에 많은 가비지가 생성.
    //따라서 new를 사용해서 새로운 객체를 계속 생성하는것 보다 캐싱해서 사용하는 것이 효율적입니다.
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
    //OnEnable : 스크립트가 비활성화된 후 다시 활성화될 때 마다 호출

    //SetActive(false)시는 코루틴 종료, enabled false는 코루틴 안멈춤

    //코루틴 이름을 이용한 재시작은 처음부터 재시작, IEnumerator를 통한 방법은 해당 코루틴이 멈춘 지점에서 다시 시작. 
    //함수 이름으로 코루틴 시작시 종료 불가
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
        //스크립트가 활성화될 때 호출되며
        //한번 호출된 이후로는 재호출되지 않는다. 
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
            
            for(int i = 0; i < enemyNum; i++) //한번에 enemyNum마리만큼 적 호출
            {   
                if (curEnemyIndex + i >= enemyMaxCount) continue;
                if (enemyPool[curEnemyIndex + i].gameObject.activeSelf)
                {                //적이 아직 살아있다면 다시 불러내지 않음
                    curEnemyIndex++;
                    i--;    //i를 증가시키지 않고 다음 인덱스로 넘어가기 위함
                    continue;
                }

                enemyPool[curEnemyIndex + i].transform.position = transform.position + 
                    new Vector3(0, i * enemy.transform.lossyScale.y, 0);

                //게임 진행하며 꺼졌던 설정들 모두 초기화
                enemyPool[curEnemyIndex + i].gameObject.SetActive(true);
                enemyPool[curEnemyIndex + i].gameObject.GetComponent<BoxCollider2D>().enabled = true;
                //gravityScale은 PlayerController스크립트에서 magnet사용시 바꾸므로 미리 저장해둔 gravity 변수에서 꺼내와 적용한다
                enemyPool[curEnemyIndex + i].gameObject.GetComponent<Rigidbody2D>().gravityScale = gravity;
                enemyPool[curEnemyIndex + i].transform.GetChild(0).gameObject.SetActive(true); //자식도 꺼져있으므로 켜줌

            }
            curEnemyIndex += enemyNum;

           
            yield return new WaitForSecondsRealtime(enemyTime);

            //적 생성 간격 점점 짧아짐을 표현하기 위해 적이 빨리 내려오도록 중력 증가, 시간 단축, 마리수 증가
            flag++;
            if (flag % 2 == 0)
            {   //2번의 침입마다 한번씩 적의 침투속도가 빨라짐
                gravity += 0.2f;
                enemyTime -= 0.5f;
            }
            if(flag % 3 == 0) enemyNum++; //3번에 한번 마리수 증가
            //최대치 고정
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
