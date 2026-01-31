using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance {  get; private set; }

    public AudioClip AddFoodSound;
    public AudioClip BoilingSound;
    public AudioClip BoomSound;
    public AudioClip NextPaper;
    public AudioClip RedMushroom;
    public AudioClip ScrapeSound;
    public AudioClip ShitSound;
    public AudioClip GetScore;
    public AudioClip GetTime;
    public AudioClip LoseTime;
    public AudioClip LoseScore;
    public AudioClip Shouting;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private Vector3 DefaultPos => transform.position;

    public void PlayAddFoodSound() => PlayAddFoodSound(DefaultPos);
    public void PlayBoilingSound() => PlayBoilingSound(DefaultPos);
    public void PlayBoomSound() => PlayBoomSound(DefaultPos);
    public void PlayNextPaper() => PlayNextPaper(DefaultPos);
    public void PlayRedMushroom() => PlayRedMushroom(DefaultPos);
    public void PlayScrapeSound() => PlayScrapeSound(DefaultPos);
    public void PlayShitSound() => PlayShitSound(DefaultPos);
    public void PlayGetScore() => PlayGetScore(DefaultPos);
    public void PlayGetTime() => PlayGetTime(DefaultPos);
    public void PlayLoseTime() => PlayLoseTime(DefaultPos);
    public void PlayLoseScore() => PlayLoseScore(DefaultPos);

    public void PlayShouting() => PlayShouting(DefaultPos);

    public void PlayAddFoodSound(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(AddFoodSound, position, 1f);
    }
    public void PlayBoilingSound(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(BoilingSound, position, 1f);
    }
    public void PlayBoomSound(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(BoomSound, position, 1f);
    }
    public void PlayNextPaper(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(NextPaper, position, 1f);
    }
    public void PlayRedMushroom(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(RedMushroom, position, 1f);
    }
    public void PlayScrapeSound(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(ScrapeSound, position, 1f);
    }
    public void PlayShitSound(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(ShitSound, position, 1f);
    }
    public void PlayGetScore(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(GetScore, position, 1f);
    }
    public void PlayGetTime(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(GetTime, position, 1f);
    }
    public void PlayLoseTime(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(LoseTime, position, 1f);
    }
    public void PlayLoseScore(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(LoseScore, position, 1f);
    }
    public void PlayShouting(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(Shouting, position, 1f);
    }
}
