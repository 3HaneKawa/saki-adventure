using UnityEngine;
using System.Collections;
using System;

public class GhostController : MonoBehaviour
{
    public enum GhostType { Light, Riki, Wooden, Vegetarian }
    public GhostType ghostType;

    public float normalSpeed = 3f;
    public float poweredSpeed = 2f;
    public float crazySpeed = 5f; // 立希疯狂状态速度
    public float woodenInitialSpeed = 1.5f; // 木头人初始速度

    public Transform respawnPoint;
    public GameObject collectEffect;
    public GameObject killEffect;
    public GameObject powerPelletIndicator;

    private Rigidbody2D rb;
    private Transform target;
    private bool isCollected = false;
    private bool isKilled = false;
    private bool hasPowerPellet = false;
    private bool isMotris = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.FindGameObjectWithTag("Player").transform;

        // 初始化速度
        if (ghostType == GhostType.Wooden)
        {
            normalSpeed = woodenInitialSpeed;
        }

        // 灯鬼初始沉眠
        if (ghostType == GhostType.Light && GameController.Instance.currentState == GameController.GameState.Normal)
        {
            StartCoroutine(StartChaseAfterDelay());
        }
    }

    void Update()
    {
        if (isCollected || isKilled)
            return;

        // 移动逻辑
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;

            // 根据不同状态和类型调整速度
            float currentSpeed = normalSpeed;

            // 立希在面具状态且灯鬼被击杀后进入疯狂状态
            if (ghostType == GhostType.Riki &&
                GameController.Instance.currentState == GameController.GameState.Masked &&
                GameController.Instance.ghostLight != null &&
                GameController.Instance.ghostLight.isKilled)
            {
                currentSpeed = crazySpeed;
            }

            // 木头人在面具状态下吃到大力丸变为Motris
            if (ghostType == GhostType.Wooden && isMotris)
            {
                currentSpeed = normalSpeed * 2; // 变为正常速度的两倍
            }

            rb.velocity = direction * currentSpeed;
        }
    }

    IEnumerator StartChaseAfterDelay()
    {
        // 初始沉眠
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(2f);

        // 开始追逐
        normalSpeed = 3f; // 恢复正常速度
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 木头人碰到糖豆有概率将其变为黄瓜
        if (ghostType == GhostType.Wooden &&
            !isMotris &&
            collision.CompareTag("Pellet") &&
            UnityEngine.Random.value < 0.3f) // 30%概率
        {
            GameController.Instance.ConvertPelletToCucumber(collision.gameObject);
        }

        // 鬼吃到大力丸
        if (collision.CompareTag("PowerPellet") && hasPowerPellet)
        {
            // 木头人吃到大力丸变为Motris
            if (ghostType == GhostType.Wooden && !isMotris && GameController.Instance.currentState == GameController.GameState.Masked)
            {
                isMotris = true;
                GameController.Instance.PlayerHit(); // 立即扣血
            }

            hasPowerPellet = false;
            powerPelletIndicator.SetActive(false);
        }
    }

    public void Collect()
    {
        isCollected = true;
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);

        // 显示收集特效
        if (collectEffect != null)
        {
            GameObject effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    public void Kill()
    {
        isKilled = true;
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);

        // 显示击杀特效
        if (killEffect != null)
        {
            GameObject effect = Instantiate(killEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    public void EnablePowerPellet()
    {
        if (GameController.Instance.currentState == GameController.GameState.Masked)
        {
            hasPowerPellet = true;
            powerPelletIndicator.SetActive(true);
        }
    }

    public void DisablePowerPellet()
    {
        hasPowerPellet = false;
        powerPelletIndicator.SetActive(false);
    }

    public void Reset()
    {
        isCollected = false;
        isKilled = false;
        hasPowerPellet = false;
        isMotris = false;
        gameObject.SetActive(true);
        transform.position = respawnPoint.position;

        // 重新初始化速度
        if (ghostType == GhostType.Wooden)
        {
            normalSpeed = woodenInitialSpeed;
        }

        // 灯鬼重新进入沉眠状态
        if (ghostType == GhostType.Light && GameController.Instance.currentState == GameController.GameState.Normal)
        {
            StartCoroutine(StartChaseAfterDelay());
        }
    }
}