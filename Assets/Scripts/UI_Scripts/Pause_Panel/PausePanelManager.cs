using UnityEngine;

public class PausePanelManager : MonoBehaviour
{
    [Header("UI ÆÐ³Î")]
    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject escPanel;
    [SerializeField] private GameObject pauseTitle;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject tutorialsPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleESC();
        }
    }

    private void HandleESC()
    {
        if (!escPanel.activeSelf)
        {
            // ESC_Panel ºñÈ°¼ºÈ­ »óÅÂ ¡æ Main_UI ²ô°í ESC_Panel ÄÔ
            mainUI.SetActive(false);
            escPanel.SetActive(true);
        }
        else if (pauseTitle.activeSelf)
        {
            // Pause_Title ÄÑÁ®ÀÖÀ¸¸é ¡æ ESC_Panel ²ô°í Main_UI ÄÔ
            escPanel.SetActive(false);
            mainUI.SetActive(true);
        }
        else if (optionsPanel.activeSelf)
        {
            // Options ÄÑÁ®ÀÖÀ¸¸é ¡æ Options ²ô°í Pause_Title ÄÔ
            optionsPanel.SetActive(false);
            pauseTitle.SetActive(true);
        }
        else if (controlsPanel.activeSelf)
        {
            // Controls ÄÑÁ®ÀÖÀ¸¸é ¡æ Controls ²ô°í Pause_Title ÄÔ
            controlsPanel.SetActive(false);
            pauseTitle.SetActive(true);
        }
        else if (tutorialsPanel.activeSelf)
        {
            // Tutorials ÄÑÁ®ÀÖÀ¸¸é ¡æ Tutorials ²ô°í Pause_Title ÄÔ
            tutorialsPanel.SetActive(false);
            pauseTitle.SetActive(true);
        }
    }
}
