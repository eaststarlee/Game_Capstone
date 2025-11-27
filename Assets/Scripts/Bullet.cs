using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public float lifeTime = 3f;

    void Start()
    {
        // Destroy the bullet after a certain time to clean up the scene
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Move the bullet forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}