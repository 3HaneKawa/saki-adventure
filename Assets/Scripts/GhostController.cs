using UnityEngine;
using System.Collections;
using System;

public class GhostController : MonoBehaviour
{
    public enum GhostType { Light, Riki, Wooden, Vegetarian }
    public GhostType ghostType;

    public float normalSpeed = 3f;
    public float poweredSpeed = 2f;
    public float crazySpeed = 5f; // ��ϣ���״̬�ٶ�
    public float woodenInitialSpeed = 1.5f; // ľͷ�˳�ʼ�ٶ�

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

        // ��ʼ���ٶ�
        if (ghostType == GhostType.Wooden)
        {
            normalSpeed = woodenInitialSpeed;
        }

        // �ƹ��ʼ����
        if (ghostType == GhostType.Light && GameController.Instance.currentState == GameController.GameState.Normal)
        {
            StartCoroutine(StartChaseAfterDelay());
        }
    }

    void Update()
    {
        if (isCollected || isKilled)
            return;

        // �ƶ��߼�
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;

            // ���ݲ�ͬ״̬�����͵����ٶ�
            float currentSpeed = normalSpeed;

            // ��ϣ�����״̬�ҵƹ���ɱ�������״̬
            if (ghostType == GhostType.Riki &&
                GameController.Instance.currentState == GameController.GameState.Masked &&
                GameController.Instance.ghostLight != null &&
                GameController.Instance.ghostLight.isKilled)
            {
                currentSpeed = crazySpeed;
            }

            // ľͷ�������״̬�³Ե��������ΪMotris
            if (ghostType == GhostType.Wooden && isMotris)
            {
                currentSpeed = normalSpeed * 2; // ��Ϊ�����ٶȵ�����
            }

            rb.velocity = direction * currentSpeed;
        }
    }

    IEnumerator StartChaseAfterDelay()
    {
        // ��ʼ����
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(2f);

        // ��ʼ׷��
        normalSpeed = 3f; // �ָ������ٶ�
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // ľͷ�������Ƕ��и��ʽ����Ϊ�ƹ�
        if (ghostType == GhostType.Wooden &&
            !isMotris &&
            collision.CompareTag("Pellet") &&
            UnityEngine.Random.value < 0.3f) // 30%����
        {
            GameController.Instance.ConvertPelletToCucumber(collision.gameObject);
        }

        // ��Ե�������
        if (collision.CompareTag("PowerPellet") && hasPowerPellet)
        {
            // ľͷ�˳Ե��������ΪMotris
            if (ghostType == GhostType.Wooden && !isMotris && GameController.Instance.currentState == GameController.GameState.Masked)
            {
                isMotris = true;
                GameController.Instance.PlayerHit(); // ������Ѫ
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

        // ��ʾ�ռ���Ч
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

        // ��ʾ��ɱ��Ч
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

        // ���³�ʼ���ٶ�
        if (ghostType == GhostType.Wooden)
        {
            normalSpeed = woodenInitialSpeed;
        }

        // �ƹ����½������״̬
        if (ghostType == GhostType.Light && GameController.Instance.currentState == GameController.GameState.Normal)
        {
            StartCoroutine(StartChaseAfterDelay());
        }
    }
}