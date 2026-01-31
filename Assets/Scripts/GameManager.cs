using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public float timer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Debug.Log("Time's up!");
            // Handle end of game logic here
        }
    }

    public void ChangeScore(int points)
    {
        Score += points;
        Debug.Log("Score: " + Score);
    }

    public void ChangeTimer(float time)
    {
        timer += time;
        if(timer >= 99)
        {
            timer = 99;
        }
    }


}
