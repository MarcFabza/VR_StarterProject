using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CubeType { Gun, Sword, Switch, Slow, Fast }

[System.Serializable]
public class Wave
{
    public string name = "Wave";
    public float duration = 20f;
    public float spawnInterval = 2f;
    public float speedLevel = 5f;
    [Range(0f, 1f)] public float doubleSpawnChance = 0f;
    public bool startWithSwitchCube = false;

    [Header("Probabilidad relativa de cada tipo")]
    public float gunWeight = 1f;
    public float swordWeight = 0f;
    public float switchWeight = 0f;
    public float slowWeight = 0f;
    public float fastWeight = 0f;
}

public class CubeSpawner : MonoBehaviour
{
    public static CubeSpawner Instance;
    public static float GameSpeed = 1f;

    [Header("Prefabs")]
    public GameObject[] cubePrefabs;
    public GameObject switchCubePrefab;
    public GameObject slowCubePrefab;
    public GameObject fastCubePrefab;
    public Transform[] spawnPoints;

    [Header("Oleadas (si está vacío se usan las de por defecto)")]
    public Wave[] waves;
    public int startWaveIndex = 0;

    [Header("Velocidad (nivel de movimiento)")]
    public float speedPerLevel = 2f;
    public float maxSpeedLevel = 12f;

    [Header("Progresión dentro de la última oleada")]
    public float difficultyStepSeconds = 8f;
    public float speedLevelIncrease = 0.5f;
    [Range(0.1f, 1f)] public float intervalMultiplier = 0.92f;
    public float minSpawnInterval = 0.5f;

    [Header("Cubos especiales")]
    public float switchDuration = 8f;
    public float speedEffectDuration = 6f;
    public float slowMultiplier = 0.5f;
    public float fastMultiplier = 1.7f;

    private static HashSet<GameObject> spawnedCubes = new HashSet<GameObject>();

    private GameObject gunPrefab;
    private GameObject swordPrefab;
    private float currentSpeedLevel;
    private float currentInterval;
    private Coroutine speedEffectRoutine;

    void Awake()
    {
        Instance = this;
        GameSpeed = 1f;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        GameSpeed = 1f;
    }

    void Start()
    {
        spawnedCubes.RemoveWhere(c => c == null);

        foreach (GameObject prefab in cubePrefabs)
        {
            if (prefab == null)
                continue;
            if (gunPrefab == null && prefab.GetComponent<GunCube>() != null)
                gunPrefab = prefab;
            else if (swordPrefab == null && prefab.GetComponent<SwordCube>() != null)
                swordPrefab = prefab;
        }

        if (waves == null || waves.Length == 0)
            waves = CreateDefaultWaves();

        StartCoroutine(WaveLoop());
    }

    Wave[] CreateDefaultWaves()
    {
        return new Wave[]
        {
            new Wave
            {
                name = "Wave 1 - Introducción",
                duration = 15f,
                spawnInterval = 3.5f,
                speedLevel = 3f,
                gunWeight = 1f
            },
            new Wave
            {
                name = "Wave 2 - Combinación",
                duration = 20f,
                spawnInterval = 2.2f,
                speedLevel = 4.5f,
                doubleSpawnChance = 0.35f,
                gunWeight = 1f,
                swordWeight = 1f
            },
            new Wave
            {
                name = "Wave 3 - Ritmo y cubos nuevos",
                duration = 0f,
                spawnInterval = 1.4f,
                speedLevel = 6.5f,
                doubleSpawnChance = 0.4f,
                startWithSwitchCube = true,
                gunWeight = 1f,
                swordWeight = 1f,
                switchWeight = 0.25f,
                slowWeight = 0.2f,
                fastWeight = 0.2f
            }
        };
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
        speedEffectRoutine = null;
        GameSpeed = 1f;
    }

    IEnumerator WaveLoop()
    {
        for (int i = Mathf.Clamp(startWaveIndex, 0, waves.Length - 1); i < waves.Length; i++)
        {
            Wave wave = waves[i];
            bool firstSpawn = true;
            bool endless = i == waves.Length - 1 || wave.duration <= 0f;

            currentSpeedLevel = wave.speedLevel;
            currentInterval = wave.spawnInterval;
            Debug.Log("Oleada " + (i + 1) + ": " + wave.name);

            float elapsed = 0f;
            float stepTimer = 0f;

            while (endless || elapsed < wave.duration)
            {
                SpawnGroup(wave, firstSpawn && wave.startWithSwitchCube);
                firstSpawn = false;

                float wait = currentInterval;
                yield return new WaitForSeconds(wait);
                elapsed += wait;

                if (endless)
                {
                    stepTimer += wait;
                    if (stepTimer >= difficultyStepSeconds)
                    {
                        stepTimer = 0f;
                        currentSpeedLevel = Mathf.Min(currentSpeedLevel + speedLevelIncrease, maxSpeedLevel);
                        currentInterval = Mathf.Max(currentInterval * intervalMultiplier, minSpawnInterval);
                    }
                }
            }
        }
    }

