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
        scoreText.text = "Score: " + GameManager.Instance.Score.ToString();
        timerText.text = "Time: " + Mathf.CeilToInt(GameManager.Instance.timer).ToString();
        timerBar.fillAmount = GameManager.Instance.timer / 59f;
    }
}
