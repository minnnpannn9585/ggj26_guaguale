using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextCardBtn : MonoBehaviour
{
    public GameObject[] cards;

    private int currentIndex = 0;

    public eraser era;
    public RawImage[] raws;
    
    public void ShowNextCard()
    {
        cards[currentIndex].GetComponent<Animator>().SetTrigger("finish");
        Invoke("ActivateNextCard", 1.0f);
    }

    public void ActivateNextCard()
    {
        cards[currentIndex].SetActive(false);
        currentIndex += 1;
        if (currentIndex < cards.Length)
        {
            cards[currentIndex].SetActive(true);
            // Use the eraser's API to properly initialize the new top image
            if (era != null)
            {
                era.SetTopImage(raws[currentIndex]);
            }
            else
            {
                // fallback: assign directly (not recommended)
                era.topImage = raws[currentIndex];
            }
        }
        else
        {
            //game end
        }
    }
}
