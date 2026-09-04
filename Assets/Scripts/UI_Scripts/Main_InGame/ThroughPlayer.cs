using UnityEngine;

public class ThroughPlayer : MonoBehaviour
{
    // 이제 총알이 플레이어한테 충돌하지 않습니다
    void Awake()
    {
        // Bullet 레이어: 8, Player에 할당된 Ignore Raycast 레이어: 2 . 총알과 플레이어가 충돌하지 않습니다.
        Physics.IgnoreLayerCollision(8, 2, true);
        // Bullet 레이어: 8, Invisible Ground 레이어 : 9 . 총알과 보이지 않는 바닥이 충돌하지 않습니다.
        Physics.IgnoreLayerCollision(8, 9, true);
        // Bullet 레이어: 8, Invisible Object 레이어 : 10 . 총알과 보이지 않는 물체가 충돌하지 않습니다.
        Physics.IgnoreLayerCollision(8, 10, true);
        Physics.IgnoreLayerCollision(8, 11, true);
    }
}
