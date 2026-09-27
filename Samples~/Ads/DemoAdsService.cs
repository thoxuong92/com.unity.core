using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DemoAdsService : IAdsService
{
    public bool CanShowAds()
    {
        return true;
    }
    public bool CanShowInterstitial()
    {
        return true;
    }

    public bool CanShowRewarded()
    {
        return true;
    }

    public void HideBanner()
    {
        Debug.Log("HideBanner");
    }

    public void Initialize()
    {
        Debug.Log("Initialize");
    }

    public void ShowAppOpen(string location, UnityAction callback)
    {
        Debug.Log("ShowAppOpen");
    }

    public void ShowBanner()
    {
        Debug.Log("ShowBanner");
    }

    public void ShowInterstitial(string location, UnityAction callback)
    {
        Debug.Log("ShowInterstitial");
    }

    public void ShowRewarded(string location, UnityAction<bool> callback)
    {
        Debug.Log("ShowRewarded");
    }
}
