using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class StringActivator : MonoBehaviour
{
    // 어디서든 StringActivator.Instance.Activate("이름")으로 접근 가능
    public static StringActivator Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬이 바뀌어도 파괴되지 않게 하려면 아래 주석을 해제하세요.
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 비활성화된 오브젝트를 포함하여 이름으로 찾아 활성화합니다.
    /// </summary>
    public void Activate(string targetName)
    {
        if (string.IsNullOrEmpty(targetName)) return;

        GameObject target = FindAllInsideScene(targetName);

        if (target != null)
        {
            target.SetActive(true);
            Debug.Log($"[StringActivator] '{targetName}'을(를) 활성화했습니다.");
        }
        else
        {
            Debug.LogWarning($"[StringActivator] '{targetName}'을(를) 찾을 수 없습니다.");
        }
    }

    private GameObject FindAllInsideScene(string targetName)
    {
        // 1. 현재 로드된 모든 씬을 확인 (MainUI 씬 포함)
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            // 2. 씬의 최상위(Root) 오브젝트들을 가져옴
            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (GameObject root in rootObjects)
            {
                // 3. 최상위부터 자식의 자식까지 모두 검색 (비활성화 포함)
                Transform[] allChildren = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.name == targetName)
                    {
                        return child.gameObject;
                    }
                }
            }
        }
        return null;
    }
}