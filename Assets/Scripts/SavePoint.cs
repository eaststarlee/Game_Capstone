using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePoint : MonoBehaviour
{
    private Collider triggerCollider;
    public static event Action OnSaveTriggered;

    [Header("Save UI Panel")]
    public GameObject saveUIPanel;

    [Header("Sound Effects")]
    public AudioClip saveSfx;
    [Range(0f, 1f)] public float saveSfxVolume = 1f;

    private bool isPlayerInRange = false;
    private PlayerHealth currentPlayerHealth;

    [Header("Respawn Adjustment")]
    public float respawnHeightOffset = 1.0f;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null) triggerCollider.isTrigger = true;
    }

    private void Update()
    {
        if (isPlayerInRange && currentPlayerHealth != null && Input.GetKeyDown(KeyCode.F))
        {
            ExecuteSaveSequence();
        }
    }

    private void ExecuteSaveSequence()
    {
        Vector3 colliderCenter = triggerCollider.bounds.center;
        float colliderBottomY = triggerCollider.bounds.min.y;
        Vector3 adjustedRespawnPoint = new Vector3(colliderCenter.x, colliderBottomY, colliderCenter.z)
                                     + Vector3.up * respawnHeightOffset;

        currentPlayerHealth.SetRespawnPoint(adjustedRespawnPoint);

        // 📸 1. UI 제외한 '순수 인게임 화면만' 캡처
        Texture2D screenshot = CaptureInGameOnly();

        string currentSceneName = SceneManager.GetActiveScene().name;

        // 💾 2. 1번 슬롯 무조건 자동저장 (AutoSave)
        SaveManager.AutoSave(adjustedRespawnPoint, screenshot);
        OnSaveTriggered?.Invoke();

        // 🖥️ 3. UI 컨트롤러에 최신 캡처본/데이터 전달 후 세이브 UI 팝업 켜기
        SavePanelController.SetCurrentSaveData(currentSceneName, adjustedRespawnPoint, screenshot);
        if (saveUIPanel != null)
        {
            saveUIPanel.SetActive(true);
        }

        // 🔊 4. 사운드 재생
        if (saveSfx != null)
        {
            AudioSource.PlayClipAtPoint(saveSfx, adjustedRespawnPoint, saveSfxVolume);
        }
    }

    // 💡 UI(Canvas)를 제외하고 메인 카메라 시점만 깔끔하게 캡처하는 핵심 함수
    private Texture2D CaptureInGameOnly()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return null;

        // 캡처용 썸네일 해상도 (640x360 - 용량 및 성능 최적화)
        int width = 640;
        int height = 360;

        RenderTexture rt = new RenderTexture(width, height, 24);
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        RenderTexture prevActive = RenderTexture.active;
        RenderTexture prevTarget = mainCam.targetTexture;

        // 메인 카메라인 순수 3D 게임 화면만 RenderTexture에 강제 렌더링
        mainCam.targetTexture = rt;
        mainCam.Render();

        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        // 카메라 설정 원래대로 복구
        mainCam.targetTexture = prevTarget;
        RenderTexture.active = prevActive;

        Destroy(rt); // 메모리 해제

        return screenshot;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            currentPlayerHealth = other.GetComponent<PlayerHealth>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            currentPlayerHealth = null;
        }
    }
}