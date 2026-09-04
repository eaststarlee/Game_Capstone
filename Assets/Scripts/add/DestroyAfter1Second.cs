using UnityEngine;

public class DestroyAfter1Second : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 1f); // 1초 뒤에 자기 자신 삭제
    }
}