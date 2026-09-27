using UnityEngine;
using Unity.Core.Services;
using Unity.Core.Services.Ads;

public class TestAds : MonoBehaviour
{
    void Start()
    {
        AdsService.Register(new DemoAdsService());
    }

    [ContextMenu("ShowAppOpen")]
    void ShowAOA()
    {
        AdsService.ShowAppOpen("location", () => Debug.Log("Callback"));
    }
}
