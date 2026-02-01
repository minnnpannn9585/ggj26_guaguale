using UnityEngine;

/// <summary>
/// 挂在每个小 icon 上的组件（图标现在为独立 GameObject）
/// 当被擦除判定为“已清除”时会触发 OnCleared（默认隐藏物体）
/// 可自定义在 OnCleared 中播放动画、声效等
/// </summary>
public class IconTarget : MonoBehaviour
{
    // 已被清除（得分）标记
    [HideInInspector] public bool isCleared = false;

    // 每个 icon 的得分（现在将记分逻辑放到这里）
    public int scoreValue = 10;

    // 可选的动画播放支持：优先使用 Animator 的 Trigger，其次回退到 legacy Animation 组件
    [Tooltip("可选：为该图标指定 Animator 组件，以在清除时播放动画（SetTrigger）")]
    public Animator animator;

    [Tooltip("Animator 的触发器参数名，留空则使用默认 'Cleared'")]
    public string animatorTrigger = "Cleared";

    [Tooltip("如果没有 Animator，可选 legacy Animation 组件名（Play）")]
    public Animation legacyAnimation;

    [Tooltip("清除后是否隐藏该 GameObject（保留原行为）")]
    public bool hideOnCleared = true;

    // 新增：显示得分文本的预制体（例如一个包含 Text 或 TextMeshProUGUI 的 UI 元素）
    [Tooltip("可选：在得分时显示的 UI 文本预制体，预制体应为 Canvas 下的 UI 元素（RectTransform）")]
    public GameObject scoreTextPrefab;

    [Tooltip("新建的得分文本在 Canvas 中显示的持续时间（秒）")]
    public float scoreTextDuration = 1.5f;

    [Tooltip("得分文本相对图标位置的偏移（以 Canvas 本地坐标为准）")]
    public Vector2 scoreTextOffset = Vector2.zero;

    // 新增：当得分被倍数影响（例如 mofashu 触发）时要显示的额外文本
    [Tooltip("可选：当得分被倍数放大时显示的额外 UI 文本预制体")]
    public GameObject doubleScoreTextPrefab;
    [Tooltip("倍数文本显示持续时间（秒）")]
    public float doubleScoreTextDuration = 1.5f;
    [Tooltip("倍数文本相对图标位置的偏移（Canvas 本地坐标）")]
    public Vector2 doubleScoreTextOffset = new Vector2(0, 20f);

    // 新增：当负分被倍数放大时显示的额外文本
    [Tooltip("可选：当负分被倍数放大时显示的额外 UI 文本预制体")]
    public GameObject multipliedNegativeTextPrefab;
    [Tooltip("负分倍数文本显示持续时间（秒）")]
    public float multipliedNegativeTextDuration = 1.5f;
    [Tooltip("负分倍数文本相对图标位置的偏移（Canvas 本地坐标）")]
    public Vector2 multipliedNegativeTextOffset = new Vector2(0, -20f);

    private Canvas uiCanvas;

    void Start()
    {
        // 自动尝试获取组件以方便使用
        if (animator == null)
            animator = GetComponent<Animator>();
        if (legacyAnimation == null)
            legacyAnimation = GetComponent<Animation>();

        // 尝试找到合适的 Canvas（优先查找父级 Canvas）
        uiCanvas = GetComponentInParent<Canvas>();
        if (uiCanvas == null)
            uiCanvas = FindObjectOfType<Canvas>();
    }

