using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextCardBtn : MonoBehaviour
{
    public GameObject[] cards;

    private int currentIndex = 0;

    public void ShowNextCard()
    {
        cards[currentIndex].SetActive(false);
        currentIndex += 1;
        if (currentIndex >= cards.Length)
        {
            Debug.Log("No more cards to show.");
        }
        else
        {
            cards[currentIndex].SetActive(true);
        }
    }
}
