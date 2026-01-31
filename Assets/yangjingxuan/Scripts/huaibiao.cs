using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class huaibiao : MonoBehaviour
{
    [Tooltip("分数要减少的值，按钮点击时会从当前分数中减去这个值")] 
    public int scoreDecrease = 5;

    [Tooltip("点击按钮后增加的时间（秒）")]
    public float timeIncrease = 10f;

    // 这个方法可以在 Unity 编辑器的 Button OnClick() 中绑定
    public void OnButtonClick()
    {
        if (GameManager.Instance != null)
        {
            // 减少分数（ChangeScore 接受正数增加，负数减少）
            GameManager.Instance.ChangeScore(-scoreDecrease);
            // 增加时间
            GameManager.Instance.ChangeTimer(timeIncrease);
        }
        else
        {
            Debug.LogWarning("huaibiao: GameManager.Instance 未找到，无法修改分数或时间。");
        }
    }
}
