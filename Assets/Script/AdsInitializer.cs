using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener // ���� IUnityAdsLoadListener
{
    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField] bool _testMode = true;
    private string _gameId;

    // ����Ѻ Rewarded Ad
    [SerializeField] string _androidRewardedAdUnitId = "Rewarded Android"; // ��Ǩ�ͺ���͹��� Unity Dashboard
    [SerializeField] string _iOSRewardedAdUnitId = "Rewarded iOS";       // ��Ǩ�ͺ���͹��� Unity Dashboard
    private string _rewardedAdUnitId; // <--- ����ù���ͧ�١��С��

    public static event Action OnRewardedAdCompleted;

    void Awake()
    {
        // *** ���/������ǹ���: ��˹�������Ѻ _rewardedAdUnitId ����ŵ����� ***
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
        LoadRewardedAd(); // ��Ŵ�ɳ�����������������
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    // ===============================================
    // Rewarded Ad Methods
    // ===============================================

    public void LoadRewardedAd() // *** ���ʹ����Ѻ��Ŵ�ɳ� ***
    {
        Debug.Log("Loading Ad: " + _rewardedAdUnitId);
        Advertisement.Load(_rewardedAdUnitId, this); // 'this' ���¶֧ class ��� implements IUnityAdsLoadListener
    }

    public void ShowRewardedAd() // *** ���ʹ����Ѻ�ʴ��ɳ� (�ж١���¡�ҡ PlayerController) ***
    {
        // ����ͧ�� Advertisement.IsReady() �ç�������
        // ��Ҩ��礤���������¡����Ŵ��͹
        Debug.Log("Trying to show Ad: " + _rewardedAdUnitId);
        Advertisement.Show(_rewardedAdUnitId, this); // 'this' ���¶֧ class ��� implements IUnityAdsShowListener
    }

    // Implement IUnityAdsLoadListener interface
    public void OnUnityAdsAdLoaded(string adUnitId) // *** Callback ������ɳ���Ŵ������о�����ʴ� ***
    {
        Debug.Log("Ad Loaded: " + adUnitId);
        // �͹����ɳҾ�������� �س����ö�Դ�����������蹡���
        // ���Ͷ�Ҥس��ͧ��������Ŵ�ѵ��ѵ�����ʴ��ѹ������;����
        // 㹡óչ�� ��Ҩ������ PlayerController ���¡ ShowRewardedAd()
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // �Ҩ���� UI ����ɳ��������
    }

    // Implement IUnityAdsShowListener interface
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        Time.timeScale = 1f; // ��Ǩ�ͺ�������������Ѻ���Թ����
        // �駼���������ɳ��ʴ������
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Debug.Log("Ad Show Start: " + adUnitId);
        Time.timeScale = 0f; // ��ش��������ɳ�������ʴ�
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        Debug.Log("Ad Show Clicked: " + adUnitId);
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Ad Show Complete: {adUnitId} - {showCompletionState}");

        Time.timeScale = 1f; // ��Ѻ����������

        if (adUnitId.Equals(_rewardedAdUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            OnRewardedAdCompleted?.Invoke(); // ���¡ Event ������� PlayerController �Ѻ�ҧ���
            LoadRewardedAd(); // ��Ŵ�ɳ�����ѹ�� �������������ҹ���駵���
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