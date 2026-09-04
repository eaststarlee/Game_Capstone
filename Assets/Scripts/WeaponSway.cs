
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float smooth = 12f;
    [SerializeField] private float positionSwayMultiplier = 0.01f;
    [SerializeField] private float rotationSwayMultiplier = 0.5f;

    [Header("Bob Settings")]
    [SerializeField] private float landingBobAmount = 0.1f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        // --- Positional Sway ---
        Vector3 positionTarget = new Vector3(-mouseX * positionSwayMultiplier, -mouseY * positionSwayMultiplier, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + positionTarget, Time.deltaTime * smooth);

        // --- Rotational Sway ---
        Quaternion rotationTargetX = Quaternion.AngleAxis(mouseY * rotationSwayMultiplier, Vector3.right);
        Quaternion rotationTargetY = Quaternion.AngleAxis(-mouseX * rotationSwayMultiplier, Vector3.up);
        Quaternion rotationTarget = initialRotation * rotationTargetX * rotationTargetY;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, rotationTarget, Time.deltaTime * smooth);
    }

    public void ApplyLandingBob()
    {
        transform.localPosition -= new Vector3(0, landingBobAmount, 0);
    }
}
