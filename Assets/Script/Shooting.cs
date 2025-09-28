using UnityEngine;

public class Shooting : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float x, y, z;
    

    void Awake()
    {
        rb = GetComponent<Rigidbody>();    
    }
    void Start()
    {
        rb.AddForce(new Vector3(x, y, z));
    }
}
