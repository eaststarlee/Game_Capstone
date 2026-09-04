using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP System")]
    public int maxHealth = 5;
    public int currentHealth;

    public static event System.Action OnPlayerRespawn;

    [Header("Auto Regen System")]
    public float noDamageDuration = 7f;
    public float regenSpeed = 2f;
    private float lastDamageTime;
    private float currentRegenTimer;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 3f;
    private bool isInvincible = false;

    [Header("Respawn Settings")]
    public Vector3 respawnPoint;
    public float deathYLevel = -100f;

    [Header("1인칭 시각 피드백 (무기 모델)")]
    public GameObject weaponRoot;
    private List<Renderer> weaponRenderers = new List<Renderer>();

    [Header("Sound Effects")]
    public AudioClip damageSfx;
    public AudioClip healSfx;
    public AudioClip respawnSfx;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Auto Regen System")]
    public bool canRegen = true;

    public static PlayerHealth Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        respawnPoint = transform.position;
        currentHealth = maxHealth;
        lastDamageTime = Time.time;

        if (weaponRoot != null)
        {
            Renderer[] renderers = weaponRoot.GetComponentsInChildren<Renderer>();
            weaponRenderers.AddRange(renderers);
        }
    }

    private void Start()
    {
        // 💡 게임 로드 후 위치 배치 처리
        if (LoadPanelController.hasLoadedPosition)
        {
            // 석상 저장 등 좌표값이 존재하는 경우 해당 위치로 이동
            if (LoadPanelController.loadedPlayerPosition != Vector3.zero)
            {
                transform.position = LoadPanelController.loadedPlayerPosition;
                respawnPoint = LoadPanelController.loadedPlayerPosition;
            }
            // Vector3.zero인 경우(보스 클리어 로드): 이동 없이 Stage2 씬 내 기본 스폰 위치 유지

            LoadPanelController.hasLoadedPosition = false;
        }
    }

    void Update()
    {
        if (transform.position.y < deathYLevel)
        {
            Respawn();
        }

        HandleAutoRegen();
    }

    private void HandleAutoRegen()
    {
        if (currentHealth >= maxHealth || !canRegen)
        {
            currentRegenTimer = 0f;
            return;
        }

        if (Time.time - lastDamageTime >= noDamageDuration)
        {
            currentRegenTimer += Time.deltaTime;

            if (currentRegenTimer >= regenSpeed)
            {
                currentHealth++;
                currentRegenTimer = 0f;

                if (healSfx != null)
                {
                    AudioSource.PlayClipAtPoint(healSfx, transform.position, sfxVolume);
                }
            }
        }
        else
        {
            currentRegenTimer = 0f;
        }
    }

    public float GetRegenProgress()
    {
        if (currentHealth >= maxHealth) return 0f;
        if (Time.time - lastDamageTime < noDamageDuration) return 0f;

        return Mathf.Clamp01(currentRegenTimer / regenSpeed);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        lastDamageTime = Time.time;
        currentRegenTimer = 0f;

        currentHealth -= damage;

        if (damageSfx != null)
        {
            AudioSource.PlayClipAtPoint(damageSfx, transform.position, sfxVolume);
        }

        if (currentHealth <= 0)
        {
            Respawn();
        }
        else
        {
            StartCoroutine(BecomeInvincible());
        }
    }

    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;

        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            SetWeaponVisible(false);
            yield return new WaitForSeconds(0.15f);

            SetWeaponVisible(true);
            yield return new WaitForSeconds(0.15f);

            elapsed += 0.3f;
        }

        SetWeaponVisible(true);
        isInvincible = false;
    }

    private void SetWeaponVisible(bool visible)
    {
        foreach (Renderer rend in weaponRenderers)
        {
            if (rend != null) rend.enabled = visible;
        }
    }

    public void Respawn()
    {
        OnPlayerRespawn?.Invoke();

        StopAllCoroutines();

        currentHealth = maxHealth;
        isInvincible = false;
        SetWeaponVisible(true);
        lastDamageTime = Time.time;
        currentRegenTimer = 0f;

        StartCoroutine(SafeRespawnRoutine());

        if (respawnSfx != null)
        {
            GameObject sfxObj = new GameObject("RespawnSfx");
            sfxObj.transform.position = respawnPoint;
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.clip = respawnSfx;
            source.spatialBlend = 0f;
            source.volume = sfxVolume;
            source.Play();
            Destroy(sfxObj, respawnSfx.length + 0.1f);
        }
    }

    private IEnumerator SafeRespawnRoutine()
    {
        CharacterController cc = GetComponent<CharacterController>();
        PlayerController pc = GetComponent<PlayerController>();

        if (cc != null) cc.enabled = false;
        if (pc != null) pc.ResetVelocity();

        yield return new WaitForEndOfFrame();

        transform.position = respawnPoint;

        if (pc != null) pc.ResetVelocity();
        if (cc != null) cc.enabled = true;

        // 💾 [AutoSave] 부활이 완료된 후 1번 슬롯 자동 저장
        SaveManager.AutoSave(transform.position);
    }

    public void SetRespawnPoint(Vector3 newPoint)
    {
        respawnPoint = newPoint;
    }
}