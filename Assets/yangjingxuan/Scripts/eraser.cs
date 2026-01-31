using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 改造后的刮刮乐脚本：
/// - 下层小 icon 为单独的 GameObject（带 IconTarget）
/// - 脚本在 Start 时把每个 icon 的 Rect 转换为 drawTexture 的像素矩形（RectInt）
/// - 每次擦除只检查与刷子相交的 icon，按像素统计透明比例判断是否清除并记分
/// </summary>
public class eraser : MonoBehaviour
{

    // 上层 mask（RawImage），我们在其 texture 上擦除 alpha
    public RawImage topImage;
    public Texture2D[] brushTextures;
    public Button[] brushButtons;
    public GameObject[] mouseImages;

    // 判定阈值
    [Tooltip("判定像素 alpha<=threshold 为已被擦除")]
    public float alphaThreshold = 0.1f;
    [Tooltip("判定为已清除需要的最小比例（0-1）")]
    public float requiredRatio = 2f / 3f;

    // 内部状态
    private Texture2D drawTexture;
    private int currentBrush = 0;
    private Canvas parentCanvas;

    // 每个 icon 对应的像素矩形与组件引用
    class IconData
    {
        public IconTarget target;
        public RectInt pixelRect; // 在 drawTexture 像素坐标系中的矩形（包含）
        public bool cleared;
    }
    private List<IconData> icons = new List<IconData>();

    void Start()
    {
        if (topImage == null)
        {
            Debug.LogError("eraser: topImage 未设置");
            enabled = false;
            return;
        }

        Texture2D original = topImage.texture as Texture2D;
        if (original == null)
        {
            Debug.LogError("eraser: topImage.texture 不是 Texture2D");
            enabled = false;
            return;
        }

        // 创建可写副本并替换 RawImage 的 texture
        drawTexture = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
        Graphics.CopyTexture(original, drawTexture);
        topImage.texture = drawTexture;

        parentCanvas = topImage.canvas;

        // 按钮切换 brush：除了切换 currentBrush，还切换 mouseImages 的显示
        if (brushButtons != null)
        {
            for (int i = 0; i < brushButtons.Length; i++)
            {
                int idx = i; // capture local copy
                if (brushButtons[i] != null)
                {
                    brushButtons[i].onClick.AddListener(() => OnBrushSelected(idx));
                }
            }
        }

        // 初始化 mouseImages 可见性（确保与 currentBrush 同步）
        UpdateMouseImages(currentBrush);

        // 查找场景中所有 IconTarget（你也可以手动赋值）
        IconTarget[] found = FindObjectsOfType<IconTarget>();
        foreach (var it in found)
        {
            IconData data = new IconData();
            data.target = it;
            data.cleared = it.isCleared;
            // 计算 icon 在 drawTexture 的像素矩形
            data.pixelRect = ComputeIconPixelRect(it.GetComponent<RectTransform>());
            icons.Add(data);
        }
    }

    // 新增：按钮回调集中处理
    private void OnBrushSelected(int idx)
    {
        currentBrush = Mathf.Clamp(idx, 0, (brushTextures != null ? brushTextures.Length - 1 : 0));
        UpdateMouseImages(currentBrush);
    }

