using UnityEngine;
using System.Collections;

public class VignetteLoad : MonoBehaviour
{
    // 활성화/비활성화할 오브젝트 3개
    public GameObject targetObject1;
    public GameObject targetObject2;
    public GameObject targetObject3;

    // 활성화 지속 시간
    public float duration = 1f;

    // 빠르게 활성화했다 비활성화
    public void ActivateOnce()
    {
        if (targetObject1 != null)
            StartCoroutine(ActivateTemporary(targetObject1, duration));

        if (targetObject2 != null)
            StartCoroutine(ActivateTemporary(targetObject2, duration));

        if (targetObject3 != null)
            StartCoroutine(ActivateTemporary(targetObject3, duration));
    }

    private IEnumerator ActivateTemporary(GameObject obj, float duration)
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
    }
}
