using UnityEngine;

public class PlaySFXOnEvent : MonoBehaviour
{
    public enum PlayMode
    {
        OnEnable,   // 활성화될 때마다 1회 재생
        OnStart,    // 처음 활성화될 때 1회 재생
        LoopWhileActive // 활성화되어 있는 동안 계속 반복 재생
    }

    [Header("Play Setting")]
    public PlayMode playMode = PlayMode.OnEnable;

    [Header("Audio")]
    public AudioClip sfx;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // AudioSource가 없으면 자동 추가
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 기본 설정
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        if (playMode == PlayMode.OnEnable)
        {
            PlayOneShot();
        }
        else if (playMode == PlayMode.LoopWhileActive)
        {
            StartLoop();
        }
    }

    void Start()
    {
        if (playMode == PlayMode.OnStart)
        {
            PlayOneShot();
        }
    }

    void OnDisable()
    {
        // 루프 재생 중이면 비활성화 시 정지
        if (playMode == PlayMode.LoopWhileActive && audioSource != null)
        {
            audioSource.Stop();
        }
    }

    void PlayOneShot()
    {
        if (sfx == null || audioSource == null)
            return;

        audioSource.loop = false;
        audioSource.PlayOneShot(sfx);
    }

    void StartLoop()
    {
        if (sfx == null || audioSource == null)
            return;

        audioSource.clip = sfx;
        audioSource.loop = true;
        audioSource.Play();
    }
}