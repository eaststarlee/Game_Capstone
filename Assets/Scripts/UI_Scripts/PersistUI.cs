using UnityEngine;

public class PersistUI : MonoBehaviour
{
    private void Awake()
    {
        // 이 오브젝트와 이 씬에 속한 것들을 파괴하지 않음
        DontDestroyOnLoad(gameObject);
    }
}