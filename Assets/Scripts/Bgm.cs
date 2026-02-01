using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Bgm : MonoBehaviour
{
    public static Bgm Instance { get; private set; }

    void Awake()
    {
        // If there's no instance yet, become the singleton and persist
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure audio starts if there's an AudioSource with a clip
            var src = GetComponent<AudioSource>();
            if (src != null && src.clip != null && !src.isPlaying)
                src.Play();
        }
        else if (Instance != this)
        {
            // A singleton already exists -> destroy duplicate immediately
            Destroy(gameObject);
        }
    }
}
