using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    // 单例模式
    public static GameController Instance { get; private set; }

    // 游戏状态
    public enum GameState { Normal, Masked }
    public GameState currentState = GameState.Normal;

    // 玩家引用
    public PlayerController player;

    // 鬼引用
    public GhostController[] ghosts;
    public GhostController ghostLight;    // 鬼1(灯)
    public GhostController ghostRiki;    // 鬼2(立希)
    public GhostController ghostWooden;  // 鬼3(木头人)
    public GhostController ghostVegetarian; // 鬼4(长期素食)

    // 道具引用
    public GameObject maskItem;
    public GameObject hatsuneItem;
    public List<GameObject> powerPellets;
    public List<GameObject> normalPellets;
    public List<GameObject> cucumbers;

    // UI元素
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI stateText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject rankingPanel;
    public Transform scoreListParent;
    public GameObject scoreEntryPrefab;

    // 游戏参数
    public int playerLives = 3;
    public int score = 0;
    public float gameTime = 300f; // 5分钟
    public float powerPelletDuration = 10f;
    public float maskDuration = 30f; // 面具持续时间
    public float ghostPowerPelletInterval = 20f; // 给鬼发放大力丸的间隔

    // 结局标志
    public bool chunriying = false;
    public bool ouneigai = false;

    // 排行榜
    private List<int> highScores = new List<int>();
    private const string HIGH_SCORE_KEY = "PacmanHighScores";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // 初始化排行榜
        LoadHighScores();
    }

    void Start()
    {
        // 初始化游戏
        UpdateUI();
        StartCoroutine(GhostPowerPelletTimer());

        // 生成面具
        if (currentState == GameState.Normal)
        {
            SpawnMask();
            StartCoroutine(MaskDespawnTimer());
        }
    }

    void Update()
    {
        // 更新游戏时间
        gameTime -= Time.deltaTime;
        if (gameTime <= 0)
            EndGame();

        // 检查结局条件
        CheckEndingConditions();

        // 玩家可以随时选择结束游戏
        if (Input.GetKeyDown(KeyCode.Escape))
            EndGame();
    }

    #region 状态管理
    public void EnterMaskedState()
    {
        if (currentState == GameState.Normal && !player.HasCollectedAnyGhost())
        {
            currentState = GameState.Masked;
            stateText.text = "面具状态";

            // 隐藏面具道具
            if (maskItem != null)
                maskItem.SetActive(false);

            // 生成初音
            SpawnHatsune();
        }
    }

    public void CollectGhost(GhostController ghost)
    {
        if (currentState == GameState.Normal)
        {
            player.CollectGhost(ghost);
            stateText.text = "正常状态 (已收集鬼)";
        }
        else if (currentState == GameState.Masked)
        {
            player.KillGhost(ghost);

            // 检查是否是最后一只鬼
            if (player.GetKilledGhostCount() == ghosts.Length - 1)
            {
                if (ghost == ghostVegetarian)
                    ouneigai = true;
            }
        }
    }
    #endregion

    #region 道具管理
    void SpawnMask()
    {
        if (maskItem != null)
            maskItem.SetActive(true);
    }

    IEnumerator MaskDespawnTimer()
    {
        yield return new WaitForSeconds(maskDuration);

        // 如果还没被拾取，移除面具
        if (currentState == GameState.Normal && maskItem != null && maskItem.activeSelf)
        {
            maskItem.SetActive(false);
        }
    }

    void SpawnHatsune()
    {
        if (hatsuneItem != null)
        {
            // 在随机位置生成初音
            Vector3 randomPos = GetRandomSpawnPosition();
            hatsuneItem.transform.position = randomPos;
            hatsuneItem.SetActive(true);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        // 这里简化处理，实际应该在地图有效位置生成
        Bounds mapBounds = GameObject.FindGameObjectWithTag("Map").GetComponent<BoxCollider2D>().bounds;
        float x = UnityEngine.Random.Range(mapBounds.min.x + 2f, mapBounds.max.x - 2f);
        float y = UnityEngine.Random.Range(mapBounds.min.y + 2f, mapBounds.max.y - 2f);
        return new Vector3(x, y, 0);
    }

    public void PlayerGotPowerPellet()
    {
        // 玩家获得大力丸
        StartCoroutine(PlayerPoweredState());

        // 检查春日影结局条件
        if (currentState == GameState.Masked && player.GetPowerPelletCount() >= 2 && ghostLight != null)
        {
            chunriying = true;
            EndGame();
        }
    }

    IEnumerator PlayerPoweredState()
    {
        player.EnterPoweredState();

        // 禁用所有鬼的大力丸
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

            // 随机给一只鬼发放大力丸
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
            // 随机选择一个黄瓜预制体
            GameObject cucumber = cucumbers[UnityEngine.Random.Range(0, cucumbers.Count)];

            // 在相同位置生成黄瓜
            GameObject newCucumber = Instantiate(cucumber, pellet.transform.position, Quaternion.identity);

            // 隐藏原来的糖豆
            pellet.SetActive(false);
        }
    }
    #endregion

    #region 玩家状态
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
        scoreText.text = "分数: " + score;
        livesText.text = "生命: " + playerLives;
        stateText.text = currentState == GameState.Normal ?
            (player.HasCollectedAnyGhost() ? "正常状态 (已收集鬼)" : "正常状态") :
            "面具状态";
    }
    #endregion

    #region 结局管理
    void CheckEndingConditions()
    {
        // 检查所有糖豆是否被吃完
        bool allPelletsEaten = true;
        foreach (var pellet in normalPellets)
        {
            if (pellet.activeSelf)
            {
                allPelletsEaten = false;
                break;
            }
        }

        // 如果所有糖豆被吃完，玩家可以选择结束游戏
        if (allPelletsEaten)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                EndGame();
        }
    }

    void EndGame()
    {
        // 添加时间奖励
        int timeBonus = Mathf.FloorToInt(gameTime * 10);
        score += timeBonus;

        // 更新排行榜
        UpdateHighScores(score);

        // 显示游戏结束面板
        gameOverPanel.SetActive(true);
        finalScoreText.text = "最终分数: " + score;

        // 显示结局信息
        string endingText = "普通结局";
        if (chunriying) endingText = "春日影结局";
        else if (ouneigai) endingText = "欧内该结局";

        finalScoreText.text += "\n结局: " + endingText;

        // 显示排行榜
        ShowRankingPanel();

        // 暂停游戏
        Time.timeScale = 0;
    }
    #endregion

    #region 排行榜管理
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

        // 排序
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

        // 只保留前10名
        if (highScores.Count > 10)
            highScores.RemoveRange(10, highScores.Count - 10);

        SaveHighScores();
    }

    void ShowRankingPanel()
    {
        // 清空现有条目
        foreach (Transform child in scoreListParent)
        {
            Destroy(child.gameObject);
        }

        // 显示排行榜
        for (int i = 0; i < highScores.Count; i++)
        {
            GameObject entry = Instantiate(scoreEntryPrefab, scoreListParent);
            entry.GetComponent<TextMeshProUGUI>().text =
                $"{i + 1}. {highScores[i]}";
        }

        rankingPanel.SetActive(true);
    }
    #endregion

    // 重新开始游戏
    public void RestartGame()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}