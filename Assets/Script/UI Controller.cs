using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TMP_Text healthText;
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject LevelUpPanel;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Slider playerExperienceSlider;
    [SerializeField] private TMP_Text experienceText;

    public LevelUpButton[] levelUpButtons; // ��Ǩ�ͺ��� LevelUpButton �繤��ʷ��س�������ҹ���ԧ

    // --- ��ǹ�������Ѻ Rewarded Ads UI ---
    [Header("Rewarded Ads UI")]
    [SerializeField] private Button rewardedAdButton; // ��ͧ����Ѻ�ҡ���� "Watch Ad"
    [SerializeField] private TMP_Text adsRemainingText; // ��ͧ����Ѻ�ҡ Text �ʴ� "Ads Left: X/5"
    public GameObject adLimitMessagePanel; // ��ͧ����Ѻ�ҡ Panel/Text �������͹����Ͷ֧�մ�ӡѴ
    // ------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void UpdateHealthSlider()
    {
        playerHealthSlider.maxValue = PlayerController.Instance.playerMaxHealth;
        playerHealthSlider.value = PlayerController.Instance.playerHealth;
        healthText.text = playerHealthSlider.value + " / " + playerHealthSlider.maxValue;
    }
    public void UpdateExperienceSlider()
    {
        playerExperienceSlider.maxValue = PlayerController.Instance.playerLevels
            [PlayerController.Instance.currentLevel - 1]; // ��Ǩ�ͺ currentLevel �������� 0 ���͵Դź
        playerExperienceSlider.value = PlayerController.Instance.experience;
        experienceText.text = playerExperienceSlider.value + " / " + playerExperienceSlider.maxValue;
    }

    public void UpdateTimer(float timer)
    {
        float min = Mathf.FloorToInt(timer / 60f);
        float sec = Mathf.FloorToInt(timer % 60f);

        timerText.text = min + ":" + sec.ToString("00");
    }

    public void LevelUpPanelOpen()
    {
        LevelUpPanel.SetActive(true);
        Time.timeScale = 0f; // ��ش������� Level Up Panel �Դ
    }

    public void LevelUpPanelClose()
    {
        LevelUpPanel.SetActive(false);
        Time.timeScale = 1f; // ��Ѻ������������ Level Up Panel �Դ
    }

    // --- ���ʹ����Ѻ Rewarded Ads UI ---

    // �ѻവ��ͤ����ʴ��ӹǹ�ɳҷ�������
    public void UpdateAdsRemainingText(int currentWatched, int maxAllowed)
    {
        if (adsRemainingText != null)
        {
            adsRemainingText.text = $"Ads Left: {maxAllowed - currentWatched}/{maxAllowed}";
        }
        else
        {
            Debug.LogWarning("adsRemainingText is not assigned in UIController!");
        }
    }

    // �ʴ���ͤ�������Ͷ֧�մ�ӡѴ
    public void ShowAdLimitMessage()
    {
        if (adLimitMessagePanel != null)
        {
            adLimitMessagePanel.SetActive(true);
            // �� Coroutine ��������ͤ��������ͧ��ѧ�ҡ������Թҷ�
            StartCoroutine(HideAdLimitMessageAfterDelay(3f));
        }
        else
        {
            Debug.LogWarning("adLimitMessagePanel is not assigned in UIController!");
        }
    }

    private IEnumerator HideAdLimitMessageAfterDelay(float delay)
    {
        // �� WaitForSecondsRealtime ������� Coroutine �ӧҹ��� Time.timeScale �� 0
        yield return new WaitForSecondsRealtime(delay);
        if (adLimitMessagePanel != null)
        {
            adLimitMessagePanel.SetActive(false);
        }
    }

    // �Ǻ���ʶҹл����ɳ� (����/�����)
    public void SetRewardedAdButtonState(bool isActive)
    {
        if (rewardedAdButton != null)
        {
            rewardedAdButton.interactable = isActive; // ���������������������������ö���ɳ���
            // Optional: �Ҩ������¹�� ���͢�ͤ�������������
        }
        else
        {
            Debug.LogWarning("rewardedAdButton is not assigned in UIController!");
        }
    }
    // ------------------------------------------
}