using UnityEngine;

public class WeaponSocketBinder : MonoBehaviour
{
    public Animator animator;
    public GameObject weaponRoot;
    public string socketName = "WeaponSocket";

    [Header("Offsets")]
    public Vector3 localRotationOffset = Vector3.zero;   // 기본 0 권장
    public Vector3 localPositionOffset = Vector3.zero;   // 필요하면 약간 보정

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!animator || !weaponRoot) return;

        var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        if (!rightHand) return;

        Transform socket = rightHand.Find(socketName);
        if (!socket)
        {
            socket = new GameObject(socketName).transform;
            socket.SetParent(rightHand, false);
            socket.localPosition = Vector3.zero;
            socket.localRotation = Quaternion.identity;
            socket.localScale = Vector3.one;
        }

        var t = weaponRoot.transform;
        t.SetParent(socket, false);
        t.localPosition = localPositionOffset;                  // ★ 핵심: 0으로 스냅
        t.localRotation = Quaternion.Euler(localRotationOffset);
        t.localScale = Vector3.one;
    }
}
