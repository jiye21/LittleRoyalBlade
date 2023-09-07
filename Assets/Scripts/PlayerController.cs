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
	bool doAttack = false;	//������ ���� ����
	bool isAttack = false;  //���� �ִϸ��̼� ���� ����
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
		//mainCamera�� �׻� �÷��̾� position.y���� y������ 3.685f ���ʿ� ��ġ�ϸ� �÷��̾ ����ٴ�
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

	//-------��ư Onclick �Լ�----------

	public void JumpBtn()
    {
        if (isGrounded)
        {
			isJumping = true;
        }
    }
	public void ShieldBtn()
    {
		//��Ÿ�� �߿��� Shield �Ұ�
		if (isShieldCool) return;

		isShield = true;
	}
	public void AttackBtn()
    {
		isAttack = true;
		doAttack = true;
	}
	
	public void Magnet()
	{   //magnet�� 3�̾ƴϸ� ���۾���, ���� �ѹ� �����ø��� 1����
		if (isMagnet < 3) return;
		StartCoroutine(MagnetEnemy());
	}
	//-----------------------------

	void Jump()
	{
		//isJumping�϶� �ѹ��� �����ϱ� ����
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
		{	//�����ϴ� ���� ������ attack����, ������ ������ ��������(�ڷ�ƾ ����)
			doAttack = false;
		}


		if (!isAttack)  //���� ���۰� �ڷ�ƾ�� �ѹ��� �����ϱ� ����
			return;

		StartCoroutine(AttackEnemy());
		anim.SetTrigger("doAttack");
		isAttack = false;
	}

	void Shield()
    {
		if (!isShield)
			return;

		//ī�޶� �ȿ� �ִ� ��� ���� collisionArray�� ����
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


		//��Ÿ���� �ΰ� Shield�� ���
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
		if (collision.gameObject.tag == "Floor")  //�������� ���� ���� isGrounded���� �˻�
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
            {	//�÷��̾ ���� ���°� �ƴϸ� �ڷ�ƾ ����
				yield break;
            }

			collisionArray = Physics2D.OverlapBoxAll(transform.position, new Vector2(1.85f, 1.85f), 0);
			foreach (var col in collisionArray)
			{	//�÷��̾� ��ó�� ���ӿ�����Ʈ�� Enemy�̰� �÷��̾ ���� �����̸� �� ����
				if (col.gameObject.tag == "Enemy")
				{
					coin += 50f; //50���� ȹ��
					col.gameObject.GetComponentInChildren<ParticleSystem>().Play();
					col.transform.GetChild(0).gameObject.SetActive(false);

					//���� ��������� �θ� collider�� �÷��̾�� �ε����Ƿ� ��� ����
					col.gameObject.GetComponent<BoxCollider2D>().enabled = false;
					isMagnet++;	//���� �������� magnet1����

					while (true)
                    {
						if (col.gameObject.GetComponentInChildren<ParticleSystem>().isStopped)
						{	//�θ� �̸� ���� ��ƼŬ�� �Ⱥ����� ��ƼŬ ��� ������ ����
							col.gameObject.SetActive(false);
							yield break;	//���� �Ϸ�, �ڷ�ƾ ����
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
		//�ٴ����� Ray�� ��� �÷��̾ �ٴ� ��ó�� ���� �� ������ �ǰݴ��ϸ�(=���� �ʹ� ���Ƽ�) hp 1 ����
		RaycastHit2D rayHit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, LayerMask.GetMask("Floor"));
		if (rayHit.collider != null)
		{

			isHit = true; //�ǰ������� �ڷ�ƾ�� �ѹ��� �����ϱ� ����
			hp--;
			if (hp == 0)
			{	//�����
				transform.GetChild(0).gameObject.SetActive(false);//��� ĳ���͸� �����
				psDetroy.Play();

				gm.GetComponent<ReStart>().isDie = true;	//�������� Canvas ����

				Destroy(gameObject, 0.25f);	//��ƼŬ�� �����ֱ� ���� ���� ����
			}

			StartCoroutine(HitAnimation());	//�ǰ� �� ���� �ִϸ��̼� ����

			yield return new WaitForSecondsRealtime(3f); //�ǰ� �� 3�ʵ����� ����	
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
		//���� ����, ���� �ʱ�ȭ
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
    {	//�����ÿ��� �÷��̾��� ��ġ�� �ٲ�Ƿ� Update���� magnet�� ��ġ ���
		//magnet������ ������ �簢���� �÷��̾� �ڽ����� ��ġ�� magnet������ �÷��̾�� �׻� ���� �̵�
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
		//ī�޶� �亸�� 1.5�� ū ������ �˻��� ���� ���� ���� �迭�� ��� ��´�
		Vector2 startViewPos = Camera.main.ViewportToWorldPoint(Vector2.zero);
		Vector2 endViewPos = Camera.main.ViewportToWorldPoint(new Vector2(1f, 1f));
		Collider2D[] colArray = Physics2D.OverlapAreaAll(startViewPos * 1.5f, endViewPos * 1.5f);

		foreach (var col in colArray)
		{
			if (col.gameObject.tag == "Enemy")
			{	//�̸� �����ص� �簢�� ���� ���� ������ ��ǥ�� ���� ��ġ�Ѵ�
				float posX = Random.Range(-magnetPosX, magnetPosX);
				float posY = Random.Range(magnetPosYBottom, magnetPosYTop);
				col.transform.position = new Vector2(posX, posY);
				//���� �з����� �ʰ� ���ִ°��� ǥ���ϱ� ���� collider ��� ����
				col.gameObject.GetComponent<BoxCollider2D>().enabled = false;
				//���߿� ��� ���ְ� �ϱ����� �߷� ��� ����
				col.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
			}
		}

		while (true)
		{
			if (doAttack)
			{   //����Ű�� ������ ��Ƶ� �� ����
				foreach (var col in colArray)
				{
					if (col.gameObject.tag == "Enemy")
                    {
						coin += 50f; //50���� ȹ��
						col.gameObject.GetComponentInChildren<ParticleSystem>().Play();
						col.transform.GetChild(0).gameObject.SetActive(false);
						//�θ� �ٷβ��� ��ƼŬ�� �Ⱥ����� ��� ��ٸ� �� ����
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
