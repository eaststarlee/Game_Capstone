// AutoDestroyEffect.cs
using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    public float delay = 2.0f; // 2초 뒤 삭제
    void Start() => Destroy(gameObject, delay);
}