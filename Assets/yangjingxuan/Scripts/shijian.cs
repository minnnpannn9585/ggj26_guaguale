using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class shijian : MonoBehaviour
{
    
        [Tooltip("判定像素 alpha<=threshold 为已被擦除（显现）")]
        public float alphaThreshold = 0.1f;
        [Tooltip("判定为已显现需要的最小比例（0-1）")]
        public float requiredRatio = 2f / 3f;

        [Tooltip("时间增减的数值（正数增加，负数减少）")]
        public float timeDelta = 5f;

        // optional reference to the RawImage used by eraser; if null we'll try to find the eraser instance
        public RawImage topImage;

        private Texture2D drawTexture;
        private Canvas parentCanvas;
        private bool applied = false;

        void Start()
        {
            if (topImage == null)
            {
                var er = FindObjectOfType<eraser>();
                if (er != null)
                {
                    // adopt eraser thresholds by default
                    alphaThreshold = er.alphaThreshold;
                    requiredRatio = er.requiredRatio;
                    topImage = er.topImage;
                }
            }

            if (topImage != null)
            {
                parentCanvas = topImage.canvas;
                drawTexture = topImage.texture as Texture2D;
            }
        }

        void Update()
        {
            if (applied) return;
            if (topImage == null) return;

            // always refresh drawTexture (eraser may replace texture at runtime)
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
            // debug can be enabled by uncommenting
            // Debug.Log($"TimeTrigger check: cleared={cleared}, total={total}, ratio={ratio}");

            if (total > 0 && ratio >= requiredRatio)
            {
                ApplyTimeDelta();
                applied = true;
            }
        }

        public void ApplyTimeDelta()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeTimer(timeDelta);
                Debug.Log($"TimeTrigger: applied timeDelta {timeDelta}, new timer = {GameManager.Instance.timer}");
            }
            else
            {
                Debug.LogWarning("TimeTrigger: GameManager.Instance is null; cannot apply time delta.");
            }
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

    /// <summary>
    /// Attach this to the same GameObject as IconTarget.
    /// When the IconTarget becomes cleared (isCleared == true) this component will apply a time delta via GameManager.ChangeTimer.
    /// timeDelta is editable in the Inspector (positive to add time, negative to subtract).
    /// </summary>
    public class TimeOnCleared : MonoBehaviour
    {
        [Tooltip("时间增减的数值（正数增加，负数减少)")]
        public float timeDelta = 5f;

        [Tooltip("是否只在第一次清除时应用（默认 true）")]
        public bool applyOnce = true;

        private IconTarget iconTarget;
        private bool applied = false;

        void Start()
        {
            iconTarget = GetComponent<IconTarget>();
            if (iconTarget == null)
            {
                Debug.LogWarning("TimeOnCleared: no IconTarget found on the same GameObject. This component expects IconTarget to be present.");
            }
        }

        void Update()
        {
            if (iconTarget == null) return;
            if (applied && applyOnce) return;

            if (iconTarget.isCleared)
            {
                ApplyTimeDelta();
                applied = true;
            }
        }

        public void ApplyTimeDelta()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeTimer(timeDelta);
                Debug.Log($"TimeOnCleared: applied timeDelta {timeDelta}, new timer = {GameManager.Instance.timer}");
            }
            else
            {
                Debug.LogWarning("TimeOnCleared: GameManager.Instance is null; cannot change timer.");
            }
        }
    
}
