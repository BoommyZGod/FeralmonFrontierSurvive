using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener // เพิ่ม IUnityAdsLoadListener
{
    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField] bool _testMode = true;
    private string _gameId;

    // สำหรับ Rewarded Ad
    [SerializeField] string _androidRewardedAdUnitId = "Rewarded Android"; // ตรวจสอบชื่อนี้ใน Unity Dashboard
    [SerializeField] string _iOSRewardedAdUnitId = "Rewarded iOS";       // ตรวจสอบชื่อนี้ใน Unity Dashboard
    private string _rewardedAdUnitId; // <--- ตัวแปรนี้ต้องถูกประกาศ

    public static event Action OnRewardedAdCompleted;

    void Awake()
    {
        // *** แก้ไข/เพิ่มส่วนนี้: กำหนดค่าให้กับ _rewardedAdUnitId ตามแพลตฟอร์ม ***
#if UNITY_IOS
        _gameId = _iOSGameId;
        _rewardedAdUnitId = _iOSRewardedAdUnitId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
        _rewardedAdUnitId = _androidRewardedAdUnitId;
#elif UNITY_EDITOR
        _gameId = _androidGameId; // For testing in Editor
        _rewardedAdUnitId = _androidRewardedAdUnitId; // For testing in Editor
#endif

        InitializeAds();
    }

    public void InitializeAds()
    {
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        LoadRewardedAd(); // โหลดโฆษณาเมื่อเริ่มต้นเสร็จ
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    // ===============================================
    // Rewarded Ad Methods
    // ===============================================

    public void LoadRewardedAd() // *** เมธอดสำหรับโหลดโฆษณา ***
    {
        Debug.Log("Loading Ad: " + _rewardedAdUnitId);
        Advertisement.Load(_rewardedAdUnitId, this); // 'this' หมายถึง class นี้ implements IUnityAdsLoadListener
    }

    public void ShowRewardedAd() // *** เมธอดสำหรับแสดงโฆษณา (จะถูกเรียกจาก PlayerController) ***
    {
        // ไม่ต้องเช็ค Advertisement.IsReady() ตรงนี้แล้ว
        // เราจะเช็คความพร้อมโดยการโหลดก่อน
        Debug.Log("Trying to show Ad: " + _rewardedAdUnitId);
        Advertisement.Show(_rewardedAdUnitId, this); // 'this' หมายถึง class นี้ implements IUnityAdsShowListener
    }

    // Implement IUnityAdsLoadListener interface
    public void OnUnityAdsAdLoaded(string adUnitId) // *** Callback เมื่อโฆษณาโหลดเสร็จและพร้อมแสดง ***
    {
        Debug.Log("Ad Loaded: " + adUnitId);
        // ตอนนี้โฆษณาพร้อมแล้ว คุณสามารถเปิดปุ่มให้ผู้เล่นกดได้
        // หรือถ้าคุณต้องการให้โหลดอัตโนมัติและแสดงทันทีเมื่อพร้อม
        // ในกรณีนี้ เราจะรอให้ PlayerController เรียก ShowRewardedAd()
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // อาจจะแจ้ง UI ว่าโฆษณาไม่พร้อม
    }

    // Implement IUnityAdsShowListener interface
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        Time.timeScale = 1f; // ตรวจสอบให้แน่ใจว่าเกมกลับมาเดินปกติ
        // แจ้งผู้เล่นว่าโฆษณาแสดงไม่ได้
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Debug.Log("Ad Show Start: " + adUnitId);
        Time.timeScale = 0f; // หยุดเกมเมื่อโฆษณาเริ่มแสดง
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        Debug.Log("Ad Show Clicked: " + adUnitId);
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Ad Show Complete: {adUnitId} - {showCompletionState}");

        Time.timeScale = 1f; // กลับมาเล่นเกมต่อ

        if (adUnitId.Equals(_rewardedAdUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            OnRewardedAdCompleted?.Invoke(); // เรียก Event เพื่อให้ PlayerController รับรางวัล
            LoadRewardedAd(); // โหลดโฆษณาใหม่ทันที เพื่อให้พร้อมใช้งานครั้งต่อไป
        }
        else if (showCompletionState.Equals(UnityAdsShowCompletionState.SKIPPED))
        {
            Debug.Log("Unity Ads Rewarded Ad Skipped");
        }
        else if (showCompletionState.Equals(UnityAdsShowCompletionState.UNKNOWN))
        {
            Debug.Log("Unity Ads Rewarded Ad Unknown");
        }
    }
}