    // 得分后调用（可修改为播放动画等）
    public void OnCleared()
    {
        // 防止重复记分/重复触发
        if (isCleared) return;
        isCleared = true;

        int appliedPoints = 0;
        // 这里处理得分逻辑（将 GameManager 的调用放在 IconTarget）
        if (GameManager.Instance != null)
        {
            // ChangeScore 现在返回实际应用的分数（考虑倍数）
            appliedPoints = GameManager.Instance.ChangeScore(scoreValue, transform.position);
        }
        else
        {
            Debug.LogWarning($"IconTarget.OnCleared: GameManager.Instance is null for {gameObject.name}. scoreValue={scoreValue}");
        }

        // 播放动画：优先 Animator Trigger，然后 legacy Animation
        if (animator != null)
        {
            string trigger = string.IsNullOrEmpty(animatorTrigger) ? "Cleared" : animatorTrigger;
            animator.SetTrigger(trigger);
        }
        else if (legacyAnimation != null)
        {
            // 尝试播放第一个 clip 名称或默认 clip
            if (legacyAnimation.clip != null)
                legacyAnimation.Play(legacyAnimation.clip.name);
            else if (legacyAnimation.GetClipCount() > 0)
            {
                // Play first clip found
                foreach (AnimationState state in legacyAnimation)
                {
                    legacyAnimation.Play(state.name);
                    break;
                }
            }
        }

        // 确保有 Canvas 可用（在某些情况下 Start 未找到）
        if (uiCanvas == null)
            uiCanvas = FindObjectOfType<Canvas>();

        // 新增：在 UI Canvas 中实例化得分文本预制体并放置在图标位置
        if (scoreTextPrefab != null && uiCanvas != null)
        {
            GameObject go = Instantiate(scoreTextPrefab, uiCanvas.transform);
            RectTransform goRt = go.GetComponent<RectTransform>();
            RectTransform canvasRt = uiCanvas.GetComponent<RectTransform>();

            // 将世界坐标转换为 Canvas 本地坐标
            Camera cam = (uiCanvas.renderMode == RenderMode.ScreenSpaceCamera) ? uiCanvas.worldCamera : null;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, transform.position);
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPos, cam, out localPoint);

            if (goRt != null)
            {
                goRt.anchoredPosition = localPoint + scoreTextOffset;
            }

            // 自动销毁
            if (scoreTextDuration > 0f)
                Destroy(go, scoreTextDuration);
        }

        // 如果实际应用的分数与原始分值不同（意味着倍数生效），显示额外文字
        if (appliedPoints != 0 && appliedPoints != scoreValue && uiCanvas != null)
        {
            // 正分被放大
            if (appliedPoints > 0 && doubleScoreTextPrefab != null)
            {
                GameObject go2 = Instantiate(doubleScoreTextPrefab, uiCanvas.transform);
                RectTransform go2Rt = go2.GetComponent<RectTransform>();
                RectTransform canvasRt2 = uiCanvas.GetComponent<RectTransform>();

                Camera cam2 = (uiCanvas.renderMode == RenderMode.ScreenSpaceCamera) ? uiCanvas.worldCamera : null;
                Vector2 screenPos2 = RectTransformUtility.WorldToScreenPoint(cam2, transform.position);
                Vector2 localPoint2;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt2, screenPos2, cam2, out localPoint2);

                if (go2Rt != null)
                {
                    go2Rt.anchoredPosition = localPoint2 + doubleScoreTextOffset;
                }

                if (doubleScoreTextDuration > 0f)
                    Destroy(go2, doubleScoreTextDuration);
            }

            // 负分被放大
            if (appliedPoints < 0 && multipliedNegativeTextPrefab != null)
            {
                GameObject go3 = Instantiate(multipliedNegativeTextPrefab, uiCanvas.transform);
                RectTransform go3Rt = go3.GetComponent<RectTransform>();
                RectTransform canvasRt3 = uiCanvas.GetComponent<RectTransform>();

                Camera cam3 = (uiCanvas.renderMode == RenderMode.ScreenSpaceCamera) ? uiCanvas.worldCamera : null;
                Vector2 screenPos3 = RectTransformUtility.WorldToScreenPoint(cam3, transform.position);
                Vector2 localPoint3;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt3, screenPos3, cam3, out localPoint3);

                if (go3Rt != null)
                {
                    go3Rt.anchoredPosition = localPoint3 + multipliedNegativeTextOffset;
                }

                if (multipliedNegativeTextDuration > 0f)
                    Destroy(go3, multipliedNegativeTextDuration);
            }
        }

        Debug.Log($"IconTarget.OnCleared: {gameObject.name} original={scoreValue} applied={appliedPoints}");

        // 根据设置隐藏对象（如果想在动画后隐藏，可改为在动画事件里隐藏）
        if (hideOnCleared)
            gameObject.SetActive(false);
    }
}
