using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class ADSManager : MonoBehaviour
{
    private string bannerID = "ca-app-pub-8743537631045593/6002772779";
    private string interstialID = "ca-app-pub-8743537631045593/1134385633";
    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    public static ADSManager Instance;
    private void Awake()
    {
        Instance = this;
        RequestBanner();
        RequestInterstitial();
    }

    private void RequestBanner()
    {
        // Create a 320x50 banner at the top of the screen.
        this.bannerView = new BannerView(bannerID, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();
        this.bannerView.LoadAd(request);

    }
    private void RequestInterstitial()
    {
        this.interstitialAd = new InterstitialAd(interstialID);
        AdRequest request = new AdRequest.Builder().Build();
        this.interstitialAd.LoadAd(request);
    }
    public void ShowInterstitial()
    {
        this.interstitialAd.Show();
        RequestInterstitial();
    }
}
