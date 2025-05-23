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
        // 处理输入
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        // 移动玩家
        rb.velocity = moveDirection * moveSpeed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 碰到糖豆
        if (collision.CompareTag("Pellet"))
        {
            GameController.Instance.AddScore(10);
            collision.gameObject.SetActive(false);
        }

        // 碰到大力丸
        if (collision.CompareTag("PowerPellet"))
        {
            GameController.Instance.AddScore(50);
            GameController.Instance.PlayerGotPowerPellet();
            powerPelletCount++;
            collision.gameObject.SetActive(false);
        }

        // 碰到面具
        if (collision.CompareTag("Mask") && !HasCollectedAnyGhost())
        {
            GameController.Instance.EnterMaskedState();
            collision.gameObject.SetActive(false);
        }

        // 碰到初音
        if (collision.CompareTag("Hatsune") && GameController.Instance.currentState == GameController.GameState.Masked)
        {
            hasHatsune = true;
            collision.gameObject.SetActive(false);
            StartCoroutine(StartHatsunePenalty());
        }

        // 碰到黄瓜
        if (collision.CompareTag("Cucumber"))
        {
            collision.gameObject.SetActive(false);
            // 黄瓜不得分
        }

        // 碰到鬼
        if (collision.CompareTag("Ghost"))
        {
            GhostController ghost = collision.GetComponent<GhostController>();

            if (isPowered)
            {
                // 玩家处于无敌状态，收集或击杀鬼
                GameController.Instance.CollectGhost(ghost);

                if (GameController.Instance.currentState == GameController.GameState.Masked)
                {
                    // 面具状态下击杀鬼
                    killedGhosts.Add(ghost);

                    // 检查是否是木头人且有初音效果
                    if (ghost == GameController.Instance.ghostWooden && hasHatsune)
                    {
                        // 秒杀木头人
                        ghost.Kill();
                        hasHatsune = false; // 消耗初音效果
                    }
                    else
                    {
                        ghost.Kill();
                    }
                }
                else
                {
                    // 正常状态下收集鬼
                    collectedGhosts.Add(ghost);
                    ghost.Collect();
                }
            }
            else
            {
                // 玩家不是无敌状态，受伤
                GameController.Instance.PlayerHit();
            }
        }
    }

    System.Collections.IEnumerator StartHatsunePenalty()
    {
        while (hasHatsune)
        {
            // 每秒扣10分
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