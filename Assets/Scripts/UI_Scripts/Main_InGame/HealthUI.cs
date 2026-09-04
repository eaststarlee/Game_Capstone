using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HealthUI : MonoBehaviour
{
    private PlayerHealth playerHealth;
    public Image[] heartImages;

    void Start()
    {
        StartCoroutine(FindPlayerHealthCoroutine());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FindPlayerHealthCoroutine());
    }

    IEnumerator FindPlayerHealthCoroutine()
    {
        playerHealth = null;

        // 최대 5초 동안 Player 태그를 가진 오브젝트를 찾음
        float timeout = 5f;
        float elapsed = 0f;

        while (playerHealth == null && elapsed < timeout)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }

            if (playerHealth != null)
                yield break;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.LogError("[HealthUI] PlayerHealth를 찾지 못했습니다.");
    }

    void Update()
    {
        UpdateHealthDisplay();
    }

    void UpdateHealthDisplay()
    {
        if (playerHealth == null) return;

        float regenProgress = playerHealth.GetRegenProgress();

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < playerHealth.currentHealth)
                heartImages[i].fillAmount = 1f;
            else if (i == playerHealth.currentHealth)
                heartImages[i].fillAmount = regenProgress;
            else
                heartImages[i].fillAmount = 0f;
        }
    }
}