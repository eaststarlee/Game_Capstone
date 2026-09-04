// SpinObject.cs
using UnityEngine;

public class SpinObject : MonoBehaviour
{
    [Tooltip("오브젝트의 회전 속도 (초당 각도)")]
    public float spinSpeed = 100f;

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }
}
