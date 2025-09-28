using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]              // Animator LateUpdate 이후에 보정하고 싶을 때 유용
[DisallowMultipleComponent]
public class PlayerWeaponController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform cameraTransform;      // 주 카메라 Transform
    public CameraController cameraController; // 네가 쓰는 카메라 스크립트가 있다면(옵션)
    public Weapon currentWeapon;           // 발사 로직을 가진 Weapon 스크립트(옵션)
    public GameObject weaponRoot;          // 조준 시 활성화될 무기 루트(옵션)

    [Header("Aim Turning (Yaw)")]
    public Transform playerRoot;           // 캐릭터 전체 Yaw 회전 기준(보통 Hips 위 루트)
    public float aimTurnSpeed = 12f;
    public float yawBiasOnAim = 10f;       // 조준 시 약간의 Yaw 바이어스(+Y)

    [Header("Upper Body Pitch - Bone Chain")]
    public Transform hipsBone;             // mixamorig:Hips
    public Transform spine1Bone;           // mixamorig:Spine1
    public Transform chestBone;            // mixamorig:Spine2/UpperChest

    [Tooltip("본 회전 상/하한 (deg)")]
    public float hipsUpLimit = 10f, hipsDownLimit = 15f;
    public float spine1UpLimit = 20f, spine1DownLimit = 25f;
    public float chestUpLimit = 35f, chestDownLimit = 45f;

    [Tooltip("상/하 각도에 곱해질 가중치")]
    public float hipsWeightUp = 0.15f, hipsWeightDown = 0.35f;
    public float spine1WeightUp = 0.35f, spine1WeightDown = 0.50f;
    public float chestWeightUp = 0.60f, chestWeightDown = 0.85f;

    public float bonePitchLerp = 12f;      // 본 회전 보간 속도

    [Header("Pivot Fine-Tune (optional)")]
    public Transform aimPivot;             // 총 방향 보정용 로컬 피벗(옵션)
    public bool rotateWeaponToAim = false; // 피벗을 카메라 조준으로 회전할지
    public float pivotMaxYaw = 8f;         // 피벗 yaw 제한
    public float pivotMaxPitch = 8f;       // 피벗 pitch 제한
    public float pivotLerp = 15f;          // 피벗 보간 속도

    [Header("Controls")]
    public KeyCode unarmedKey = KeyCode.Alpha1;
    public KeyCode rifleKey = KeyCode.Alpha2;
    public KeyCode aimKey = KeyCode.Mouse1;
    public KeyCode fireKey = KeyCode.Mouse0;

    [Header("Weapon & Layers")]
    public string rifleLayerName = "Rifle Layer";
    public float layerChangeSpeed = 10f;

    [Header("Animator Params")]
    public string paramIsFiring = "isFiring";
    public string paramIsAiming = "isAiming";

    [Header("Aim Solve")]
    public LayerMask aimMask = ~0;
    public float minAimDistance = 3f;

    // ---- 내부 상태 ----
    int rifleLayerIndex = -1;
    float rifleLayerTargetWeight = 0f;
    float rifleLayerCurrentWeight = 0f;

    bool isAiming = false;
    public bool IsAiming => isAiming;

    Quaternion hipsDefault, spine1Default, chestDefault, pivotDefault;
    bool _ikTicked; // 이번 프레임에 OnAnimatorIK가 호출됐는지 플래그

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
        if (!cameraController && Camera.main) cameraController = Camera.main.GetComponent<CameraController>();

        if (animator)
        {
            animator.applyRootMotion = false;
            rifleLayerIndex = animator.GetLayerIndex(rifleLayerName);
            if (rifleLayerIndex < 0)
                Debug.LogWarning($"[PlayerWeaponController] Animator에 '{rifleLayerName}' 레이어가 없습니다. Animator Layers를 확인하세요.");
        }

        if (hipsBone) hipsDefault = hipsBone.localRotation;
        if (spine1Bone) spine1Default = spine1Bone.localRotation;
        if (chestBone) chestDefault = chestBone.localRotation;
        if (aimPivot) pivotDefault = aimPivot.localRotation;

        // 잘못 연결한 경우 방지
        if (chestBone && playerRoot && chestBone == playerRoot) chestBone = null;

        // 시작 시 무기 루트 비활성(선택)
        if (weaponRoot) weaponRoot.SetActive(false);
    }

    void Update()
    {
        HandleWeaponSwitch();
        HandleAim();
        HandleFire();
        ApplyLayerWeight();
    }

    // --- IK에서 조준 포즈 보정(있으면 여기서), 없으면 LateUpdate에서 한 번 더 ---
    void OnAnimatorIK(int layerIndex)
    {
        _ikTicked = true;
        ApplyAimPose();
    }

    void LateUpdate()
    {
        if (!_ikTicked) ApplyAimPose();
        _ikTicked = false;
    }

    // ─────────────────────────────────────────────────────────────────────
    // 입력 처리
    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(unarmedKey)) rifleLayerTargetWeight = 0f;
        if (Input.GetKeyDown(rifleKey)) rifleLayerTargetWeight = 1f;
    }

    void HandleAim()
    {
        bool wantAim = Input.GetKey(aimKey) && currentWeapon != null;

        if (wantAim != isAiming)
        {
            isAiming = wantAim;
            animator?.SetBool(paramIsAiming, isAiming);
            cameraController?.SetAiming(isAiming);
            if (weaponRoot) weaponRoot.SetActive(isAiming);
            if (!isAiming) animator?.SetBool(paramIsFiring, false);
        }
    }

    void HandleFire()
    {
        if (!currentWeapon) return;

        bool wantFire = Input.GetKey(fireKey) && isAiming;
        animator?.SetBool(paramIsFiring, wantFire);

        if (wantFire && currentWeapon.CanFire)
        {
            // 무기는 카메라 방향 기준으로 쏜다고 가정
            currentWeapon.Fire(cameraTransform, isAiming);
        }
    }
    // ─────────────────────────────────────────────────────────────────────

    // 레이어 가중치 보간 적용
    void ApplyLayerWeight()
    {
        if (rifleLayerIndex < 0 || animator == null) return;

        // 조준 중에는 항상 라이플 레이어를 1로 강제
        float target = isAiming ? 1f : rifleLayerTargetWeight;
        rifleLayerCurrentWeight = Mathf.Lerp(
            rifleLayerCurrentWeight,
            target,
            Time.deltaTime * layerChangeSpeed
        );

        animator.SetLayerWeight(rifleLayerIndex, rifleLayerCurrentWeight);
    }

    // 조준 포즈(몸통 pitch + 루트 yaw + 무기 피벗 보정)
    void ApplyAimPose()
    {
        if (!isAiming)
        {
            LerpBack(hipsBone, hipsDefault);
            LerpBack(spine1Bone, spine1Default);
            LerpBack(chestBone, chestDefault);
            if (aimPivot)
                aimPivot.localRotation = Quaternion.Slerp(aimPivot.localRotation, pivotDefault, Time.deltaTime * pivotLerp);
            return;
        }

        // (1) 캐릭터 Yaw를 카메라 Yaw로 정렬 + Yaw 바이어스
        if (playerRoot && cameraTransform)
        {
            Vector3 fwd = cameraTransform.forward; fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.0001f)
            {
                Quaternion camYaw = Quaternion.LookRotation(fwd);
                Quaternion bias = Quaternion.Euler(0f, yawBiasOnAim, 0f);
                Quaternion target = camYaw * bias;
                playerRoot.rotation = Quaternion.Slerp(playerRoot.rotation, target, Time.deltaTime * aimTurnSpeed);
            }
        }

        // (2) 상체 Pitch 분배(hips → spine1 → chest)
        if (cameraTransform)
        {
            float camPitch = GetCameraPitchDeg(cameraTransform.forward); // 위: +, 아래: -
            ApplyPitchChain(camPitch);
        }

        // (3) 무기 피벗 보정(선택)
        if (aimPivot)
        {
            if (!rotateWeaponToAim)
            {
                aimPivot.localRotation = Quaternion.Slerp(aimPivot.localRotation, pivotDefault, Time.deltaTime * pivotLerp);
            }
            else
            {
                Transform p = aimPivot.parent ? aimPivot.parent : aimPivot;
                Quaternion worldTarget = Quaternion.LookRotation(cameraTransform.forward, Vector3.up);
                Quaternion localTarget = Quaternion.Inverse(p.rotation) * worldTarget;

                Quaternion delta = Quaternion.Inverse(pivotDefault) * localTarget;
                Vector3 e = NormalizeEuler(delta.eulerAngles);
                e.x = Mathf.Clamp(e.x, -pivotMaxPitch, pivotMaxPitch);
                e.y = Mathf.Clamp(e.y, -pivotMaxYaw, pivotMaxYaw);
                e.z = 0f;

                Quaternion finalLocal = Quaternion.Slerp(
                    aimPivot.localRotation,
                    pivotDefault * Quaternion.Euler(e),
                    Time.deltaTime * pivotLerp
                );

                aimPivot.localRotation = finalLocal;
            }
        }
    }

    void ApplyPitchChain(float pitchDeg)
    {
        float hipsW = (pitchDeg >= 0f) ? hipsWeightUp : hipsWeightDown;
        float spine1W = (pitchDeg >= 0f) ? spine1WeightUp : spine1WeightDown;
        float chestW = (pitchDeg >= 0f) ? chestWeightUp : chestWeightDown;

        float hipsAng = Mathf.Clamp(pitchDeg * hipsW, -hipsDownLimit, hipsUpLimit);
        float spine1Ang = Mathf.Clamp(pitchDeg * spine1W, -spine1DownLimit, spine1UpLimit);
        float chestAng = Mathf.Clamp(pitchDeg * chestW, -chestDownLimit, chestUpLimit);

        if (hipsBone)
            hipsBone.localRotation = Quaternion.Slerp(
                hipsBone.localRotation,
                hipsDefault * Quaternion.Euler(hipsAng, 0f, 0f),
                Time.deltaTime * bonePitchLerp);

        if (spine1Bone)
            spine1Bone.localRotation = Quaternion.Slerp(
                spine1Bone.localRotation,
                spine1Default * Quaternion.Euler(spine1Ang, 0f, 0f),
                Time.deltaTime * bonePitchLerp);

        if (chestBone)
            chestBone.localRotation = Quaternion.Slerp(
                chestBone.localRotation,
                chestDefault * Quaternion.Euler(chestAng, 0f, 0f),
                Time.deltaTime * bonePitchLerp);
    }

    void LerpBack(Transform t, Quaternion defRot)
    {
        if (!t) return;
        t.localRotation = Quaternion.Slerp(t.localRotation, defRot, Time.deltaTime * bonePitchLerp);
    }

    static Vector3 NormalizeEuler(Vector3 e)
    {
        e.x = (e.x > 180f) ? e.x - 360f : e.x;
        e.y = (e.y > 180f) ? e.y - 360f : e.y;
        e.z = (e.z > 180f) ? e.z - 360f : e.z;
        return e;
    }

    static float GetCameraPitchDeg(Vector3 camForward)
    {
        camForward.Normalize();
        return Mathf.Asin(Mathf.Clamp(camForward.y, -1f, 1f)) * Mathf.Rad2Deg;
    }
}
