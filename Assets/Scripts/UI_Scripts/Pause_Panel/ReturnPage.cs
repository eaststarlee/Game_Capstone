using UnityEngine;

public class ReturnPage : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private GameObject firstPage; // 처음에 보여줄 오브젝트
    [SerializeField] private GameObject[] otherPages; // 나머지 페이지들 (선택 사항)

    // 오브젝트가 활성화될 때마다 실행됩니다.
    private void OnEnable()
    {
        ResetUI();
    }

    public void ResetUI()
    {
        // 1. 첫 페이지가 할당되어 있다면 활성화
        if (firstPage != null)
        {
            firstPage.SetActive(true);
        }

        // 2. 나머지 페이지들은 모두 비활성화 (필요한 경우)
        if (otherPages != null)
        {
            foreach (GameObject page in otherPages)
            {
                if (page != null && page != firstPage)
                {
                    page.SetActive(false);
                }
            }
        }
    }
}