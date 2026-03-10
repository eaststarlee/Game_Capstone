using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 관리를 위해 추가

public class CameraAddOn : MonoBehaviour
{
    // --- 싱글톤 설정 ---
    public static CameraAddOn Instance;

    [Header("UI 참조")]
    public Slider sensitivitySlider;
    public GameObject uiPanelToCheck;

    [Header("PlayerController 참조")]
    public PlayerController playerController;

    [Header("무기 스크립트")]
    private WeaponSway weaponSway;
    private GunController gunController;

    [Header("감도 계산")]
    public SensitivitySlider sensitivityCalculator;

    private bool uiActive = false;

    private void Awake()
    {
        // 1. 싱글톤 및 파괴 방지 설정
        if (Instance == null)
        {
            Instance = this;
            // 만약 이 스크립트가 UI_Root 같은 최상위 오브젝트에 붙어 있다면 아래 주석 해제
            // DontDestroyOnLoad(transform.root.gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 2. 씬 전환 시마다 플레이어를 새로 찾기 위한 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지 (이벤트 구독 해제)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 로드되면 자동으로 플레이어 참조 갱신
        FindAndRegisterPlayer();
    }

    void Start()
    {
        // 첫 시작 시 초기화
        InitializeReferences();
    }

    // 공통 참조 초기화 로직
    private void InitializeReferences()
    {
        if (playerController == null)
            playerController = Object.FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            weaponSway = playerController.GetComponentInChildren<WeaponSway>();
            gunController = playerController.GetComponentInChildren<GunController>();
        }

        if (sensitivityCalculator == null && sensitivitySlider != null)
            sensitivityCalculator = sensitivitySlider.GetComponent<SensitivitySlider>();

        // 초기 감도 적용
        ApplySensitivity();
    }

    void Update()
    {
        // 플레이어나 체크할 패널이 없으면 리턴 (씬 전환 직후 찰나의 순간 방지)
        if (uiPanelToCheck == null || playerController == null) return;

        bool shouldBlock = uiPanelToCheck.activeSelf;

        // --- PlayerController 입력 제어 ---
        playerController.inputEnabled = !shouldBlock;

        // --- 무기 스크립트 제어 ---
        if (weaponSway != null) weaponSway.enabled = !shouldBlock;
        if (gunController != null) gunController.enabled = !shouldBlock;

        // --- 커서 상태 제어 ---
        HandleCursorState(shouldBlock);

        // --- 감도 실시간 적용 ---
        ApplySensitivity();
    }

    private void HandleCursorState(bool shouldBlock)
    {
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
    }

    private void ApplySensitivity()
    {
        if (playerController != null && sensitivityCalculator != null)
            playerController.mouseSensitivity = sensitivityCalculator.GetCalculatedSensitivity();
    }

    // 플레이어가 직접 등록하거나 씬 로드 시 호출될 함수
    public void RegisterPlayer(PlayerController newPlayer)
    {
        playerController = newPlayer;
        if (playerController != null)
        {
            weaponSway = playerController.GetComponentInChildren<WeaponSway>();
            gunController = playerController.GetComponentInChildren<GunController>();
        }
        Debug.Log($"[CameraAddOn] '{newPlayer.gameObject.name}'로 참조가 성공적으로 갱신되었습니다.");
    }

    private void FindAndRegisterPlayer()
    {
        PlayerController foundPlayer = Object.FindFirstObjectByType<PlayerController>();
        if (foundPlayer != null)
        {
            RegisterPlayer(foundPlayer);
        }
    }
}