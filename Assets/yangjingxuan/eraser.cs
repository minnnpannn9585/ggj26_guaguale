using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class eraser : MonoBehaviour
{

    public RawImage topImage;           // 上层可擦除图片
    public Texture2D[] brushTextures;   // 多种擦除刷子
    public Button[] brushButtons;       // 刷子切换按钮
    public Texture2D iconTexture;       // 下层图标
    public int scoreIncrement = 10;     // 分数增量
    public Text scoreText;              // 分数显示

    private Texture2D drawTexture;
    private int currentBrush = 0;
    private bool iconCleared = false;
    private int score = 0;
    private Vector2 iconAutoPosition;   // 自动识别的图标位置

    void Start()
    {
        // 创建可擦除纹理副本
        Texture2D original = (Texture2D)topImage.texture;
        drawTexture = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
        Graphics.CopyTexture(original, drawTexture);
        topImage.texture = drawTexture;

        // 初始化刷子按钮
        for (int i = 0; i < brushButtons.Length; i++)
        {
            int index = i;
            brushButtons[i].onClick.AddListener(() => { currentBrush = index; });
        }

        // 自动找到图标位置
        iconAutoPosition = FindIconPosition((Texture2D)topImage.texture, iconTexture);

        UpdateScoreText();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                topImage.rectTransform,
                Input.mousePosition,
                null,
                out localPoint
            );
            Rect rect = topImage.rectTransform.rect;
            float x = (localPoint.x - rect.x) / rect.width * drawTexture.width;
            float y = (localPoint.y - rect.y) / rect.height * drawTexture.height;
            EraseAt(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        }
    }

    void EraseAt(int x, int y)
    {
        Texture2D brush = brushTextures[currentBrush];
        int bw = brush.width / 2;
        int bh = brush.height / 2;

        for (int i = 0; i < brush.width; i++)
        {
            for (int j = 0; j < brush.height; j++)
            {
                int px = x + i - bw;
                int py = y + j - bh;
                if (px >= 0 && px < drawTexture.width && py >= 0 && py < drawTexture.height)
                {
                    Color brushPixel = brush.GetPixel(i, j);
                    if (brushPixel.a > 0)
                    {
                        Color dstPixel = drawTexture.GetPixel(px, py);
                        drawTexture.SetPixel(px, py, new Color(dstPixel.r, dstPixel.g, dstPixel.b, dstPixel.a * (1 - brushPixel.a)));
                    }
                }
            }
        }
        drawTexture.Apply();

        if (!iconCleared)
        {
            int clearedPixels = 0;
            int totalPixels = iconTexture.width * iconTexture.height;
            for (int i = 0; i < iconTexture.width; i++)
            {
                for (int j = 0; j < iconTexture.height; j++)
                {
                    int px = (int)iconAutoPosition.x + i;
                    int py = (int)iconAutoPosition.y + j;
                    if (px >= 0 && px < drawTexture.width && py >= 0 && py < drawTexture.height)
                    {
                        Color pixel = drawTexture.GetPixel(px, py);
                        if (pixel.a <= 0.1f)
                            clearedPixels++;
                    }
                }
            }
            if ((float)clearedPixels / totalPixels >= 2f / 3f)
            {
                iconCleared = true;
                score += scoreIncrement;
                UpdateScoreText();
            }
        }
    }

    Vector2 FindIconPosition(Texture2D background, Texture2D icon)
    {
        // 简单暴力匹配：在背景中寻找完全匹配的图标区域
        for (int x = 0; x <= background.width - icon.width; x++)
        {
            for (int y = 0; y <= background.height - icon.height; y++)
            {
                bool match = true;
                for (int i = 0; i < icon.width && match; i++)
                {
                    for (int j = 0; j < icon.height && match; j++)
                    {
                        Color bgColor = background.GetPixel(x + i, y + j);
                        Color iconColor = icon.GetPixel(i, j);
                        if (Mathf.Abs(bgColor.r - iconColor.r) > 0.01f ||
                            Mathf.Abs(bgColor.g - iconColor.g) > 0.01f ||
                            Mathf.Abs(bgColor.b - iconColor.b) > 0.01f)
                        {
                            match = false;
                        }
                    }
                }
                if (match) return new Vector2(x, y);
            }
        }
        // 找不到返回0,0
        return Vector2.zero;
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "分数: " + score;
    }
}
