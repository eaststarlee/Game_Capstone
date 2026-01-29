using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalAnim : MonoBehaviour
{
    public Texture[] frames;
    public float fps = 20.0f;
    public bool destroyOnEnd = false;

    private DecalProjector projector;
    private Material runtimeMaterial;

    private float timer = 0f;
    private int index = 0;
    private bool finished = false;

    void Start()
    {
        projector = GetComponent<DecalProjector>();

        runtimeMaterial = new Material(projector.material);
        projector.material = runtimeMaterial;

        if (frames.Length > 0)
            runtimeMaterial.SetTexture("Base_Map", frames[0]);
    }

    void Update()
    {
        if (finished || frames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f / fps)
        {
            timer -= 1f / fps;
            index++;

            if (index >= frames.Length)
            {
                index = frames.Length - 1;
                finished = true;

                if (destroyOnEnd)
                    Destroy(gameObject);

                return;
            }

            runtimeMaterial.SetTexture("Base_Map", frames[index]);
        }
    }
}
