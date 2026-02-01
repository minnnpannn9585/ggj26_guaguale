using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameUi : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public Image timerBar;

    
    void Update()
    {
        scoreText.text = "分数：$" + GameManager.Instance.Score.ToString();
        timerText.text = "倒计时：" + Mathf.CeilToInt(GameManager.Instance.timer).ToString() + "s";
        timerBar.fillAmount = GameManager.Instance.timer / 59f;
    }
}