    // 新增：根据索引显示对应 mouseImage，隐藏其他
    private void UpdateMouseImages(int activeIndex)
    {
        if (mouseImages == null || mouseImages.Length == 0) return;

        for (int i = 0; i < mouseImages.Length; i++)
        {
            var go = mouseImages[i];
            if (go == null) continue;
            bool shouldActive = (i == activeIndex);
            if (go.activeSelf != shouldActive)
                go.SetActive(shouldActive);
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 localPoint;
            Camera cam = (parentCanvas != null && (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera || parentCanvas.renderMode == RenderMode.WorldSpace))
                         ? parentCanvas.worldCamera
                         : null;

            bool inside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                topImage.rectTransform,
                Input.mousePosition,
                cam,
                out localPoint
            );
            if (!inside) return;

            Rect rect = topImage.rectTransform.rect;

            // 使用 rect.xMin/xMax 映射到 [0,1]
            float normalizedX = (localPoint.x - rect.xMin) / rect.width;
            float normalizedY = (localPoint.y - rect.yMin) / rect.height;

            // 如果归一化坐标超出 [0,1] 说明在 RawImage 可视区域外，禁止擦除
            if (normalizedX < 0f || normalizedX > 1f || normalizedY < 0f || normalizedY > 1f)
                return;

            // 考虑 uvRect（RawImage 可能只显示纹理的一部分）
            Rect uv = topImage.uvRect;
            float texU = uv.x + normalizedX * uv.width;
            float texV = uv.y + normalizedY * uv.height;

            // 如果映射到纹理 UV 超出 [0,1]，也禁止擦除（避免在纹理外写像素）
            if (texU < 0f || texU > 1f || texV < 0f || texV > 1f)
                return;

            int px = Mathf.FloorToInt(texU * drawTexture.width);
            int py = Mathf.FloorToInt(texV * drawTexture.height);

            px = Mathf.Clamp(px, 0, drawTexture.width - 1);
            py = Mathf.Clamp(py, 0, drawTexture.height - 1);

            EraseAt(px, py);
        }
    }

    void EraseAt(int x, int y)
    {
        if (brushTextures == null || brushTextures.Length == 0) return;
        Texture2D brush = brushTextures[Mathf.Clamp(currentBrush, 0, brushTextures.Length - 1)];
        if (brush == null) return;

        int bw = brush.width / 2;
        int bh = brush.height / 2;

        RectInt brushRect = new RectInt(x - bw, y - bh, brush.width, brush.height);
        // 实际擦除像素
        bool anyChanged = false;
        for (int i = 0; i < brush.width; i++)
        {
            int px = x + i - bw;
            if (px < 0 || px >= drawTexture.width) continue;
            for (int j = 0; j < brush.height; j++)
            {
                int py = y + j - bh;
                if (py < 0 || py >= drawTexture.height) continue;
                Color b = brush.GetPixel(i, j);
                if (b.a <= 0f) continue;
                Color dst = drawTexture.GetPixel(px, py);
                float newA = dst.a * (1f - b.a);
                if (newA < dst.a - 0.0001f)
                {
                    dst.a = newA;
                    drawTexture.SetPixel(px, py, dst);
                    anyChanged = true;
                }
            }
        }
        if (anyChanged) drawTexture.Apply();

        // 只检查与刷子相交且尚未清除的 icon
        foreach (var icon in icons)
        {
            if (icon.cleared) continue;
            if (!icon.pixelRect.Overlaps(brushRect)) continue;

            // 统计 icon 区域内已被擦除的像素比例
            int cleared = 0;
            int total = icon.pixelRect.width * icon.pixelRect.height;
            int x0 = icon.pixelRect.xMin;
            int y0 = icon.pixelRect.yMin;
            for (int ix = 0; ix < icon.pixelRect.width; ix++)
            {
                int pxIcon = x0 + ix;
                if (pxIcon < 0 || pxIcon >= drawTexture.width) continue;
                for (int jy = 0; jy < icon.pixelRect.height; jy++)
                {
                    int pyIcon = y0 + jy;
                    if (pyIcon < 0 || pyIcon >= drawTexture.height) continue;
                    Color c = drawTexture.GetPixel(pxIcon, pyIcon);
                    if (c.a <= alphaThreshold) cleared++;
                }
            }

            if (total > 0 && (float)cleared / total >= requiredRatio)
            {
                // 标记并触发 IconTarget 的 OnCleared
                icon.cleared = true;
                // IconTarget.OnCleared 负责设置 isCleared 和记分
                icon.target.OnCleared();
            }
        }
    }

    // 将 icon 的 RectTransform（UI）映射为 drawTexture 的像素 RectInt
    RectInt ComputeIconPixelRect(RectTransform iconRT)
    {
        RectInt pixelRect = new RectInt(0, 0, 0, 0);
        if (iconRT == null) return pixelRect;

        // 用上层 topImage 的 canvas 摄像机（或 null）
        Camera cam = (parentCanvas != null && (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera || parentCanvas.renderMode == RenderMode.WorldSpace))
                     ? parentCanvas.worldCamera
                     : null;

        // 获取 icon 在屏幕空间的四个角
        Vector3[] corners = new Vector3[4];
        iconRT.GetWorldCorners(corners);

        Rect rtRect = topImage.rectTransform.rect;
        Rect uv = topImage.uvRect;

        // 将四个角转换为 topImage 本地坐标，并映射到纹理 UV，再转换为像素，取包围盒
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

        // clamp 到纹理边界并构造 RectInt
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
