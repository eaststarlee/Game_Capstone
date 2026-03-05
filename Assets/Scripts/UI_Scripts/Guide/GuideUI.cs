using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuideUI : MonoBehaviour
{
    public static GuideUI Instance;

    [Header("Notification Settings")]
    [SerializeField] private GameObject notifyPanel;
    [SerializeField] private float notifyDuration = 2.0f;

    private Dictionary<string, GameObject> guideDictionary = new Dictionary<string, GameObject>();
    private GameObject currentActiveGuide;
    public bool IsGuideActive => currentActiveGuide != null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitGuideDictionary();
        if (notifyPanel != null) notifyPanel.SetActive(false);
    }

    private void Update()
    {
        // [추가] ESC 키 입력 감지
        // 현재 활성화된 가이드가 있을 때만 작동합니다.
        if (currentActiveGuide != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseGuide();
            }
        }
    }

    private void InitGuideDictionary()
    {
        foreach (Transform child in transform)
        {
            // 인스펙터에 연결된 notifyPanel 오브젝트는 가이드 목록(Dictionary)에 넣지 않음!
            if (child.gameObject == notifyPanel) continue;

            if (!guideDictionary.ContainsKey(child.name))
            {
                guideDictionary.Add(child.name, child.gameObject);
                child.gameObject.SetActive(false);
            }
        }
    }

    public void OpenGuide(string guideName)
    {
        if (guideDictionary.ContainsKey(guideName))
        {
            currentActiveGuide = guideDictionary[guideName];
            currentActiveGuide.SetActive(true);

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Debug.LogError($"{guideName} 이라는 이름의 가이드 UI를 찾을 수 없습니다!");
        }
    }

    public void CloseGuide()
    {
        if (currentActiveGuide == null) return;

        // 1. UI는 즉시 비활성화하여 플레이어 눈에는 안 보이게 합니다.
        currentActiveGuide.SetActive(false);

        // 2. 시간 재개 및 커서 설정
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 3. 알림 메시지 출력
        if (notifyPanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(NotifyRoutine());
        }

        // 4. [핵심] currentActiveGuide를 이번 프레임이 완전히 끝날 때 null로 만듭니다.
        StartCoroutine(ClearActiveGuideNextFrame());
    }

    private IEnumerator ClearActiveGuideNextFrame()
    {
        // 이번 프레임의 모든 Update, LateUpdate가 끝날 때까지 기다립니다.
        yield return new WaitForEndOfFrame();

        // 이제야 null로 만들어 다음 프레임부터 PausePanelManager가 ESC를 처리할 수 있게 합니다.
        currentActiveGuide = null;
    }

    private IEnumerator NotifyRoutine()
    {
        notifyPanel.SetActive(true);
        // Time.timeScale이 1이므로 일반 WaitForSeconds 사용
        yield return new WaitForSeconds(notifyDuration);
        notifyPanel.SetActive(false);
    }
}