    void SpawnGroup(Wave wave, bool forceSwitch)
    {
        if (spawnPoints.Length == 0)
            return;

        int first = Random.Range(0, spawnPoints.Length);
        SpawnCube(forceSwitch && !WeaponSwap.IsSwapped ? CubeType.Switch : PickType(wave), spawnPoints[first]);

        if (spawnPoints.Length > 1 && Random.value < wave.doubleSpawnChance)
        {
            int second = (first + Random.Range(1, spawnPoints.Length)) % spawnPoints.Length;
            SpawnCube(PickType(wave), spawnPoints[second]);
        }
    }

    CubeType PickType(Wave wave)
    {
        float switchWeight = WeaponSwap.IsSwapped ? 0f : wave.switchWeight;
        float total = wave.gunWeight + wave.swordWeight + switchWeight + wave.slowWeight + wave.fastWeight;
        if (total <= 0f)
            return CubeType.Gun;

        float roll = Random.value * total;
        if ((roll -= wave.gunWeight) < 0f) return CubeType.Gun;
        if ((roll -= wave.swordWeight) < 0f) return CubeType.Sword;
        if ((roll -= switchWeight) < 0f) return CubeType.Switch;
        if ((roll -= wave.slowWeight) < 0f) return CubeType.Slow;
        return CubeType.Fast;
    }

    GameObject PrefabFor(CubeType type)
    {
        switch (type)
        {
            case CubeType.Gun:
                return gunPrefab;
            case CubeType.Switch:
                return switchCubePrefab != null ? switchCubePrefab : swordPrefab;
            case CubeType.Slow:
                return slowCubePrefab != null ? slowCubePrefab : swordPrefab;
            case CubeType.Fast:
                return fastCubePrefab != null ? fastCubePrefab : swordPrefab;
            default:
                return swordPrefab;
        }
    }

    void SpawnCube(CubeType type, Transform point)
    {
        GameObject prefab = PrefabFor(type);
        if (prefab == null)
            return;

        GameObject cube = Instantiate(prefab, point.position, point.rotation);
        spawnedCubes.Add(cube);

        if (type != CubeType.Gun && type != CubeType.Sword)
            Debug.Log("Cubo especial creado: " + type);

        CubePhysics mover = cube.AddComponent<CubePhysics>();
        mover.Init(point.forward, currentSpeedLevel, speedPerLevel);

        SwordCube swordCube = cube.GetComponent<SwordCube>();
        if (swordCube == null)
            return;

        switch (type)
        {
            case CubeType.Switch:
                swordCube.ConfigureEffect(CubeEffect.Switch, switchDuration, 1f);
                break;
            case CubeType.Slow:
                swordCube.ConfigureEffect(CubeEffect.Slow, speedEffectDuration, slowMultiplier);
                break;
            case CubeType.Fast:
                swordCube.ConfigureEffect(CubeEffect.Fast, speedEffectDuration, fastMultiplier);
                break;
        }
    }

    public static void ApplySpeedEffect(float multiplier, float duration)
    {
        if (Instance != null)
            Instance.StartSpeedEffect(multiplier, duration);
    }

    void StartSpeedEffect(float multiplier, float duration)
    {
        if (speedEffectRoutine != null)
            StopCoroutine(speedEffectRoutine);
        speedEffectRoutine = StartCoroutine(SpeedEffect(multiplier, duration));
    }

    IEnumerator SpeedEffect(float multiplier, float duration)
    {
        GameSpeed = multiplier;
        yield return new WaitForSeconds(duration);
        GameSpeed = 1f;
        speedEffectRoutine = null;
    }

    public static GameObject GetSpawnedCube(Transform t)
    {
        while (t != null)
        {
            if (spawnedCubes.Contains(t.gameObject))
                return t.gameObject;
            t = t.parent;
        }
        return null;
    }

    public static void DestroyCube(GameObject cube, float delay = 0f)
    {
        spawnedCubes.Remove(cube);
        Destroy(cube, delay);
    }

    public static void DestroyAllCubes()
    {
        foreach (GameObject cube in spawnedCubes)
        {
            if (cube != null)
                Destroy(cube);
        }
        spawnedCubes.Clear();
    }
}
