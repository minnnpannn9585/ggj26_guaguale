using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainBtn : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        // Restore time scale in case the game was paused via Time.timeScale = 0
        Time.timeScale = 1f;

        // Make sure the OS cursor is visible when returning to main menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Destroy persistent GameManager so a fresh instance with proper scene references will be created.
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        // Optionally destroy other singletons if you want them recreated on the next scene.
        // Example: if you don't want Bgm to persist between runs, uncomment:
        // if (Bgm.Instance != null) Destroy(Bgm.Instance.gameObject);

        SceneManager.LoadScene(0);
    }
}
