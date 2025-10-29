using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InkProjectileController : MonoBehaviour
{
    [SerializeField] private GameObject inkDecalObjectPrefab;
    [SerializeField] private float lifeTime = 5.0f;
    [SerializeField] private Color prefabColor;

    private Rigidbody rigidBodyComp;
    private Vector3 dir;

    void Start()
    {
        rigidBodyComp = GetComponent<Rigidbody>();
        gameObject.GetComponent<Renderer>().material.color = prefabColor;
        inkDecalObjectPrefab.GetComponent<DecalProjector>().material.color= prefabColor;
    }

    void Update()
    {
        dir = rigidBodyComp.linearVelocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject Instance = Instantiate(inkDecalObjectPrefab);

        Instance.transform.SetPositionAndRotation(collision.GetContact(0).point + collision.GetContact(0).normal * 0.1f, Quaternion.LookRotation(dir));
        

        Destroy(Instance, lifeTime);
        Destroy(gameObject);
    }
}