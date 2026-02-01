using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mofashu : MonoBehaviour
{
    [Tooltip("点击时立即扣除的分数（正数表示扣分）")]
    public int scoreCost = 7;

    [Tooltip("正分倍数，默认双倍")]
    public float positiveMultiplier = 2f;

    [Tooltip("倍数持续时间（秒）")]
    public float multiplierDuration = 10f;

    // 点击一次后标记，防止重复点击
    private bool clicked = false;

    // 绑定到按钮的点击事件
    public void OnButtonClick()
    {
        if (clicked) return;
        clicked = true;

        // 如果挂在 Button 上，禁用交互
        var btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = false;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("mofashu: GameManager.Instance 未找到，无法执行操作。");
            return;
        }

        // 立即扣除分数（不受随即设置的 multiplier 影响）
        if (scoreCost > 0)
        {
            // pass the button world position so UI feedback can be placed near the source
            GameManager.Instance.ChangeScore(-scoreCost, transform.position);
            Debug.Log($"mofashu: deducted {scoreCost} points immediately.");
        }

        // 应用正分倍数（只影响之后的正分）
        if (positiveMultiplier > 1f && multiplierDuration > 0f)
        {
            GameManager.Instance.ApplyTemporaryPositiveScoreMultiplier(positiveMultiplier, multiplierDuration);
            Debug.Log($"mofashu: applied positive multiplier x{positiveMultiplier} for {multiplierDuration} seconds.");
        }

        // 新增：对减分也进行倍数（只影响之后的负分惩罚）
        if (positiveMultiplier > 1f && multiplierDuration > 0f)
        {
            GameManager.Instance.ApplyTemporaryNegativeScoreMultiplier(positiveMultiplier, multiplierDuration);
            Debug.Log($"mofashu: applied negative multiplier x{positiveMultiplier} for {multiplierDuration} seconds.");
        }
    }
}
