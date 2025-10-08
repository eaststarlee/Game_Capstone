using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float speed = 40f;
    public float lifeTime = 5f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Fire(Vector3 dir)
    {
        // 3D ������ٵ�� linearVelocity�� �ƴ϶� velocity!
        rb.linearVelocity = dir.normalized * speed;
        CancelInvoke();
        Invoke(nameof(Despawn), lifeTime);
    }

    void Despawn()
    {
        Destroy(gameObject);
    }
}
