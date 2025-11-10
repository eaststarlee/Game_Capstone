using UnityEngine;

public class TriggerTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{gameObject.name} Trigger Enter: {other.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"{gameObject.name} Trigger Exit: {other.name}");
    }
}
