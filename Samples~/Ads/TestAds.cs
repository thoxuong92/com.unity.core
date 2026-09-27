using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAds : MonoBehaviour
{
    void Start()
    {
        W_Ads.Register(new DemoAdsService());
    }

    [ContextMenu("ShowAppOpen")]
    void ShowAOA()
    {
        API.Get<W_Ads>().ShowAppOpen("location", () => Debug.Log("Callback"));
    }
}
