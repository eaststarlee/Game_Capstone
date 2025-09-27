using UnityEngine;

[DefaultExecutionOrder(1000)] // Animator 갱신 이후에 LateUpdate가 오도록 안전망
public class PlayerWeaponController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform cameraTransform;
    public CameraController cameraController;
    public Weapon currentWeapon;
    public GameObject weaponRoot;

    [Header("Aim Turning")]
    public Transform playerRoot;            // 몸(Yaw) 회전 기준
    public float aimTurnSpeed = 12f;
    public Transform aimPivot;              // 무기 소켓 보정 피벗(선택)
    public bool rotateWeaponToAim = false;  // 총을 카메라로 돌릴지 여부
    public float yawBiasOnAim = 10f;        // 오른쪽 어깨 시야 바이어스(+Y)

    [Header("Upper Body Pitch - Bone Chain")]
    public Transform hipsBone;              // mixamorig:Hips
    public Transform spine1Bone;            // mixamorig:Spine1 (없으면 비워둬도 됨)
    public Transform chestBone;             // mixamorig:Spine2 / UpperChest

    // 각 뼈 회전 한계(위/아래)
    public float hipsUpLimit = 10f, hipsDownLimit = 15f;
    public float spine1UpLimit = 20f, spine1DownLimit = 25f;
    public float chestUpLimit = 35f, chestDownLimit = 45f;

    // 위(+)/아래(-)로 볼 때 가중치
    public float hipsWeightUp = 0.15f, hipsWeightDown = 0.35f;
    public float spine1WeightUp = 0.35f, spine1WeightDown = 0.50f;
    public float chestWeightUp = 0.60f, chestWeightDown = 0.85f;

    public float bonePitchLerp = 12f;       // 뼈 보간 속도

    [Header("Fine-Tune (optional)")]
    public float pivotMaxYaw = 8f;
    public float pivotMaxPitch = 8f;
    public float pivotLerp = 15f;

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

    // 내부 상태
    int rifleLayerIndex = -1;
    float rifleLayerTargetWeight = 0f;
    float rifleLayerCurrentWeight = 0f;

    bool isAiming = false;
    public bool IsAiming => isAiming;   // 다른 스크립트(예: RigWeightDriver)에서 사용

    Quaternion hipsDefault, spine1Default, chestDefault, pivotDefault;
    bool _ikTicked; // 해당 프레임에 OnAnimatorIK가 호출됐는지

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
                Debug.LogWarning($"Animator에 '{rifleLayerName}' 레이어가 없습니다. (Layers 탭에서 추가)");
            // Animator 설정은 기본값이면 OK. (필요시 Culling: Always Animate)
        }

        if (hipsBone) hipsDefault = hipsBone.localRotation;
        if (spine1Bone) spine1Default = spine1Bone.localRotation;
        if (chestBone) chestDefault = chestBone.localRotation;
        if (aimPivot) pivotDefault = aimPivot.localRotation;

        if (chestBone && playerRoot && chestBone == playerRoot) chestBone = null;
    }

    void Start()
    {
        if (weaponRoot) weaponRoot.SetActive(false);
    }

    void Update()
    {
        HandleWeaponSwitch();
        HandleAim();
        HandleFire();
        ApplyLayerWeight();
    }

    // --- B안 핵심: IK가 오면 여기서 처리, 안 오면 LateUpdate에서 한 번 더 처리 ---
    void OnAnimatorIK(int layerIndex)
    {
        _ikTicked = true;
        ApplyAimPose();
    }

    void LateUpdate()
    {
        if (!_ikTicked) ApplyAimPose();
        _ikTicked = false; // 다음 프레임 초기화
    }

    // 몸 Yaw, 상체 Pitch 분배, 피벗 보정을 모두 한 함수로 묶음
    void ApplyAimPose()
    {
        // 조준 아닐 때는 원래 자세로 복귀
        if (!isAiming)
        {
            LerpBack(hipsBone, hipsDefault);
            LerpBack(spine1Bone, spine1Default);
            LerpBack(chestBone, chestDefault);
            if (aimPivot)
                aimPivot.localRotation = Quaternion.Slerp(aimPivot.localRotation, pivotDefault, Time.deltaTime * pivotLerp);
            return;
        }

        // (1) 몸(Yaw): 카메라 Yaw + 오른쪽 바이어스
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

        // (2) 상체 Pitch를 뼈 체인에 분배
        if (cameraTransform)
        {
            float camPitch = GetCameraPitchDeg(cameraTransform.forward); // 위 + / 아래 -
            ApplyPitchChain(camPitch);
        }

        // (3) 총 피벗(옵션)
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
                    Time.deltaTime * pivotLerp);

                aimPivot.localRotation = finalLocal;
            }
        }
    }

    void ApplyPitchChain(float pitchDeg)
    {
        // 위(+)/아래(-)에 따라 각 뼈에 줄 비율을 다르게
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

    // ---------- 입력/애니메이터 ----------

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

        bool wantFire = Input.GetKey(fireKey) && isAiming; // 조준 중에만 발사
        animator?.SetBool(paramIsFiring, wantFire);

        if (wantFire && currentWeapon.CanFire)
        {
            currentWeapon.Fire(cameraTransform, isAiming);
        }
    }

    void ApplyLayerWeight()
    {
        if (rifleLayerIndex < 0) return;

        float target = isAiming ? 1f : rifleLayerTargetWeight;
        rifleLayerCurrentWeight = Mathf.Lerp(
            rifleLayerCurrentWeight,
            target,
            Time.deltaTime * layerChangeSpeed
        );

        animator.SetLayerWeight(rifleLayerIndex, rifleLayerCurrentWeight);
    }
}
