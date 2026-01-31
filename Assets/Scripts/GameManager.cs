using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public float timer;

    // If true, the next adverse event (score decrease, timer decrease, or reveal) will be ignored.
    // The ignore only applies if it was set in a previous frame so that the button that sets it
    // doesn't have its own immediate operations suppressed.
    private bool ignoreNextAdverse = false;
    private int ignoreSetFrame = -1;

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

    // Call this to request that the next adverse event (score/time decrease or reveal) be ignored.
    public void SetIgnoreNextAdverseEvent()
    {
        ignoreNextAdverse = true;
        ignoreSetFrame = Time.frameCount;
        Debug.Log("GameManager: ignoreNextAdverse set at frame " + ignoreSetFrame);
    }

    // If an adverse event occurs (score decrease / timer decrease / reveal), callers should call this
    // to determine whether the event should be ignored. It returns true if the ignore was consumed.
    public bool ConsumeIgnoreIfEligible()
    {
        if (!ignoreNextAdverse) return false;
        // only allow consume when we're in a later frame than when the ignore was set
        if (Time.frameCount <= ignoreSetFrame) return false;
        ignoreNextAdverse = false;
        Debug.Log("GameManager: consumed ignoreNextAdverse at frame " + Time.frameCount);
        return true;
    }

    public void ChangeScore(int points)
    {
        // If this is a decrease and the ignore is eligible, consume and skip applying it
        if (points < 0 && ConsumeIgnoreIfEligible())
        {
            Debug.Log("GameManager: ignored score decrease of " + points);
            return;
        }

        Score += points;
        Debug.Log("Score: " + Score);
    }

    public void ChangeTimer(float time)
    {
        // If this is a decrease and the ignore is eligible, consume and skip applying it
        if (time < 0f && ConsumeIgnoreIfEligible())
        {
            Debug.Log("GameManager: ignored timer decrease of " + time);
            return;
        }

        timer += time;
        if(timer >= 99)
        {
            timer = 99;
        }
    }


}
