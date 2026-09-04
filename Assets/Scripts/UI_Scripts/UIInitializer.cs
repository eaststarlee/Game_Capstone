using UnityEngine;
using System.Collections;

public class UIInitializer : MonoBehaviour
{
    [Header("오브젝트 연결")]
    public GameObject ESC_Panel;
    public GameObject OptionPanel;
    public GameObject LoadingImage; // Inspector에서 로딩 이미지 연결

    private void Start()
    {
        StartCoroutine(InitializePanels());
    }

    private IEnumerator InitializePanels()
    {
        // 로딩 이미지 활성화 (있다면)
        if (LoadingImage != null)
            LoadingImage.SetActive(true);

        // 1. ESC_Panel 활성화
        if (ESC_Panel != null)
            ESC_Panel.SetActive(true);

        // 2. OptionPanel 활성화
        if (OptionPanel != null)
            OptionPanel.SetActive(true);

        // 한 프레임 대기 (슬라이더 Awake/Start 초기화 보장)
        yield return null;

        // 3. OptionPanel 비활성화
        if (OptionPanel != null)
            OptionPanel.SetActive(false);

        // 4. ESC_Panel 비활성화
        if (ESC_Panel != null)
            ESC_Panel.SetActive(false);

        // 로딩 이미지 비활성화
        if (LoadingImage != null)
            LoadingImage.SetActive(false);
    }
}
