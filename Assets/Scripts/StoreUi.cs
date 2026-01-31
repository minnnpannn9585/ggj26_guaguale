using System.Collections;
using UnityEngine;

public class StoreUi : MonoBehaviour
{
    [SerializeField] private RectTransform panel;        // the panel that will move
    [SerializeField] private float moveDistance = 500f;  // distance in pixels to move right
    [SerializeField] private float moveSpeed = 1000f;    // pixels per second

    private Vector2 closedPos;
    private Vector2 openedPos;
    private bool isOpen = false;
    private Coroutine moveCoroutine;

    void Start()
    {
        // If no panel assigned, try to use parent RectTransform (common when script is on the button)
        if (panel == null)
        {
            panel = transform.parent as RectTransform ?? GetComponent<RectTransform>();
        }

        if (panel == null)
        {
            Debug.LogError("StoreUi: No RectTransform assigned or found on parent.");
            enabled = false;
            return;
        }

        closedPos = panel.anchoredPosition;
        openedPos = closedPos + new Vector2(moveDistance, 0f);
    }

    // Call this from the Button OnClick() (or wire it in code)
    public void TogglePanel()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        isOpen = !isOpen;
        Vector2 target = isOpen ? openedPos : closedPos;
        moveCoroutine = StartCoroutine(MovePanelTo(target));
    }

    private IEnumerator MovePanelTo(Vector2 target)
    {
        while (Vector2.Distance(panel.anchoredPosition, target) > 0.01f)
        {
            panel.anchoredPosition = Vector2.MoveTowards(panel.anchoredPosition, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        panel.anchoredPosition = target;
        moveCoroutine = null;
    }
}
