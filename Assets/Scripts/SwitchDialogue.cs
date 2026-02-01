using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchDialogue : MonoBehaviour
{
    public GameObject[] dialogues;
    int index = 0;
    
    public void NextDia()
    {
        dialogues[index].SetActive(false);
        index++;
        if(index < dialogues.Length)
        {
            dialogues[index].SetActive(true);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }
}
