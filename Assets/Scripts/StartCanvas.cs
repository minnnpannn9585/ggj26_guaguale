using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCanvas : MonoBehaviour
{
    public GameObject tut;
    public GameObject dialogueCanvas;
    public void StartDialogue()
    {
        dialogueCanvas.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void Tut()
    {
        tut.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void QuitGameBtn()
    {
        Application.Quit();
    }
}
