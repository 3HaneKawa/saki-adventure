using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    // ����ģʽ
    public static GameController Instance { get; private set; }

    // ��Ϸ״̬
    public enum GameState { Normal, Masked }
    public GameState currentState = GameState.Normal;

    // �������
    public PlayerController player;

    // ������
    public GhostController[] ghosts;
    public GhostController ghostLight;    // ��1(��)
    public GhostController ghostRiki;    // ��2(��ϣ)
    public GhostController ghostWooden;  // ��3(ľͷ��)
    public GhostController ghostVegetarian; // ��4(������ʳ)

    // ��������
    public GameObject maskItem;
    public GameObject hatsuneItem;
    public List<GameObject> powerPellets;
    public List<GameObject> normalPellets;
    public List<GameObject> cucumbers;

    // UIԪ��
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI stateText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject rankingPanel;
    public Transform scoreListParent;
    public GameObject scoreEntryPrefab;

    // ��Ϸ����
    public int playerLives = 3;
    public int score = 0;
    public float gameTime = 300f; // 5����
    public float powerPelletDuration = 10f;
    public float maskDuration = 30f; // ��߳���ʱ��
    public float ghostPowerPelletInterval = 20f; // �����Ŵ�����ļ��

    // ��ֱ�־
    public bool chunriying = false;
    public bool ouneigai = false;

    // ���а�
    private List<int> highScores = new List<int>();
    private const string HIGH_SCORE_KEY = "PacmanHighScores";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // ��ʼ�����а�
        LoadHighScores();
    }

    void Start()
    {
        // ��ʼ����Ϸ
        UpdateUI();
        StartCoroutine(GhostPowerPelletTimer());

        // �������
        if (currentState == GameState.Normal)
        {
            SpawnMask();
            StartCoroutine(MaskDespawnTimer());
        }
    }

    void Update()
    {
        // ������Ϸʱ��
        gameTime -= Time.deltaTime;
        if (gameTime <= 0)
            EndGame();

        // ���������
        CheckEndingConditions();

        // ��ҿ�����ʱѡ�������Ϸ
        if (Input.GetKeyDown(KeyCode.Escape))
            EndGame();
    }

    #region ״̬����
    public void EnterMaskedState()
    {
        if (currentState == GameState.Normal && !player.HasCollectedAnyGhost())
        {
            currentState = GameState.Masked;
            stateText.text = "���״̬";

            // ������ߵ���
            if (maskItem != null)
                maskItem.SetActive(false);

            // ���ɳ���
            SpawnHatsune();
        }
    }

    public void CollectGhost(GhostController ghost)
    {
        if (currentState == GameState.Normal)
        {
            player.CollectGhost(ghost);
            stateText.text = "����״̬ (���ռ���)";
        }
        else if (currentState == GameState.Masked)
        {
            player.KillGhost(ghost);

            // ����Ƿ������һֻ��
            if (player.GetKilledGhostCount() == ghosts.Length - 1)
            {
                if (ghost == ghostVegetarian)
                    ouneigai = true;
            }
        }
    }
    #endregion

    #region ���߹���
    void SpawnMask()
    {
        if (maskItem != null)
            maskItem.SetActive(true);
    }

    IEnumerator MaskDespawnTimer()
    {
        yield return new WaitForSeconds(maskDuration);

        // �����û��ʰȡ���Ƴ����
        if (currentState == GameState.Normal && maskItem != null && maskItem.activeSelf)
        {
            maskItem.SetActive(false);
        }
    }

    void SpawnHatsune()
    {
        if (hatsuneItem != null)
        {
            // �����λ�����ɳ���
            Vector3 randomPos = GetRandomSpawnPosition();
            hatsuneItem.transform.position = randomPos;
            hatsuneItem.SetActive(true);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        // ����򻯴���ʵ��Ӧ���ڵ�ͼ��Чλ������
        Bounds mapBounds = GameObject.FindGameObjectWithTag("Map").GetComponent<BoxCollider2D>().bounds;
        float x = UnityEngine.Random.Range(mapBounds.min.x + 2f, mapBounds.max.x - 2f);
        float y = UnityEngine.Random.Range(mapBounds.min.y + 2f, mapBounds.max.y - 2f);
        return new Vector3(x, y, 0);
    }

    public void PlayerGotPowerPellet()
    {
        // ��һ�ô�����
        StartCoroutine(PlayerPoweredState());

        // ��鴺��Ӱ�������
        if (currentState == GameState.Masked && player.GetPowerPelletCount() >= 2 && ghostLight != null)
        {
            chunriying = true;
            EndGame();
        }
    }

    IEnumerator PlayerPoweredState()
    {
        player.EnterPoweredState();

        // �������й�Ĵ�����
        foreach (var ghost in ghosts)
        {
            ghost.DisablePowerPellet();
        }

        yield return new WaitForSeconds(powerPelletDuration);

        player.ExitPoweredState();
    }

    IEnumerator GhostPowerPelletTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(ghostPowerPelletInterval);

            // �����һֻ���Ŵ�����
            if (ghosts.Length > 0 && currentState == GameState.Masked)
            {
                int randomIndex = UnityEngine.Random.Range(0, ghosts.Length);
                ghosts[randomIndex].EnablePowerPellet();
            }
        }
    }

    public void ConvertPelletToCucumber(GameObject pellet)
    {
        if (cucumbers.Count > 0)
        {
            // ���ѡ��һ���ƹ�Ԥ����
            GameObject cucumber = cucumbers[UnityEngine.Random.Range(0, cucumbers.Count)];

            // ����ͬλ�����ɻƹ�
            GameObject newCucumber = Instantiate(cucumber, pellet.transform.position, Quaternion.identity);

            // ����ԭ�����Ƕ�
            pellet.SetActive(false);
        }
    }
    #endregion

    #region ���״̬
    public void PlayerHit()
    {
        playerLives--;
        UpdateUI();

        if (playerLives <= 0)
            EndGame();
        else
            player.ResetPosition();
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = "����: " + score;
        livesText.text = "����: " + playerLives;
        stateText.text = currentState == GameState.Normal ?
            (player.HasCollectedAnyGhost() ? "����״̬ (���ռ���)" : "����״̬") :
            "���״̬";
    }
    #endregion

    #region ��ֹ���
    void CheckEndingConditions()
    {
        // ��������Ƕ��Ƿ񱻳���
        bool allPelletsEaten = true;
        foreach (var pellet in normalPellets)
        {
            if (pellet.activeSelf)
            {
                allPelletsEaten = false;
                break;
            }
        }

        // ��������Ƕ������꣬��ҿ���ѡ�������Ϸ
        if (allPelletsEaten)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                EndGame();
        }
    }

    void EndGame()
    {
        // ���ʱ�佱��
        int timeBonus = Mathf.FloorToInt(gameTime * 10);
        score += timeBonus;

        // �������а�
        UpdateHighScores(score);

        // ��ʾ��Ϸ�������
        gameOverPanel.SetActive(true);
        finalScoreText.text = "���շ���: " + score;

        // ��ʾ�����Ϣ
        string endingText = "��ͨ���";
        if (chunriying) endingText = "����Ӱ���";
        else if (ouneigai) endingText = "ŷ�ڸý��";

        finalScoreText.text += "\n���: " + endingText;

        // ��ʾ���а�
        ShowRankingPanel();

        // ��ͣ��Ϸ
        Time.timeScale = 0;
    }
    #endregion

    #region ���а����
    void LoadHighScores()
    {
        string scoresString = PlayerPrefs.GetString(HIGH_SCORE_KEY, "");
        if (!string.IsNullOrEmpty(scoresString))
        {
            string[] scoreStrings = scoresString.Split(',');
            foreach (string s in scoreStrings)
            {
                if (int.TryParse(s, out int score))
                {
                    highScores.Add(score);
                }
            }
        }

        // ����
        highScores.Sort((a, b) => b.CompareTo(a));
    }

    void SaveHighScores()
    {
        string scoresString = string.Join(",", highScores);
        PlayerPrefs.SetString(HIGH_SCORE_KEY, scoresString);
        PlayerPrefs.Save();
    }

    void UpdateHighScores(int newScore)
    {
        highScores.Add(newScore);
        highScores.Sort((a, b) => b.CompareTo(a));

        // ֻ����ǰ10��
        if (highScores.Count > 10)
            highScores.RemoveRange(10, highScores.Count - 10);

        SaveHighScores();
    }

    void ShowRankingPanel()
    {
        // ���������Ŀ
        foreach (Transform child in scoreListParent)
        {
            Destroy(child.gameObject);
        }

        // ��ʾ���а�
        for (int i = 0; i < highScores.Count; i++)
        {
            GameObject entry = Instantiate(scoreEntryPrefab, scoreListParent);
            entry.GetComponent<TextMeshProUGUI>().text =
                $"{i + 1}. {highScores[i]}";
        }

        rankingPanel.SetActive(true);
    }
    #endregion

    // ���¿�ʼ��Ϸ
    public void RestartGame()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}