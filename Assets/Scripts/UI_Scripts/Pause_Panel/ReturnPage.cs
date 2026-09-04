using UnityEngine;
using System.Collections.Generic;

public class ReturnPage : MonoBehaviour
{
    [System.Serializable]
    public class PagePair
    {
        public string pairName;      // 구분용 이름
        public GameObject mainPage;  // Page 1 (부모)
        public GameObject subPage;   // Page 2 (자식)
    }

    [Header("페이지 쌍 설정")]
    [SerializeField] private List<PagePair> pagePairs = new List<PagePair>();

    private void OnEnable()
    {
        ResetAllToMain();
    }

    private void Update()
    {
        // 실시간으로 감시: Page 2가 꺼져있는데 Page 1도 꺼져있다면 Page 1을 켬
        CheckPageStatus();
    }

    public void ResetAllToMain()
    {
        foreach (var pair in pagePairs)
        {
            if (pair.mainPage != null) pair.mainPage.SetActive(true);
            if (pair.subPage != null) pair.subPage.SetActive(false);
        }
    }

    private void CheckPageStatus()
    {
        foreach (var pair in pagePairs)
        {
            // 예외 처리: 할당 안 된 경우 패스
            if (pair.mainPage == null || pair.subPage == null) continue;

            // Page 2(자식)가 비활성화 되었는데, Page 1(부모)도 꺼져 있다면?
            if (!pair.subPage.activeSelf && !pair.mainPage.activeSelf)
            {
                pair.mainPage.SetActive(true);
            }
        }
    }
}