using UnityEngine;
using System.Collections.Generic;

public class StringActivator : MonoBehaviour
{
    public static StringActivator Instance;

    // 등록된 UI 패널들을 이름으로 저장하는 창고
    private Dictionary<string, GameObject> uiPanels = new Dictionary<string, GameObject>();

    // ⚡ 해금된 UI 패널 이름들을 영구 보관하는 장부 (중복 방지)
    private HashSet<string> unlockedUINames = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // UI 패널이 깨어날 때 스스로를 등록하는 함수 (등록만 전담!)
    public void RegisterPanel(string uiName, GameObject panelObj)
    {
        if (panelObj == null || string.IsNullOrEmpty(uiName)) return;

        if (!uiPanels.ContainsKey(uiName))
        {
            uiPanels.Add(uiName, panelObj);
            Debug.Log($"[StringActivator] '{uiName}' 등록 완료.");
        }
        else
        {
            uiPanels[uiName] = panelObj;
        }

        // ⚡ 추가: 등록되는 시점에 이미 장부에 해금 기록이 있다면 즉시 켜준다!
        if (unlockedUINames.Contains(uiName))
        {
            panelObj.SetActive(true);
            Debug.Log($"[StringActivator] '{uiName}'은 이미 해금된 상태이므로 등록 즉시 활성화합니다.");
        }
    }

    // 이름으로 UI를 찾아 활성화하고, 장부에 해금 기록
    public void Activate(string targetName)
    {
        if (string.IsNullOrEmpty(targetName)) return;

        // 1. 장부에 기록
        if (!unlockedUINames.Contains(targetName))
        {
            unlockedUINames.Add(targetName);
        }

        // 2. 패널 활성화
        if (uiPanels.TryGetValue(targetName, out GameObject target))
        {
            if (target != null)
            {
                // 강제로 꺼졌다 켜지도록 하여 UI 갱신 보장
                target.SetActive(false);
                target.SetActive(true);
                Debug.Log($"[StringActivator] '{targetName}'을(를) 활성화했습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"[StringActivator] '{targetName}'이(가) 등록되지 않았습니다. 장부에는 기록됩니다.");
        }
    }

    // ==========================================
    // 💾 SaveManager 연동용 메서드
    // ==========================================

    public List<string> GetActivatedUINames()
    {
        return new List<string>(unlockedUINames);
    }

    public void RestoreActivatedUI(List<string> savedNames)
    {
        if (savedNames == null) return;

        unlockedUINames.Clear();
        foreach (string uiName in savedNames)
        {
            if (!unlockedUINames.Contains(uiName))
            {
                unlockedUINames.Add(uiName);
            }
        }

        Debug.Log($"[StringActivator] 총 {unlockedUINames.Count}개의 가이드북 해금 데이터 복원 완료.");

        // ⚡ [추가!] 장부 복원이 끝난 바로 그 순간, 이미 등록되어 있던 uiPanels 버튼들을 일괄 동기화!
        ForceSyncAllUnlockedUI();
    }

    public bool IsUnlocked(string uiName)
    {
        return !string.IsNullOrEmpty(uiName) && unlockedUINames.Contains(uiName);
    }

    // ⚡ ESC 눌렀을 때 장부 전체를 강제로 한번에 켜주는 확실한 함수
    public void ForceSyncAllUnlockedUI()
    {
        foreach (string uiName in unlockedUINames)
        {
            if (uiPanels.TryGetValue(uiName, out GameObject target) && target != null)
            {
                target.SetActive(true);
                Debug.Log($"[StringActivator] ForceSync로 '{uiName}' 활성화 성공!");
            }
        }
    }
}