using UnityEngine;

[DefaultExecutionOrder(1000)] // Animator  Ŀ LateUpdate  
public class PlayerWeaponController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform cameraTransform;
    public CameraController cameraController;
    public Weapon currentWeapon;
    public GameObject weaponRoot;

    [Header("Aim Turning")]
    public Transform playerRoot;            // (Yaw) ȸ 
    public float aimTurnSpeed = 12f;
    public Transform aimPivot;              //    ǹ()
    public bool rotateWeaponToAim = false;  //  ī޶  
    public float yawBiasOnAim = 10f;        //   þ ̾(+Y)

    [Header("Upper Body Pitch - Bone Chain")]
    public Transform hipsBone;              // mixamorig:Hips
    public Transform spine1Bone;            // mixamorig:Spine1 ( ֵ )
    public Transform chestBone;             // mixamorig:Spine2 / UpperChest

    //   ȸ Ѱ(/Ʒ)
    public float hipsUpLimit = 10f, hipsDownLimit = 15f;
    public float spine1UpLimit = 20f, spine1DownLimit = 25f;
    public float chestUpLimit = 35f, chestDownLimit = 45f;

    // (+)/Ʒ(-)   ġ
    public float hipsWeightUp = 0.15f, hipsWeightDown = 0.35f;
    public float spine1WeightUp = 0.35f, spine1WeightDown = 0.50f;
    public float chestWeightUp = 0.60f, chestWeightDown = 0.85f;

    public float bonePitchLerp = 12f;       //   ӵ

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

    //  
    int rifleLayerIndex = -1;
    float rifleLayerTargetWeight = 0f;
    float rifleLayerCurrentWeight = 0f;

    bool isAiming = false;
    public bool IsAiming => isAiming;   // ٸ ũƮ(: RigWeightDriver) 

    Quaternion hipsDefault, spine1Default, chestDefault, pivotDefault;
    bool _ikTicked; // ش ӿ OnAnimatorIK ȣƴ

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
                Debug.LogWarning($"Animator '{rifleLayerName}' ̾ ϴ. (Layers ǿ ߰)");
            // Animator  ⺻̸ OK. (ʿ Culling: Always Animate)
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

    // --- B ٽ: IK  ⼭ ó,   LateUpdate    ó ---
    void OnAnimatorIK(int layerIndex)
    {
        _ikTicked = true;
        ApplyAimPose();
    }

    void LateUpdate()
    {
        if (!_ikTicked) ApplyAimPose();
        _ikTicked = false; //   ʱȭ
    }

    //  Yaw, ü Pitch й, ǹ    Լ 
    void ApplyAimPose()
    {
        //  ƴ   ڼ 
        if (!isAiming)
        {
            LerpBack(hipsBone, hipsDefault);
            LerpBack(spine1Bone, spine1Default);
            LerpBack(chestBone, chestDefault);
            if (aimPivot)
                aimPivot.localRotation = Quaternion.Slerp(aimPivot.localRotation, pivotDefault, Time.deltaTime * pivotLerp);
            return;
        }

        // (1) (Yaw): ī޶ Yaw +  ̾
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

        // (2) ü Pitch  üο й
        if (cameraTransform)
        {
            float camPitch = GetCameraPitchDeg(cameraTransform.forward); //  + / Ʒ -
            ApplyPitchChain(camPitch);
        }

        // (3)  ǹ(ɼ)
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
        // (+)/Ʒ(-)      ٸ
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

    // ---------- Է/ִϸ ----------

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

        bool wantFire = Input.GetKey(fireKey) && isAiming; //  ߿ ߻
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