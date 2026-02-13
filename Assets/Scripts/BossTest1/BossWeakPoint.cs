using UnityEngine;

public class BossWeakPoint : MonoBehaviour
{
    private BossHealth mainHealth;

    void Start()
    {
        // 부모 오브젝트들에 붙어있는 BossHealth를 찾아 연결함
        mainHealth = GetComponentInParent<BossHealth>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 약점에 부딪혔을 때 부모의 HandleCollision에 "약점이다(true)"라고 전달
        if (mainHealth != null)
        {
            mainHealth.HandleCollision(other, true);
        }
    }
}