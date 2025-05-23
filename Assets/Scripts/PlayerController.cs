using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject powerEffect;
    public Transform respawnPoint;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private bool isPowered = false;
    private int powerPelletCount = 0;
    private List<GhostController> collectedGhosts = new List<GhostController>();
    private List<GhostController> killedGhosts = new List<GhostController>();
    private bool hasHatsune = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // ��������
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        // �ƶ����
        rb.velocity = moveDirection * moveSpeed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // �����Ƕ�
        if (collision.CompareTag("Pellet"))
        {
            GameController.Instance.AddScore(10);
            collision.gameObject.SetActive(false);
        }

        // ����������
        if (collision.CompareTag("PowerPellet"))
        {
            GameController.Instance.AddScore(50);
            GameController.Instance.PlayerGotPowerPellet();
            powerPelletCount++;
            collision.gameObject.SetActive(false);
        }

        // �������
        if (collision.CompareTag("Mask") && !HasCollectedAnyGhost())
        {
            GameController.Instance.EnterMaskedState();
            collision.gameObject.SetActive(false);
        }

        // ��������
        if (collision.CompareTag("Hatsune") && GameController.Instance.currentState == GameController.GameState.Masked)
        {
            hasHatsune = true;
            collision.gameObject.SetActive(false);
            StartCoroutine(StartHatsunePenalty());
        }

        // �����ƹ�
        if (collision.CompareTag("Cucumber"))
        {
            collision.gameObject.SetActive(false);
            // �ƹϲ��÷�
        }

        // ������
        if (collision.CompareTag("Ghost"))
        {
            GhostController ghost = collision.GetComponent<GhostController>();

            if (isPowered)
            {
                // ��Ҵ����޵�״̬���ռ����ɱ��
                GameController.Instance.CollectGhost(ghost);

                if (GameController.Instance.currentState == GameController.GameState.Masked)
                {
                    // ���״̬�»�ɱ��
                    killedGhosts.Add(ghost);

                    // ����Ƿ���ľͷ�����г���Ч��
                    if (ghost == GameController.Instance.ghostWooden && hasHatsune)
                    {
                        // ��ɱľͷ��
                        ghost.Kill();
                        hasHatsune = false; // ���ĳ���Ч��
                    }
                    else
                    {
                        ghost.Kill();
                    }
                }
                else
                {
                    // ����״̬���ռ���
                    collectedGhosts.Add(ghost);
                    ghost.Collect();
                }
            }
            else
            {
                // ��Ҳ����޵�״̬������
                GameController.Instance.PlayerHit();
            }
        }
    }

    System.Collections.IEnumerator StartHatsunePenalty()
    {
        while (hasHatsune)
        {
            // ÿ���10��
            GameController.Instance.AddScore(-10);
            yield return new WaitForSeconds(1f);
        }
    }

    public void EnterPoweredState()
    {
        isPowered = true;
        powerEffect.SetActive(true);
    }

    public void ExitPoweredState()
    {
        isPowered = false;
        powerEffect.SetActive(false);
    }

    public void ResetPosition()
    {
        transform.position = respawnPoint.position;
        moveDirection = Vector2.zero;
        rb.velocity = Vector2.zero;
    }

    public bool HasCollectedAnyGhost()
    {
        return collectedGhosts.Count > 0;
    }

    public void CollectGhost(GhostController ghost)
    {
        collectedGhosts.Add(ghost);
    }

    public void KillGhost(GhostController ghost)
    {
        killedGhosts.Add(ghost);
    }

    public int GetKilledGhostCount()
    {
        return killedGhosts.Count;
    }

    public int GetPowerPelletCount()
    {
        return powerPelletCount;
    }
}