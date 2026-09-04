using UnityEngine;

public class BossBalloon : MonoBehaviour
{
    [Header("Settings")]
    public InkType requiredInkType; // 이 풍선이 터지는 잉크 색상
    public GameObject burstEffectPrefab; // 터질 때 파티클
    public AudioClip burstSfx;

    [Header("Debug State")] // 인스펙터에서 실시간 확인 및 조절 가능
    public bool isBurst = false;
    private BossController2 master; // 타입을 BC2Test로 변경

    public void Init(BossController2 controller) // 매개변수 타입도 변경
    {
        master = controller;
        isBurst = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBurst) return;

        // 투사체로부터 잉크 타입 확인
        InkProjectileController projectile = other.GetComponentInParent<InkProjectileController>();

        if (projectile != null && projectile.inkType == requiredInkType)
        {
            Burst();
        }
    }
    private void Burst()
    {
        if (isBurst) return; // 중복 실행 방지
        isBurst = true;

        // 1. 시각 이펙트 생성
        if (burstEffectPrefab)
        {
            Instantiate(burstEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. 사운드 재생
        if (burstSfx)
        {
            AudioSource.PlayClipAtPoint(burstSfx, transform.position);
        }

        // 3. 보스(마스터)에게 알림
        if (master != null)
        {
            master.OnBalloonBurst();
        }

        // 4. [핵심] 풍선 오브젝트 사라지게 하기
        // Destroy(gameObject)를 써도 되지만, 
        // 다시 시작할 때를 대비해 SetActive(false)를 추천합니다.
        gameObject.SetActive(false);
    }
}