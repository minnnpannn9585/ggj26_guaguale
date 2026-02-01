using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public float timer;

    // If true, the next adverse event (score/time decrease, or reveal) will be ignored.
    private bool ignoreNextAdverse = false;
    private int ignoreSetFrame = -1;

    // Temporary multiplier for positive score additions (>=1.0). When multiplierRemaining > 0, positive points are multiplied.
    private float positiveScoreMultiplier = 1f;
    private float multiplierRemaining = 0f;

    // Temporary multiplier for negative score (penalties). When negativeMultiplierRemaining > 0, negative points are multiplied.
    private float negativeScoreMultiplier = 1f;
    private float negativeMultiplierRemaining = 0f;

    // New: optional UI prefab to show when negative penalties are multiplied
    [Tooltip("¿ÉÑ¡£ºµ±¼õ·Ö±»±¶ÔöÊ±ÔÚ UI ÉÏÏÔÊ¾µÄÔ¤ÖÆÌå£¨ÀýÈçÌáÊ¾¡®·£·Ö x2¡¯£©")]
    public GameObject negativeMultiplierTextPrefab;

    [Tooltip("¸ÃÌáÊ¾ÔÚ UI ÉÏÏÔÊ¾µÄ³ÖÐøÊ±¼ä£¨Ãë£©")]
    public float negativeMultiplierTextDuration = 1.5f;

    [Tooltip("ÌáÊ¾Ïà¶ÔÓÚ Canvas ÖÐÐÄµÄÆ«ÒÆ£¨Canvas ±¾µØ×ø±ê£©")]
    public Vector2 negativeMultiplierTextOffset = Vector2.zero;

    // --- New fields for game over handling ---
    [Header("Game Over UI")]
    [Tooltip("Panel to show when player wins")]
    public GameObject winPanel;
    [Tooltip("Panel to show when player loses")]
    public GameObject losePanel;
    [Tooltip("Score threshold to consider a win (Score >= threshold => Win)")]
    public int winScoreThreshold = 100;

    // Optional references to set final score text in the panels (TextMeshPro)
    public TMPro.TextMeshProUGUI winPanelScoreText;
    public TMPro.TextMeshProUGUI losePanelScoreText;

    // Tracks game-over state so EndGame is idempotent
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to scene loaded to manage cursor visibility per scene.
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Ensure cursor state is correct for the scene we started in
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks / dangling delegates
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (Instance == this)
            Instance = null;
    }

    // Scene loaded callback: hide cursor in non-main scenes, show in main menu (index 0).
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If your main menu is not buildIndex 0, change this check to scene.name or desired index.
        if (scene.buildIndex == 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            // Do not change lockState aggressively; keep default behavior. If you want to lock:
            // Cursor.lockState = CursorLockMode.None;
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        if(timer >= 59f)
        {
            timer = 59f;
        }
        // tick timer
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            EndGame();
        }

        // tick multiplier duration for positive
        if (multiplierRemaining > 0f)
        {
            multiplierRemaining -= Time.deltaTime;
            if (multiplierRemaining <= 0f)
            {
                multiplierRemaining = 0f;
                positiveScoreMultiplier = 1f;
                Debug.Log("GameManager: positive score multiplier expired, back to 1x.");
            }
        }

        // tick multiplier duration for negative
        if (negativeMultiplierRemaining > 0f)
        {
            negativeMultiplierRemaining -= Time.deltaTime;
            if (negativeMultiplierRemaining <= 0f)
            {
                negativeMultiplierRemaining = 0f;
                negativeScoreMultiplier = 1f;
                Debug.Log("GameManager: negative score multiplier expired, back to 1x.");
            }
        }
    }

    // Call this to request that the next adverse event (score/time decrease or reveal) be ignored.
    public void SetIgnoreNextAdverseEvent()
    {
        ignoreNextAdverse = true;
        ignoreSetFrame = Time.frameCount;
        Debug.Log("GameManager: ignoreNextAdverse set at frame " + ignoreSetFrame);
    }

    // If an adverse event occurs (score decrease / timer decrease / reveal), callers should call this
    // to determine whether the event should be ignored. It returns true if the ignore was consumed.
    public bool ConsumeIgnoreIfEligible()
    {
        if (!ignoreNextAdverse) return false;
        // only allow consume when we're in a later frame than when the ignore was set
        if (Time.frameCount <= ignoreSetFrame) return false;
        ignoreNextAdverse = false;
        Debug.Log("GameManager: consumed ignoreNextAdverse at frame " + Time.frameCount);
        return true;
    }

    // Start a temporary multiplier for positive score additions. multiplier should be >= 1 (e.g., 2 for double).
    public void ApplyTemporaryPositiveScoreMultiplier(float multiplier, float duration)
    {
        if (multiplier <= 0f || duration <= 0f) return;
        positiveScoreMultiplier = multiplier;
        multiplierRemaining = duration;
        Debug.Log($"GameManager: applied positive score multiplier x{multiplier} for {duration} seconds.");
    }

    // Start a temporary multiplier for negative score (penalties). multiplier should be >= 1 (e.g., 2 for double penalties).
    public void ApplyTemporaryNegativeScoreMultiplier(float multiplier, float duration)
    {
        if (multiplier <= 0f || duration <= 0f) return;
        negativeScoreMultiplier = multiplier;
        negativeMultiplierRemaining = duration;
        Debug.Log($"GameManager: applied negative score multiplier x{multiplier} for {duration} seconds.");
    }

    // Return the currently active positive multiplier (1 if none)
    public float GetPositiveScoreMultiplier()
    {
        return positiveScoreMultiplier;
    }

    // Return the currently active negative multiplier (1 if none)
    public float GetNegativeScoreMultiplier()
    {
        return negativeScoreMultiplier;
    }

    // ChangeScore now returns the actual points applied (after multiplier) so callers can respond (e.g. show doubled indicator)
    // Optional sourceWorldPos can be provided to position UI feedback near the source of the score change.
    public int ChangeScore(int points, Vector3? sourceWorldPos = null)
    {
        // If this is a decrease and the ignore is eligible, consume and skip applying it
        if (points < 0 && ConsumeIgnoreIfEligible())
        {
            Debug.Log("GameManager: ignored score decrease of " + points);
            return 0;
        }

        int appliedPoints = points;
        // apply multiplier only to positive additions
        if (points > 0 && positiveScoreMultiplier != 1f)
        {
            appliedPoints = Mathf.RoundToInt(points * positiveScoreMultiplier);
        }
        // apply multiplier only to negative penalties (preserve sign)
        else if (points < 0 && negativeScoreMultiplier != 1f)
        {
            appliedPoints = Mathf.RoundToInt(points * negativeScoreMultiplier);
        }

        Score += appliedPoints;
        Debug.Log("Score: " + Score);

        // If this was a negative points call and a negative multiplier was applied, show UI feedback
        if (points < 0 && negativeScoreMultiplier != 1f && negativeMultiplierTextPrefab != null)
        {
            // try to find a Canvas to parent to
            Canvas uiCanvas = FindObjectOfType<Canvas>();
            if (uiCanvas != null)
            {
                GameObject go = Instantiate(negativeMultiplierTextPrefab, uiCanvas.transform);
                RectTransform goRt = go.GetComponent<RectTransform>();
                RectTransform canvasRt = uiCanvas.GetComponent<RectTransform>();

                Camera cam = (uiCanvas.renderMode == RenderMode.ScreenSpaceCamera) ? uiCanvas.worldCamera : null;

                if (sourceWorldPos.HasValue)
                {
                    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, sourceWorldPos.Value);
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPos, cam, out localPoint);
                    if (goRt != null)
                        goRt.anchoredPosition = localPoint + negativeMultiplierTextOffset;
                }
                else if (goRt != null)
                {
                    // place near canvas center plus offset
                    goRt.anchoredPosition = negativeMultiplierTextOffset;
                }

                if (negativeMultiplierTextDuration > 0f)
                    Destroy(go, negativeMultiplierTextDuration);
            }
        }

        return appliedPoints;
    }

    // Public method to end the game. Chooses win/lose panel based on Score vs winScoreThreshold,
    // makes the choice only once, and pauses the game.
    public void EndGame()
    {
        if (isGameOver) return;
        isGameOver = true;

        bool playerWon = Score >= winScoreThreshold;

        // Activate appropriate panel and set score text if assigned
        if (playerWon)
        {
            if (winPanel != null) winPanel.SetActive(true);
            if (winPanelScoreText != null) winPanelScoreText.text = "最终得分：" + Score.ToString();
        }
        else
        {
            if (losePanel != null) losePanel.SetActive(true);
            if (losePanelScoreText != null) losePanelScoreText.text = "最终得分：" + Score.ToString();
        }

        // Pause game world (physics/time-based updates). UI animations that should run must use unscaled time.
        Time.timeScale = 0f;

        // Optional: make cursor visible / unlock (useful for desktop)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log($"GameManager.EndGame: {(playerWon ? "Win" : "Lose")}, Score={Score}");
    }

    public void ChangeTimer(float time)
    {
        // If this is a decrease and the ignore is eligible, consume and skip applying it
        if (time < 0f && ConsumeIgnoreIfEligible())
        {
            Debug.Log("GameManager: ignored timer decrease of " + time);
            return;
        }

        timer += time;
        if(timer >= 59)
        {
            timer = 59;
        }
    }
}
