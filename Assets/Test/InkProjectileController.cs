using UnityEngine;

public class InkProjectileController : MonoBehaviour
{
    [SerializeField] private GameObject inkDecalObjectPrefab;
    [SerializeField] private float LifeTime = 5.0f;

    private Rigidbody rigidBodyComp;
    private Vector3 dir;

    void Start()
    {
        rigidBodyComp = GetComponent<Rigidbody>();
    }

    void Update()
    {
        dir = rigidBodyComp.linearVelocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject Instance = Instantiate(inkDecalObjectPrefab);

        Instance.transform.SetPositionAndRotation(collision.GetContact(0).point + collision.GetContact(0).normal * 0.1f, Quaternion.LookRotation(dir));
        

        Destroy(Instance, LifeTime);
        Destroy(gameObject);
    }
}