using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTutBtn : MonoBehaviour
{
    public GameObject startCan;

    public void ExitTut()
    {
        startCan.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
