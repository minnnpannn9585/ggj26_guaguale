using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomExploEnd : MonoBehaviour
{
    public void BoomEnd()
    {
        GameManager.Instance.EndGame();
    }
}
