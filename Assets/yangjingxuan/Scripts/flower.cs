using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class flower : MonoBehaviour
{
    [Tooltip("判断像素 alpha<=threshold 为已被擦除/显现")]
    public float alphaThreshold = 0.1f;
    [Tooltip("判断为已显现需要的最小比例（0-1）")]
    public float requiredRatio = 2f / 3f;

    [Tooltip("可选，手动指定 eraser 使用的 RawImage；留空则自动查找 eraser")]
    public RawImage topImage;

    [Tooltip("触发时显示的 UI 面板（在 Inspector 指定）")]
    public GameObject revealPanel;

    [Tooltip("触发后暂停时长（秒，真实时间）")]
    public float pauseSeconds = 3f;

    [Tooltip("触发后增加的分数")]
    public int scoreIncrement = 6;

    private Texture2D drawTexture;
    private Canvas parentCanvas;
    private bool triggered = false;

    void Start()
    {
        if (topImage == null)
        {
            var er = FindObjectOfType<eraser>();
            if (er != null)
            {
                topImage = er.topImage;
                alphaThreshold = er.alphaThreshold;
                requiredRatio = er.requiredRatio;
            }
        }

        if (topImage != null)
        {
            parentCanvas = topImage.canvas;
            drawTexture = topImage.texture as Texture2D;
        }
        else
        {
            Debug.LogWarning("flower: topImage 未设置，无法检测显现比例");
        }

        if (revealPanel != null)
            revealPanel.SetActive(false);
    }

    void Update()
    {
        if (triggered) return;
        if (topImage == null) return;

        // refresh drawTexture in case eraser replaced it at runtime
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
        if (total > 0 && ratio >= requiredRatio)
        {
            StartCoroutine(HandleReveal());
            AudioManager.Instance.PlayShouting(Vector3.zero);
        }
    }

    IEnumerator HandleReveal()
    {
        triggered = true;
        Debug.Log("flower: revealed threshold reached, handling reveal.");

        // If GameManager requested to ignore next adverse event, consume and skip showing/pause
        bool consumedIgnore = false;
        if (GameManager.Instance != null)
        {
            consumedIgnore = GameManager.Instance.ConsumeIgnoreIfEligible();
            if (consumedIgnore)
                Debug.Log("flower: reveal ignored due to GameManager ignoreNextAdverse flag.");
        }

        if (consumedIgnore)
        {
            // still apply score increment if desired
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeScore(scoreIncrement);
            }
            yield break;
        }

        // show UI
        if (revealPanel != null) revealPanel.SetActive(true);

        // pause game (scaled time off); use realtime wait
        float prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(pauseSeconds);

        // hide UI and resume
        if (revealPanel != null) revealPanel.SetActive(false);
        Time.timeScale = prevTimeScale;

        // add score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeScore(scoreIncrement);
        }

        Debug.Log("flower: reveal handled, resumed game.");
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
