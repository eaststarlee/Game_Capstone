using UnityEngine;

public class BreakableObject : MonoBehaviour, IBreakable
{
    [Header("원본(부서지기 전) 오브젝트")]
    [Tooltip("비워두면 이 스크립트가 붙어있는 오브젝트가 원본으로 취급됩니다.")]
    public GameObject intactObject;

    [Header("부서진 파편 프리팹 (선택)")]
    [Tooltip("부서졌을 때 나타날 파편 프리팹. 없으면 그냥 원본만 사라집니다.")]
    public GameObject fracturedPrefab;

    [Header("파괴 이펙트 (선택)")]
    [Tooltip("파괴 시 재생할 파티클 이펙트 프리팹 (ParticleSystem 포함).")]
    public ParticleSystem destroyEffect;

    [Header("파괴 사운드 (선택)")]
    public AudioClip destroySfx;
    public float sfxVolume = 1f;

    [Header("폭발 느낌 옵션 (선택)")]
    [Tooltip("파편에 줄 폭발 힘. 0이면 적용 안 함.")]
    public float explosionForce = 5f;
    public float explosionRadius = 2f;
    public float upwardModifier = 0.5f;

    [Header("정리 옵션")]
    [Tooltip("생성된 파편/이펙트가 사라지기까지 시간.")]
    public float autoCleanupTime = 5f;

    private bool isBroken = false;

    private void Awake()
    {
        if (intactObject == null)
        {
            intactObject = gameObject;
        }
    }

    public void Break()
    {
        if (isBroken) return;
        isBroken = true;

        Vector3 pos = intactObject.transform.position;
        Quaternion rot = intactObject.transform.rotation;

        // 1) 파괴 이펙트
        if (destroyEffect != null)
        {
            ParticleSystem ps = Instantiate(destroyEffect, pos, rot);
            Destroy(ps.gameObject, autoCleanupTime);
        }

        // 2) 부서진 파편 생성
        if (fracturedPrefab != null)
        {
            GameObject frac = Instantiate(fracturedPrefab, pos, rot);
            frac.transform.localScale = intactObject.transform.lossyScale;

            if (explosionForce > 0f)
            {
                Rigidbody[] rbs = frac.GetComponentsInChildren<Rigidbody>();
                foreach (var rb in rbs)
                {
                    rb.AddExplosionForce(explosionForce, pos, explosionRadius, upwardModifier, ForceMode.Impulse);
                }
            }

            Destroy(frac, autoCleanupTime);
        }

        // 3) 사운드 재생
        if (destroySfx != null)
        {
            AudioSource.PlayClipAtPoint(destroySfx, pos, sfxVolume);
        }

        // 4) 원본 제거
        Destroy(intactObject);
    }
}
