// 지정된 영역 내에 랜덤으로 큐브 프리팹을 생성하고 삭제하는 스크립트
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject cubePrefab;
    public int numberOfCubes = 50;

    [Header("스폰 영역 설정")]
    public Vector3 spawnAreaCenter;
    public Vector3 spawnAreaSize;

    [Header("큐브 크기 설정")]
    public Vector2 minMaxScale = new Vector2(0.5f, 2.0f);

    public void GenerateCubes()
    {
        for (int i = 0; i < numberOfCubes; i++)
        {
            float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float randomY = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);
            float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);
            Vector3 spawnPosition = spawnAreaCenter + new Vector3(randomX, randomY, randomZ);

            GameObject newCube = Instantiate(cubePrefab, spawnPosition, Random.rotation);

            float randomScale = Random.Range(minMaxScale.x, minMaxScale.y);
            newCube.transform.localScale = Vector3.one * randomScale;

            newCube.transform.parent = this.transform;
        }
    }

    public void ClearCubes()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);
    }
}