using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanelManager : MonoBehaviour
{
    [Header("UI 패널")]
    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject escPanel;
    [SerializeField] private GameObject pauseTitle;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject tutorialsPanel;

    [Header("일시중지 제외 씬")]
    [SerializeField] private string exemptSceneName = "MainScene";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleESC();
        }
    }

    private void HandleESC()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (!escPanel.activeSelf)
        {
            // ESC_Panel 비활성화 상태 → Main_UI 끄고 ESC_Panel 켬
            mainUI.SetActive(false);
            escPanel.SetActive(true);

            // 제외 씬이 아니면 게임 일시중지
            if (currentScene != exemptSceneName)
                Time.timeScale = 0f;
        }
        else if (pauseTitle.activeSelf)
        {
            // Pause_Title 켜져있으면 → ESC_Panel 끄고 Main_UI 켬
            escPanel.SetActive(false);
            mainUI.SetActive(true);

            // 제외 씬이 아니면 게임 재개
            if (currentScene != exemptSceneName)
                Time.timeScale = 1f;
        }
        else if (optionsPanel.activeSelf)
        {
            optionsPanel.SetActive(false);
            pauseTitle.SetActive(true);
        }
        else if (controlsPanel.activeSelf)
        {
            controlsPanel.SetActive(false);
            pauseTitle.SetActive(true);
        }
        else if (tutorialsPanel.activeSelf)
        {
            tutorialsPanel.SetActive(false);
            pauseTitle.SetActive(true);
        }
    }
}
