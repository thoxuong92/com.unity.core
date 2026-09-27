using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoading : MonoBehaviour
{
    public TMPro.TMP_Text txtPercent;
    public void OnPercentLoading(float percent)
    {
        txtPercent.text = "LOADING " + (percent * 100).ToString("0") + "%...";
    }
    public void OnCompleted()
    {
        Debug.Log("On Loading complete!");
    }
}
