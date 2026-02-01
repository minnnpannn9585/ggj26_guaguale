using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Tooltip("可选：当减分被倍增时在 UI 上显示的预制体（例如提示‘罚分 x2’）")]
    public GameObject negativeMultiplierTextPrefab;

    [Tooltip("该提示在 UI 上显示的持续时间（秒）")]
    public float negativeMultiplierTextDuration = 1.5f;

    [Tooltip("提示相对于 Canvas 中心的偏移（Canvas 本地坐标）")]
    public Vector2 negativeMultiplierTextOffset = Vector2.zero;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if(timer >= 59f)
        {
            timer = 59f;
        }
        // tick timer
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Debug.Log("Time's up!");
            // Handle end of game logic here
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

    public void ChangeTimer(float time)
    {
        // If this is a decrease and the ignore is eligible, consume and skip applying it
        if (time < 0f && ConsumeIgnoreIfEligible())
        {
            Debug.Log("GameManager: ignored timer decrease of " + time);
            return;
        }

        timer += time;
        if(timer >= 99)
        {
            timer = 99;
        }
    }


}
