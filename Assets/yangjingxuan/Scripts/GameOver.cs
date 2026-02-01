using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [Tooltip("判定像素 alpha<=threshold 为已被擦除")]
    public float alphaThreshold = 0.1f;
    [Tooltip("判定为已显现需要的最小比例（0-1）")]
    public float requiredRatio = 2f / 3f;

    // optional reference to the RawImage used by eraser; if null we'll try to find the eraser instance
    public RawImage topImage;

    // UI to show on game over (assign in Inspector)
    public GameObject gameOverPanel;
    public Text finalScoreText; // optional

    private Texture2D drawTexture;
    private Canvas parentCanvas;
    private bool gameOverTriggered = false;

    // One-shot flag: when true the next TriggerGameOver call will be ignored and the flag cleared
    private bool ignoreNextGameOver = false;

    void Start()
    {
        if (topImage == null)
        {
            var er = FindObjectOfType<eraser>();
            if (er != null)
            {
                alphaThreshold = er.alphaThreshold;
                requiredRatio = er.requiredRatio;
                topImage = er.topImage;
            }
        }

        if (topImage != null)
        {
            parentCanvas = topImage.canvas;
            drawTexture = topImage.texture as Texture2D;
            if (drawTexture == null)
            {
                Debug.LogWarning("GameOver: topImage.texture is not a Texture2D or not initialized yet.");
            }
        }
        else
        {
            Debug.LogWarning("GameOver: topImage not assigned and eraser not found.");
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (gameOverTriggered) return;
        if (topImage == null) return;

        // ALWAYS refresh drawTexture from topImage.texture to use the writable texture created by eraser
        drawTexture = topImage.texture as Texture2D;
        if (drawTexture == null) return;

        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;

        RectInt pixelRect = ComputeIconPixelRect(rt);
        if (pixelRect.width <= 0 || pixelRect.height <= 0) return;

        int cleared = 0;
        int total = pixelRect.width * pixelRect.height;
        int x0 = pixelRect.xMin;
        int y0 = pixelRect.yMin;

        for (int ix = 0; ix < pixelRect.width; ix++)
        {
            int px = x0 + ix;
            if (px < 0 || px >= drawTexture.width) continue;
            for (int jy = 0; jy < pixelRect.height; jy++)
            {
                int py = y0 + jy;
                if (py < 0 || py >= drawTexture.height) continue;
                Color c = drawTexture.GetPixel(px, py);
                if (c.a <= alphaThreshold) cleared++;
            }
        }

        float ratio = total > 0 ? (float)cleared / total : 0f;
        // debug log to help diagnose
        Debug.Log($"GameOver check: cleared={cleared}, total={total}, ratio={ratio}");

        if (total > 0 && ratio >= requiredRatio)
        {
            TriggerGameOver();
            AudioManager.Instance.PlayBoomSound(Vector3.zero);
        }
    }

    // Make this public so other scripts (IconTarget) can force game over immediately
    public void TriggerGameOver()
    {
        // First check centralized GameManager ignore flag
        //if (GameManager.Instance != null && GameManager.Instance.ConsumeIgnoreNextPanel())
        //{
        //    Debug.Log("GameOver: TriggerGameOver ignored due to GameManager ignoreNextPanel flag.");
        //    return;
        //}

        // If ignore flag is set locally, consume it and do not trigger
        if (ignoreNextGameOver)
        {
            ignoreNextGameOver = false;
            Debug.Log("GameOver: TriggerGameOver ignored due to local ignore flag.");
            return;
        }

        if (gameOverTriggered) return;
        gameOverTriggered = true;
        Debug.Log("GameOver: revealed ratio reached, ending game.");

        // Ensure game-over UI animations run even if timeScale is set to 0.
        // Many Animator-driven UI animations stop when Time.timeScale == 0 unless the Animator's
        // updateMode is set to UnscaledTime. We set that and restart the animator states here.
        if (gameOverPanel != null)
        {
            Animator[] anims = gameOverPanel.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < anims.Length; i++)
            {
                var a = anims[i];
                // switch to unscaled time so the animator continues when Time.timeScale == 0
                a.updateMode = AnimatorUpdateMode.UnscaledTime;
                // restart the current state so the animation begins visibly on the panel
                var state = a.GetCurrentAnimatorStateInfo(0);
                a.Play(state.shortNameHash, 0, 0f);
            }

            // For legacy Animation components, restart them as well (they are affected by timeScale).
            Animation[] legacyAnims = gameOverPanel.GetComponentsInChildren<Animation>(true);
            for (int i = 0; i < legacyAnims.Length; i++)
            {
                var la = legacyAnims[i];
                if (la != null && la.clip != null)
                {
                    la.Play(la.clip.name);
                }
            }
        }

        // Freeze game world but keep UI animations using unscaled time
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null && GameManager.Instance != null)
            {
                finalScoreText.text = "Final Score: " + GameManager.Instance.Score.ToString();
            }
        }
    }

    // Public API to activate the one-time ignore
    public void ActivateIgnoreNextGameOver()
    {
        ignoreNextGameOver = true;
        Debug.Log("GameOver: next game over/panel will be ignored.");
    }

    // Map RectTransform to drawTexture pixel rect (same logic as eraser)
    RectInt ComputeIconPixelRect(RectTransform iconRT)
    {
        RectInt pixelRect = new RectInt(0, 0, 0, 0);
        if (iconRT == null || topImage == null || drawTexture == null) return pixelRect;

        Camera cam = (parentCanvas != null && (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera || parentCanvas.renderMode == RenderMode.WorldSpace))
                     ? parentCanvas.worldCamera
                     : null;

        Vector3[] corners = new Vector3[4];
        iconRT.GetWorldCorners(corners);

        Rect rtRect = topImage.rectTransform.rect;
        Rect uv = topImage.uvRect;

        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        for (int k = 0; k < 4; k++)
        {
            Vector3 worldCorner = corners[k];
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldCorner);
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(topImage.rectTransform, screenPoint, cam, out localPoint);

            float normalizedX = (localPoint.x - rtRect.xMin) / rtRect.width;
            float normalizedY = (localPoint.y - rtRect.yMin) / rtRect.height;

            float texU = uv.x + normalizedX * uv.width;
            float texV = uv.y + normalizedY * uv.height;

            int px = Mathf.FloorToInt(texU * drawTexture.width);
            int py = Mathf.FloorToInt(texV * drawTexture.height);

            minX = Mathf.Min(minX, px);
            minY = Mathf.Min(minY, py);
            maxX = Mathf.Max(maxX, px);
            maxY = Mathf.Max(maxY, py);
        }

        minX = Mathf.Clamp(minX, 0, drawTexture.width - 1);
        minY = Mathf.Clamp(minY, 0, drawTexture.height - 1);
        maxX = Mathf.Clamp(maxX, 0, drawTexture.width - 1);
        maxY = Mathf.Clamp(maxY, 0, drawTexture.height - 1);

        pixelRect.x = minX;
        pixelRect.y = minY;
        pixelRect.width = Mathf.Max(1, maxX - minX + 1);
        pixelRect.height = Mathf.Max(1, maxY - minY + 1);

        return pixelRect;
    }
}
