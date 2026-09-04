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
    [SerializeField] private GameObject guidesPanel;

    [Header("일시중지 제외 씬")]
    [SerializeField] private string exemptSceneName = "MainScene";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GuideUI.Instance != null && GuideUI.Instance.IsGuideActive)
            {
                return;
            }
            HandleESC();
        }
    }

    private void HandleESC()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // 1. 메뉴가 아예 닫혀 있는 경우 -> 일시중지 메뉴 열기
        if (!escPanel.activeSelf)
        {
            mainUI.SetActive(false);
            escPanel.SetActive(true);

            // 모든 서브 패널은 끄고 타이틀만 켠다 (초기화)
            optionsPanel.SetActive(false);
            controlsPanel.SetActive(false);
            tutorialsPanel.SetActive(false);
            guidesPanel.SetActive(false);
            pauseTitle.SetActive(true);

            if (currentScene != exemptSceneName)
                Time.timeScale = 0f;

            return; // 여기서 로직 종료 (중복 실행 방지)
        }

        // 2. 메뉴가 열려 있는 경우 -> 계층별로 닫기 (하위 -> 상위 순서)
        if (guidesPanel.activeSelf)
        {
            guidesPanel.SetActive(false);
            tutorialsPanel.SetActive(true);
        }
        else if (tutorialsPanel.activeSelf)
        {
            tutorialsPanel.SetActive(false);
            pauseTitle.SetActive(true);
        }
        else if (optionsPanel.activeSelf || controlsPanel.activeSelf)
        {
            optionsPanel.SetActive(false);
            controlsPanel.SetActive(false);
            pauseTitle.SetActive(true);
        }
        else if (pauseTitle.activeSelf)
        {
            // 최상위 타이틀에서 ESC를 누르면 메뉴 닫기
            escPanel.SetActive(false);
            mainUI.SetActive(true);

            if (currentScene != exemptSceneName)
                Time.timeScale = 1f;
        }
    }
}
