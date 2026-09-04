using UnityEngine;

public class BreakableObject : MonoBehaviour, IBreakable
{
    [Header("����(�μ����� ��) ������Ʈ")]
    [Tooltip("����θ� �� ��ũ��Ʈ�� �پ��ִ� ������Ʈ�� �������� ��޵˴ϴ�.")]
    public GameObject intactObject;

    [Header("�μ��� ���� ������ (����)")]
    [Tooltip("�μ����� �� ��Ÿ�� ���� ������. ������ �׳� ������ ������ϴ�.")]
    public GameObject fracturedPrefab;

    [Header("�ı� ����Ʈ (����)")]
    [Tooltip("�ı� �� ����� ��ƼŬ ����Ʈ ������ (ParticleSystem ����).")]
    public ParticleSystem destroyEffect;

    [Header("�ı� ���� (����)")]
    public AudioClip destroySfx;
    public float sfxVolume = 1f;

    [Header("���� ���� �ɼ� (����)")]
    [Tooltip("������ �� ���� ��. 0�̸� ���� �� ��.")]
    public float explosionForce = 5f;
    public float explosionRadius = 2f;
    public float upwardModifier = 0.5f;

    [Header("���� �ɼ�")]
    [Tooltip("������ ����/����Ʈ�� ���������� �ð�.")]
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

        // 1) �ı� ����Ʈ
        if (destroyEffect != null)
        {
            ParticleSystem ps = Instantiate(destroyEffect, pos, rot);
            Destroy(ps.gameObject, autoCleanupTime);
        }

        // 2) �μ��� ���� ����
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

        // 3) ���� ���
        if (destroySfx != null)
        {
            AudioSource.PlayClipAtPoint(destroySfx, pos, sfxVolume);
        }

        // 4) ���� ����
        Destroy(intactObject);
    }
}
