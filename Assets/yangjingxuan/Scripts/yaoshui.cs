using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class yaoshui : MonoBehaviour
{
    [Tooltip("是否在下一次减少（分数或时间）或 reveal 时忽略该事件")]
    public bool ignoreNextAdverse = true;

    [Tooltip("点击按钮时立即扣除的分数（正数表示扣分）")]
    public int scoreCost = 4;

    // 点击按钮后调用（在 Button OnClick 中绑定）
    public void OnButtonClick()
    {
        if (ignoreNextAdverse && GameManager.Instance != null)
        {
            GameManager.Instance.SetIgnoreNextAdverseEvent();
            Debug.Log("yaoshui: requested ignore for next adverse event.");
        }

        // 立即扣除分数（不受刚设置的 ignore 影响，因为 ignore 只在下一帧生效）
        if (scoreCost > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.ChangeScore(-scoreCost);
            Debug.Log("yaoshui: deducted score " + scoreCost);
        }
        else if (GameManager.Instance == null)
        {
            Debug.LogWarning("yaoshui: GameManager.Instance 未找到，无法设置 ignore 标志或扣分。");
        }
    }
}
