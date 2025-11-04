using UnityEngine;
using System.Collections.Generic;

public class NoiseVoxelMap : MonoBehaviour
{
    // === 기본 설정 ===
    public float offsetX;
    public float offsetZ;

    public int width = 20;
    public int depth = 20;
    public int maxHeight = 16;
    public int waterLevel = 8;

    [SerializeField] public float noiseScale = 20f;

    // === 프리팹 ===
    public GameObject grassPrefab;
    public GameObject dirtPrefab;
    public GameObject waterPrefab;
    public GameObject orePrefab;

    // === 광물 생성 설정 ===
    public int oreMaxHeight = 7;
    [SerializeField] public float oreNoiseScale = 45f;
    [Range(0.0f, 1.0f)] public float oreThreshold = 0.55f;
    [Range(0.0f, 1.0f)] public float oreChance = 0.15f;

    // === 내부 상태 ===
    private HashSet<Vector3Int> placedBlocks = new HashSet<Vector3Int>();

    void Start()
    {
        offsetX = Random.Range(-9999f, 9999f);
        offsetZ = Random.Range(-9999f, 9999f);

        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // 지형 높이 계산
                float nx = (x + offsetX) / noiseScale;
                float nz = (z + offsetZ) / noiseScale;

                float noise = Mathf.PerlinNoise(nx, nz);
                int h = Mathf.FloorToInt(noise * maxHeight);

                if (h <= 0) continue;

                // 블록 배치
                for (int y = 0; y <= h; y++)
                {
                    if (y == h)
                    {
                        Place(grassPrefab, x, y, z); // Grass
                    }
                    else
                    {
                        bool isOre = false;

                        // 광물 생성 조건 검사
                        if (y < h && y <= oreMaxHeight)
                        {
                            // 3D 분포를 흉내내는 노이즈 샘플링
                            float ore_nx = (x + offsetX * 2) / oreNoiseScale;
                            float ore_ny = (y + offsetZ * 3) / oreNoiseScale;
                            float ore_nz = (z + offsetX * 3) / oreNoiseScale;

                            float noise1 = Mathf.PerlinNoise(ore_nx, ore_ny);
                            float noise2 = Mathf.PerlinNoise(ore_nz, ore_ny);
                            float oreNoise = (noise1 + noise2) / 2f;

                            // 노이즈 임계값 및 확률 검사
                            if (oreNoise > oreThreshold)
                            {
                                if (Random.value < oreChance)
                                {
                                    isOre = true;
                                }
                            }
                        }

                        if (isOre)
                        {
                            Place(orePrefab, x, y, z); // Ore
                        }
                        else
                        {
                            Place(dirtPrefab, x, y, z); // Dirt
                        }
                    }
                }

                // 물 채우기
                for (int y = 0; y < waterLevel; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    if (!placedBlocks.Contains(pos))
                    {
                        Place(waterPrefab, x, y, z);
                    }
                }
            }
        }
    }

    void Place(GameObject prefab, int x, int y, int z)
    {
        var go = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"{prefab.name}_{x}_{y}_{z}";
        placedBlocks.Add(new Vector3Int(x, y, z));
    }
}