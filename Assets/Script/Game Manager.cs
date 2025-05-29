using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public float gameTime;
    public bool gameActive;

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

    void Start()
    {
        gameActive = true;
    }

    void Update()
    {
        if (gameActive)
        {
            gameTime += Time.deltaTime;
            UIController.Instance.UpdateTimer(gameTime);

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                Pause();
            }
        }
    }

    public void GameOver()
    {
        Debug.Log("GameManager: GameOver method called.");
        gameActive = false; // Stop game timer
        Time.timeScale = 0f; // Pause all game activity

        // Show Game Over panel immediately
        UIController.Instance.gameOverPanel.SetActive(true);

        if (PlayerController.Instance != null)
        {
            
            Debug.Log("GameManager: PlayerController GameObject SHOULD remain Active for Ad Events.");
        }
        
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UpdateAdUIOnGameOver();
        }
    }

    // ���ʹ���ж١���¡�ҡ PlayerController ��ѧ�ҡ���ɳ������������Ѻ�ҧ���
    public void RevivePlayer()
    {
        Debug.Log("RevivePlayer method called.");
        UIController.Instance.gameOverPanel.SetActive(false); // �Դ˹�Ҩ� Game Over

        if (PlayerController.Instance != null)
        {
            Debug.Log("GameManager: PlayerController GameObject assumed to be Active for Revive.");
        }

        gameActive = true; // Resume game timer
        Time.timeScale = 1f; // Resume normal game speed

        // Clear enemies
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            Destroy(enemy.gameObject);
        }

        Debug.Log("Player has been revived and game resumed.");
    }

    public void Restart()
    {
        Time.timeScale = 1f; // ��Ǩ�ͺ��������� Time.timeScale �� 1 ��͹��Ŵ Scene
        SceneManager.LoadScene("Game"); // ��Ŵ Scene ������
    }

    public void Pause()
    {
        if (UIController.Instance.pausePanel.activeSelf == false &&
            UIController.Instance.gameOverPanel.activeSelf == false &&
            UIController.Instance.LevelUpPanel.activeSelf == false) // ������ LevelUpPanel
        {
            UIController.Instance.pausePanel.SetActive(true);
            Time.timeScale = 0f;
            // AudioController.Instance.PlaySound(AudioController.Instance.pause); // ����� AudioController
        }
        else if (UIController.Instance.pausePanel.activeSelf == true) // ��੾������� Pause Panel �Դ����
        {
            UIController.Instance.pausePanel.SetActive(false);
            Time.timeScale = 1f;
            // AudioController.Instance.PlaySound(AudioController.Instance.unpause); // ����� AudioController
        }
    }

    public void QuitGame()
    {
        Application.Quit(); // �͡�ҡ��
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // ��Ǩ�ͺ��������� Time.timeScale �� 1 ��͹��Ŵ Scene
        SceneManager.LoadScene("Main Menu"); // ��Ѻ��ѧ������ѡ
    }
}