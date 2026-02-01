using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Make this GameObject follow the mouse pointer.
/// - If followUI is true and uiTarget is set, the script moves a UI RectTransform in screen/UI space.
/// - Otherwise it moves a world-space transform using ScreenToWorldPoint and keeps z = 0.
/// Fix: always convert screen point to the uiTarget's parent local space and set anchoredPosition.
/// This avoids pivot/anchor/parent-scale offset problems for ScreenSpace-Overlay/Camera/World canvases.
/// </summary>
public class ObjectFollowMouse : MonoBehaviour
{
    [Tooltip("If null, Camera.main will be used for world-space conversion.")]
    public Camera targetCamera;

    [Header("World follow (keeps Z = 0)")]
    public bool smooth = false;
    public float smoothSpeed = 15f;

    [Header("UI follow")]
    [Tooltip("Enable to make a UI RectTransform follow the mouse instead of a world object")]
    public bool followUI = false;
    [Tooltip("UI element (RectTransform) to move. If null, this GameObject's RectTransform will be used.")]
    public RectTransform uiTarget;
    [Tooltip("Canvas that contains the UI element. If null, the script will try to find a parent Canvas.")]
    public Canvas uiCanvas;
    [Tooltip("Optional offset in anchored pixels")]
    public Vector2 uiOffset = Vector2.zero;
    [Tooltip("If true, clamp the UI element inside the parent rect")]
    public bool clampToParent = true;
    [Tooltip("Smooth UI movement when true")]
    public bool smoothUI = false;
    [Tooltip("Smoothing speed for UI follow")]
    public float smoothUISpeed = 20f;

    // Global freeze flag. When true ObjectFollowMouse will stop updating the target position,
    // effectively freezing the visible pointer/mouse-follow object in place.
    public static bool freezeInput = false;

    RectTransform _canvasRect;
    RectTransform _parentRect;

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (followUI)
        {
            if (uiTarget == null)
                uiTarget = GetComponent<RectTransform>();

            if (uiTarget != null)
            {
                _parentRect = uiTarget.parent as RectTransform;
            }

            if (uiCanvas == null && uiTarget != null)
                uiCanvas = uiTarget.GetComponentInParent<Canvas>();

            if (uiCanvas != null)
                _canvasRect = uiCanvas.transform as RectTransform;
        }
    }

    void Update()
    {
        // Respect global freeze flag: do not update position while frozen
        if (freezeInput) return;

        if (followUI && uiTarget != null && uiCanvas != null)
            FollowMouseUI();
        else
            FollowMouseWorld();
    }

    void FollowMouseWorld()
    {
        if (targetCamera == null) return;

        Vector3 mouseScreen = Input.mousePosition;
        float zDistance = Mathf.Abs(targetCamera.transform.position.z - 0f);
        mouseScreen.z = zDistance;

        Vector3 worldPos = targetCamera.ScreenToWorldPoint(mouseScreen);
        worldPos.z = 0f;

        if (smooth)
            transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * smoothSpeed);
        else
            transform.position = worldPos;
    }

    void FollowMouseUI()
    {
        // parentRect is the coordinate space we map the screen point into.
        RectTransform parentRect = _parentRect != null ? _parentRect : _canvasRect;
        if (parentRect == null) return;

        // choose camera for ScreenPoint conversion depending on canvas render mode
        Camera cam = (uiCanvas.renderMode == RenderMode.ScreenSpaceCamera || uiCanvas.renderMode == RenderMode.WorldSpace)
                     ? uiCanvas.worldCamera
                     : null;

        // Convert screen point to local point in parent's local space.
        // For ScreenSpace-Overlay pass cam = null; for ScreenSpace-Camera/WorldSpace pass uiCanvas.worldCamera.
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, Input.mousePosition, cam, out localPoint))
            return;

        // localPoint is in parent's local coordinates. Use anchoredPosition to respect anchors/pivots.
        Vector2 targetAnchored = localPoint + uiOffset;

        // Optional clamping inside parent rect
        if (clampToParent)
        {
            Rect rect = parentRect.rect;
            float minX = rect.xMin;
            float maxX = rect.xMax;
            float minY = rect.yMin;
            float maxY = rect.yMax;
            targetAnchored.x = Mathf.Clamp(targetAnchored.x, minX, maxX);
            targetAnchored.y = Mathf.Clamp(targetAnchored.y, minY, maxY);
        }

        if (smoothUI)
            uiTarget.anchoredPosition = Vector2.Lerp(uiTarget.anchoredPosition, targetAnchored, Time.deltaTime * smoothUISpeed);
        else
            uiTarget.anchoredPosition = targetAnchored;
    }
}
