using UnityEngine;

public class ProjectileLifetime : MonoBehaviour
{
    [SerializeField] private float duration = 5f; // 5초 뒤 자동 삭제

    void Start()
    {
        // 생성되자마자 duration초 후에 스스로를 파괴하도록 예약
        Destroy(gameObject, duration);
    }
}