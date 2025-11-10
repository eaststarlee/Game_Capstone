using UnityEngine;

public class ThroughPlayer : MonoBehaviour
{
    // 이제 총알이 플레이어한테 충돌하지 않습니다
    void Awake()
    {
        // Bullet 레이어: 8, Player 레이어: 9 (레이어 번호는 프로젝트에서 확인)
        Physics.IgnoreLayerCollision(8, 2, true);
    }
}
