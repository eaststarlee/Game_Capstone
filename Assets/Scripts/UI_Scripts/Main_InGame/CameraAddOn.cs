using UnityEngine;
using UnityEngine.UI;

public class CameraAddOn : MonoBehaviour
{
    [Header("UI 참조")]
    public Slider sensitivitySlider;      // 감도 슬라이더
    public GameObject uiPanelToCheck;     // UI 활성 상태 체크

    [Header("PlayerController 참조")]
    public PlayerController playerController;

    [Header("무기 스크립트")]
    private WeaponSway weaponSway;
    private GunController gunController;

    [Header("감도 계산")]
    public SensitivitySlider sensitivityCalculator;

    private bool uiActive = false;

    void Start()
    {
        // PlayerController 자동 참조
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        // 무기 스크립트 자동 참조
        GameObject player = playerController != null ? playerController.gameObject : null;
        if (player != null)
        {
            weaponSway = player.GetComponentInChildren<WeaponSway>();
            gunController = player.GetComponentInChildren<GunController>();
        }

        // SensitivitySlider 자동 참조
        if (sensitivityCalculator == null && sensitivitySlider != null)
            sensitivityCalculator = sensitivitySlider.GetComponent<SensitivitySlider>();

        // 초기 감도 적용
        if (playerController != null && sensitivityCalculator != null)
            playerController.mouseSensitivity = sensitivityCalculator.GetCalculatedSensitivity();
    }

    void Update()
    {
        if (uiPanelToCheck == null || playerController == null) return;

        bool shouldBlock = uiPanelToCheck.activeSelf;

        // --- PlayerController 입력 제어 ---
        playerController.inputEnabled = !shouldBlock;

        // --- 무기 스크립트 제어 ---
        if (weaponSway != null) weaponSway.enabled = !shouldBlock;
        if (gunController != null) gunController.enabled = !shouldBlock;

        // --- 커서 상태 제어 ---
        if (shouldBlock && !uiActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            uiActive = true;
        }
        else if (!shouldBlock && uiActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            uiActive = false;
        }

        // --- 감도 실시간 적용 ---
        if (playerController != null && sensitivityCalculator != null)
            playerController.mouseSensitivity = sensitivityCalculator.GetCalculatedSensitivity();
    }
}
