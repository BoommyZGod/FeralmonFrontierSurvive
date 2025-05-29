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

    public LevelUpButton[] levelUpButtons; // ตรวจสอบว่า LevelUpButton เป็นคลาสที่คุณมีและใช้งานได้จริง

    // --- ส่วนนี้สำหรับ Rewarded Ads UI ---
    [Header("Rewarded Ads UI")]
    [SerializeField] private Button rewardedAdButton; // ช่องสำหรับลากปุ่ม "Watch Ad"
    [SerializeField] private TMP_Text adsRemainingText; // ช่องสำหรับลาก Text แสดง "Ads Left: X/5"
    public GameObject adLimitMessagePanel; // ช่องสำหรับลาก Panel/Text ที่แจ้งเตือนเมื่อถึงขีดจำกัด
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
            [PlayerController.Instance.currentLevel - 1]; // ตรวจสอบ currentLevel ไม่ให้เป็น 0 หรือติดลบ
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
        Time.timeScale = 0f; // หยุดเกมเมื่อ Level Up Panel เปิด
    }

    public void LevelUpPanelClose()
    {
        LevelUpPanel.SetActive(false);
        Time.timeScale = 1f; // กลับมาเล่นเกมเมื่อ Level Up Panel ปิด
    }

    // --- เมธอดสำหรับ Rewarded Ads UI ---

    // อัปเดตข้อความแสดงจำนวนโฆษณาที่เหลือ
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

    // แสดงข้อความเมื่อถึงขีดจำกัด
    public void ShowAdLimitMessage()
    {
        if (adLimitMessagePanel != null)
        {
            adLimitMessagePanel.SetActive(true);
            // ใช้ Coroutine เพื่อให้ข้อความหายไปเองหลังจากไม่กี่วินาที
            StartCoroutine(HideAdLimitMessageAfterDelay(3f));
        }
        else
        {
            Debug.LogWarning("adLimitMessagePanel is not assigned in UIController!");
        }
    }

    private IEnumerator HideAdLimitMessageAfterDelay(float delay)
    {
        // ใช้ WaitForSecondsRealtime เพื่อให้ Coroutine ทำงานแม้ Time.timeScale เป็น 0
        yield return new WaitForSecondsRealtime(delay);
        if (adLimitMessagePanel != null)
        {
            adLimitMessagePanel.SetActive(false);
        }
    }

    // ควบคุมสถานะปุ่มโฆษณา (กดได้/ไม่ได้)
    public void SetRewardedAdButtonState(bool isActive)
    {
        if (rewardedAdButton != null)
        {
            rewardedAdButton.interactable = isActive; // ทำให้ปุ่มกดไม่ได้เมื่อไม่สามารถดูโฆษณาได้
            // Optional: อาจจะเปลี่ยนสี หรือข้อความบนปุ่มด้วย
        }
        else
        {
            Debug.LogWarning("rewardedAdButton is not assigned in UIController!");
        }
    }
    // ------------------------------------------
}