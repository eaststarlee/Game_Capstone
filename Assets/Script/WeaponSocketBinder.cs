using UnityEngine;

public class WeaponSocketBinder : MonoBehaviour
{
    public Animator animator;       // Player의 Animator
    public GameObject weaponRoot;   // 손에 들 무기(라이플 인스턴스 최상위)
    public string socketName = "WeaponSocket";

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!animator || !weaponRoot) return;

        // 오른손 뼈 Transform 가져오기 (Humanoid 필수)
        var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        if (!rightHand) { Debug.LogWarning("RightHand bone not found"); return; }

        // 소켓 생성(없으면)
        Transform socket = rightHand.Find(socketName);
        if (!socket)
        {
            socket = new GameObject(socketName).transform;
            socket.SetParent(rightHand, false); // 로컬 0,0,0
        }

        // 무기 장착
        weaponRoot.transform.SetParent(socket, false); // 위치/회전 자동 정렬
    }
}
