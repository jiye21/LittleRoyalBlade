using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
	float jumpPower = 12f;

	Rigidbody2D rigid;

	bool isJumping = false;
	bool isGrounded = true;
	bool doAttack = false;	//실질적 공격 변수
	bool isAttack = false;  //공격 애니메이션 수행 변수
	bool isShield = false;
	bool isHit = false;

	Animator anim;

	Collider2D[] collisionArray;

	ParticleSystem psShield;
	ParticleSystem psDetroy;

	Canvas btnCanvas;
	GameObject gm;

	private bool isShieldCool = false;
	private int hp = 3;
	public float coin = 0f;
	int isMagnet = 0;

	private float lerpTime = 0;

	public float magnetPosX;
	public float magnetPosYTop;
	public float magnetPosYBottom;

	void Start()
	{
		rigid = GetComponent<Rigidbody2D>();
		anim = GetComponentInChildren<Animator>();
		psShield = transform.GetChild(1).GetComponent<ParticleSystem>();
		psDetroy = transform.GetChild(2).GetComponent<ParticleSystem>();
		btnCanvas = transform.GetChild(3).GetComponent<Canvas>();
		gm = GameObject.Find("GameManager");

	}

	//Graphic & Input Updates	
	void Update()
	{
		//mainCamera는 항상 플레이어 position.y보다 y축으로 3.685f 위쪽에 위치하며 플레이어를 따라다님
		Camera.main.transform.position = new Vector3 (0, transform.position.y + 3.685f, -10);


		HpCheckBar();
		CoinCheck();
		CalculateMagnetPos();
		MagnetCheckBar();
	}

	//Physics engine Updates
	void FixedUpdate()
	{
		Jump();
		Attack();
		Shield();
	}

	//-------버튼 Onclick 함수----------

	public void JumpBtn()
    {
        if (isGrounded)
        {
			isJumping = true;
        }
    }
	public void ShieldBtn()
    {
		//쿨타임 중에는 Shield 불가
		if (isShieldCool) return;

		isShield = true;
	}
	public void AttackBtn()
    {
		isAttack = true;
		doAttack = true;
	}
	
	public void Magnet()
	{   //magnet이 3이아니면 동작안함, 공격 한번 성공시마다 1적립
		if (isMagnet < 3) return;
		StartCoroutine(MagnetEnemy());
	}
	//-----------------------------

	void Jump()
	{
		//isJumping일때 한번만 실행하기 위함
		if (!isJumping)
			return;

		//Prevent Velocity amplification.
		//rigid.velocity = Vector2.zero;

		Vector2 jumpVelocity = new Vector2(0, jumpPower);
		rigid.AddForce(jumpVelocity, ForceMode2D.Impulse);

		isJumping = false;
		isGrounded = false;
	}

	void Attack()
	{
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
			anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
		{	//공격하는 동작 동안은 attack판정, 동작이 끝나야 공격종료(코루틴 종료)
			doAttack = false;
		}


		if (!isAttack)  //공격 동작과 코루틴을 한번만 수행하기 위함
			return;

		StartCoroutine(AttackEnemy());
		anim.SetTrigger("doAttack");
		isAttack = false;
	}

	void Shield()
    {
		if (!isShield)
			return;

		//카메라 안에 있는 모든 적을 collisionArray에 담음
		Vector2 startViewPos = Camera.main.ViewportToWorldPoint(Vector2.zero);
		Vector2 endViewPos = Camera.main.ViewportToWorldPoint(new Vector2(1f,1f));
		Collider2D[] colArray = Physics2D.OverlapAreaAll(startViewPos, endViewPos);
		foreach(var col in colArray)
        {
			if (col.gameObject.tag == "Enemy")
            {
				col.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 8f), ForceMode2D.Impulse);
			}
		}

		psShield.Play();


		//쿨타임을 두고 Shield를 사용
		StartCoroutine(ShieldCool());
		isShield = false;
	}

	
	IEnumerator ShieldCool()
	{
		isShieldCool = true;
        while (true)
        {
			lerpTime += Time.deltaTime;
			btnCanvas.transform.GetChild(1).GetComponent<Image>().fillAmount = Mathf.Lerp(0, 1, lerpTime * 0.5f);
			yield return null;
			if (btnCanvas.transform.GetChild(1).GetComponent<Image>().fillAmount >= 0.9f)
			{
				btnCanvas.transform.GetChild(1).GetComponent<Image>().fillAmount = 1;
				lerpTime = 0;
				isShieldCool = false;
				yield break;
			}
		}

	}


	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Floor")  //더블점프 방지 위해 isGrounded인지 검사
		{
			isGrounded = true;
		}
		if(collision.gameObject.tag == "Enemy")
        {
			if (!isHit) StartCoroutine(Hit());
		}

	}

    private void OnCollisionStay2D(Collision2D collision)
    {
		if (collision.gameObject.tag == "Enemy")
		{
			if(!isHit) StartCoroutine(Hit());
		}
	}

    IEnumerator AttackEnemy()
    {
        while (true)
        {
            if (!doAttack)
            {	//플레이어가 공격 상태가 아니면 코루틴 종료
				yield break;
            }

			collisionArray = Physics2D.OverlapBoxAll(transform.position, new Vector2(1.85f, 1.85f), 0);
			foreach (var col in collisionArray)
			{	//플레이어 근처의 게임오브젝트가 Enemy이고 플레이어가 공격 상태이면 적 공격
				if (col.gameObject.tag == "Enemy")
				{
					coin += 50f; //50코인 획득
					col.gameObject.GetComponentInChildren<ParticleSystem>().Play();
					col.transform.GetChild(0).gameObject.SetActive(false);

					//적이 사라졌지만 부모 collider에 플레이어와 부딪히므로 잠시 꺼줌
					col.gameObject.GetComponent<BoxCollider2D>().enabled = false;
					isMagnet++;	//공격 성공으로 magnet1적립

					while (true)
                    {
						if (col.gameObject.GetComponentInChildren<ParticleSystem>().isStopped)
						{	//부모를 미리 끄면 파티클이 안보여서 파티클 재생 종료후 꺼줌
							col.gameObject.SetActive(false);
							yield break;	//공격 완료, 코루틴 종료
						}
						yield return null;
					}
                    
				}
				yield return null;
			}
			yield return null;
		}
    }

	IEnumerator Hit()
	{
		//바닥으로 Ray를 쏘아 플레이어가 바닥 근처에 있을 때 적에게 피격당하면(=적이 너무 많아서) hp 1 감소
		RaycastHit2D rayHit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, LayerMask.GetMask("Floor"));
		if (rayHit.collider != null)
		{

			isHit = true; //피격판정시 코루틴을 한번만 실행하기 위함
			hp--;
			if (hp == 0)
			{	//사망시
				transform.GetChild(0).gameObject.SetActive(false);//즉시 캐릭터만 사라짐
				psDetroy.Play();

				gm.GetComponent<ReStart>().isDie = true;	//게임종료 Canvas 켜줌

				Destroy(gameObject, 0.25f);	//파티클을 보여주기 위해 지연 삭제
			}

			StartCoroutine(HitAnimation());	//피격 후 무적 애니메이션 동작

			yield return new WaitForSecondsRealtime(3f); //피격 후 3초동안은 무적	
			isHit = false;
		}	
		
		yield break;
    }

	IEnumerator HitAnimation()
    {
		int countTime = 0;
		while (countTime < 14)
		{
			if (countTime % 2 == 0)
				transform.GetChild(0).GetComponent<SpriteRenderer>().color =
					new Color32(255, 255, 255, 90);
			else
				transform.GetChild(0).GetComponent<SpriteRenderer>().color =
					new Color32(255, 255, 255, 180);

			yield return new WaitForSeconds(0.2f);
			countTime++;
		}
		//무적 종료, 투명도 초기화
		transform.GetChild(0).GetComponent<SpriteRenderer>().color =
					new Color32(255, 255, 255, 255);
		yield break;
	}

	void HpCheckBar()
    {
		btnCanvas.transform.GetChild(3).GetComponent<Image>().fillAmount = hp / 3f;
    }

	void CoinCheck()
    {
		btnCanvas.GetComponentInChildren<TextMeshProUGUI>().text = coin.ToString();
	}

	void CalculateMagnetPos()
    {	//점프시에는 플레이어의 위치가 바뀌므로 Update에서 magnet할 위치 계산
		//magnet범위의 기준인 사각형을 플레이어 자식으로 배치해 magnet범위는 플레이어와 항상 같이 이동
		magnetPosX = transform.GetChild(4).position.x +
			transform.GetChild(4).GetComponent<SpriteRenderer>().bounds.extents.x;
		magnetPosYTop = transform.GetChild(4).position.y +
			transform.GetChild(4).GetComponent<SpriteRenderer>().bounds.extents.y;
		magnetPosYBottom = transform.GetChild(4).position.y -
			transform.GetChild(4).GetComponent<SpriteRenderer>().bounds.extents.y;
	}
	IEnumerator MagnetEnemy()
    {
		isMagnet = 0;
		//카메라 뷰보다 1.5배 큰 영역을 검사해 영역 안의 적을 배열에 모두 담는다
		Vector2 startViewPos = Camera.main.ViewportToWorldPoint(Vector2.zero);
		Vector2 endViewPos = Camera.main.ViewportToWorldPoint(new Vector2(1f, 1f));
		Collider2D[] colArray = Physics2D.OverlapAreaAll(startViewPos * 1.5f, endViewPos * 1.5f);

		foreach (var col in colArray)
		{
			if (col.gameObject.tag == "Enemy")
			{	//미리 지정해둔 사각형 영역 안의 랜덤한 좌표로 적을 배치한다
				float posX = Random.Range(-magnetPosX, magnetPosX);
				float posY = Random.Range(magnetPosYBottom, magnetPosYTop);
				col.transform.position = new Vector2(posX, posY);
				//서로 밀려나지 않고 모여있는것을 표현하기 위해 collider 잠시 꺼줌
				col.gameObject.GetComponent<BoxCollider2D>().enabled = false;
				//공중에 잠시 떠있게 하기위해 중력 잠시 꺼줌
				col.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
			}
		}

		while (true)
		{
			if (doAttack)
			{   //공격키를 누르면 모아둔 적 공격
				foreach (var col in colArray)
				{
					if (col.gameObject.tag == "Enemy")
                    {
						coin += 50f; //50코인 획득
						col.gameObject.GetComponentInChildren<ParticleSystem>().Play();
						col.transform.GetChild(0).gameObject.SetActive(false);
						//부모를 바로끄면 파티클이 안보여서 잠시 기다린 후 꺼줌
						yield return new WaitForSecondsRealtime(0.1f);	
						col.gameObject.SetActive(false);
					}
				}
				yield break;
			}
			yield return null;
		}
	}

	void MagnetCheckBar()
    {
		btnCanvas.transform.GetChild(7).GetComponent<Image>().fillAmount = isMagnet / 3f;
	}

}
