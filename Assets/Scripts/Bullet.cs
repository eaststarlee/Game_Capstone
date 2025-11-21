using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public float lifeTime = 3f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 순간적인 힘(Impulse)을 가하여 발사
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);

        Destroy(gameObject, lifeTime);
    }
}