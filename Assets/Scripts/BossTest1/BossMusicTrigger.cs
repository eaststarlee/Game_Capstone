using System.Collections;
using UnityEngine;

public class BossMusicTrigger : MonoBehaviour
{
    [Header("음악 설정")]
    public AudioSource bgmAudioSource;
    public AudioClip bossMusicCD;

    private AudioClip stageMusicCD; 
    private float defaultVolume;   

    [Header("페이드 연출 설정")]
    public float fadeTime = 2.0f;

    private Coroutine currentFade; 
    private bool isBossRoom = false; 

    private void Start()
    {
        if (bgmAudioSource != null)
        {
            stageMusicCD = bgmAudioSource.clip;
            defaultVolume = bgmAudioSource.volume;
        }
    }

    // 1. 보스방에 들어갔을 때
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isBossRoom == false)
        {
            isBossRoom = true;

            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(FadeMusic(bossMusicCD));
        }
    }

    // 2. 보스방에서 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isBossRoom == true)
        {
            isBossRoom = false;

            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(FadeMusic(stageMusicCD));
        }
    }

    private IEnumerator FadeMusic(AudioClip newCD)
    {
        // 페이드 아웃
        while (bgmAudioSource.volume > 0)
        {
            bgmAudioSource.volume -= defaultVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        // BGM 변경
        bgmAudioSource.volume = 0;
        bgmAudioSource.clip = newCD;
        bgmAudioSource.Play();

        // 페이드 인
        while (bgmAudioSource.volume < defaultVolume)
        {
            bgmAudioSource.volume += defaultVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        bgmAudioSource.volume = defaultVolume; // 오차 보정
    }
}