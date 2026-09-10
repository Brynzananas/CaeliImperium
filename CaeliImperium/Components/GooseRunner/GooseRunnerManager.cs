using CaeliImperium.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaeliImperium.Components.GooseRunner;

public class GooseRunnerManager : MonoBehaviour
{
    public static GooseRunnerManager instance { get; private set; }

    [Header("Lane")]
    public static int fallBackLaneCount = 5;
    public int laneCount = 5;
    public float laneWidth = 2f;

    [Header("Speed control")]
    public float baseSpeed = 8f;
    public float maxSpeed = 22f;
    public float speedIncreasePerSecond = 0.05f;

    [Header("Prefabs")]
    public GooseRunnerTile[] segmentPrefabs;
    public GameObjectsArrayHolderDef slidePrefabsHolder;
    public GameObjectsArrayHolderDef jumpPrefabsHolder;
    public GameObjectsArrayHolderDef blockedPrefabsHolder;

    [Header("Setup")]
    public int initialSegments = 5;
    public float lookaheadDistance = 60f;
    public float recycleBuffer = 15f;

    [Header("Row layout")]
    public float rowSpacing = 6f;
    public float startSafeZone = 8f;
    [Range(0f, 1f)] public float emptyRowChance = 0.15f;

    [Header("Difficulty ramp")]
    public float difficultyRampDistance = 1000f;

    [Header("Patterns")]
    public List<RowPattern> rowPatterns = new List<RowPattern>();

    [Header("Coin")]
    public GameObject coinPrefab;
    public float groundHeight = 0.5f;
    public float jumpArcHeight = 1.8f;

    [Header("Other values")]
    public float currentSpeed;
    public float distanceTravelled;
    public bool gameOver;

    public event Action OnGameOver;
    public event Action<int> OnCoinsChanged;

    private Queue<GooseRunnerTile> active = new Queue<GooseRunnerTile>();
    private Dictionary<string, Queue<GooseRunnerTile>> pools = new Dictionary<string, Queue<GooseRunnerTile>>();
    private float nextSpawnZ;

    public void Awake()
    {
        instance = this;
        SanitizePatterns();
    }

    public void Start()
    {
        currentSpeed = baseSpeed;
        gameOver = false;
        nextSpawnZ = transform.position.z;
        for (int i = 0; i < initialSegments; i++) SpawnSegment(i == 0);
    }

    public void FixedUpdate()
    {
        if (gameOver) return;
        currentSpeed = Mathf.Min(maxSpeed, currentSpeed + speedIncreasePerSecond * Time.fixedDeltaTime);
        distanceTravelled += currentSpeed * Time.fixedDeltaTime;
        GooseRunnerPlayerController gooseRunnerPlayerController = GooseRunnerPlayerController.instance;
        if (!gooseRunnerPlayerController) return;
        while (nextSpawnZ - gooseRunnerPlayerController.transform.position.z < lookaheadDistance) SpawnSegment(false);
        while (active.Count > 0)
        {
            GooseRunnerTile oldest = active.Peek();
            float segmentEndZ = oldest.transform.position.z + oldest.length;
            if (segmentEndZ < gooseRunnerPlayerController.transform.position.z - recycleBuffer)
            {
                active.Dequeue();
                oldest.ClearSpawned();
                ReturnToPool(oldest);
            }
            else
            {
                break;
            }
        }
    }
    public void GameOver()
    {
        if (gameOver) return;
        gameOver = true;
        gameObject.SetActive(false);
        if (GooseRunnerCameraController.instance) GooseRunnerCameraController.instance.gameObject.SetActive(false);
        if (GooseRunnerPlayerController.instance) GooseRunnerPlayerController.instance.gameObject.SetActive(false);
        OnGameOver?.Invoke();
    }
    public float GetLaneX(int laneIndex)
    {
        float middle = (laneCount - 1) / 2f;
        return (laneIndex - middle) * laneWidth;
    }
    public int ClampLane(int lane) => Mathf.Clamp(lane, 0, laneCount - 1);

    public void SpawnSegment(bool isFirst)
    {
        GooseRunnerTile gooseRunnerTile = segmentPrefabs[UnityEngine.Random.Range(0, segmentPrefabs.Length)];
        GooseRunnerTile gooseRunnerTileClone = GetFromPool(gooseRunnerTile);
        gooseRunnerTileClone.transform.SetPositionAndRotation(new Vector3(0f, 0f, nextSpawnZ), Quaternion.identity);
        gooseRunnerTileClone.gameObject.SetActive(true);
        nextSpawnZ += gooseRunnerTileClone.length;
        active.Enqueue(gooseRunnerTileClone);
        if (!isFirst)
        {
            List<SpawnedRow> spawnedRows = PopulateSegmentWithObstacles(gooseRunnerTileClone);
            PopulateSegmentWithCoins(gooseRunnerTileClone, spawnedRows);
        }
    }

    public GooseRunnerTile GetFromPool(GooseRunnerTile prefab)
    {
        if (!pools.TryGetValue(prefab.name, out var queue))
        {
            queue = new Queue<GooseRunnerTile>();
            pools[prefab.name] = queue;
        }
        if (queue.Count > 0)
            return queue.Dequeue();
        GooseRunnerTile instance = Instantiate(prefab, transform);
        instance.name = prefab.name;
        return instance;
    }
    public void ReturnToPool(GooseRunnerTile segment)
    {
        segment.gameObject.SetActive(false);
        if (!pools.TryGetValue(segment.name, out var queue))
        {
            queue = new Queue<GooseRunnerTile>();
            pools[segment.name] = queue;
        }
        queue.Enqueue(segment);
    }
    public enum LaneState { Clear, Slide, Jump, Blocked }

    [Serializable]
    public class RowPattern
    {
        public LaneState[] lanes = instance ? new LaneState[instance.laneCount] : new LaneState[fallBackLaneCount];

        [Tooltip("Relative chance this pattern gets picked.")]
        [Range(1, 10)] public int weight = 1;

        [Tooltip("0 = easy/early game, higher = only shows up as the run speeds up.")]
        [Range(0, 5)] public int difficulty = 0;
        public bool IsSafe() => lanes.Any(l => l == LaneState.Clear);
    }
    public struct SpawnedRow
    {
        public float localZ;
        public RowPattern pattern;
    }
    public void ResetPatterns() => rowPatterns = BuildDefaultPatterns();
    public void SanitizePatterns()
    {
        foreach (RowPattern rowPattern in rowPatterns)
        {
            if (rowPattern.lanes == null || rowPattern.lanes.Length != laneCount) rowPattern.lanes = new LaneState[laneCount];
            if (!rowPattern.IsSafe()) rowPattern.lanes[UnityEngine.Random.Range(0, laneCount)] = LaneState.Clear;
        }
        if (rowPatterns.Count == 0) rowPatterns = BuildDefaultPatterns();
    }
    public List<SpawnedRow> PopulateSegmentWithObstacles(GooseRunnerTile segment)
    {
        var rows = new List<SpawnedRow>();
        float z = startSafeZone;
        while (z < segment.length - 2f)
        {
            if (UnityEngine.Random.value < emptyRowChance)
            {
                z += rowSpacing;
                continue;
            }
            RowPattern pattern = PickWeightedPattern();
            SpawnRow(segment, z, pattern);
            rows.Add(new SpawnedRow { localZ = z, pattern = pattern });
            z += rowSpacing;
        }
        return rows;
    }
    public RowPattern PickWeightedPattern()
    {
        float t = 0f;
        if (difficultyRampDistance > 0f) t = Mathf.Clamp01(distanceTravelled / difficultyRampDistance);
        float totalWeight = 0f;
        var effectiveWeights = new float[rowPatterns.Count];
        for (int i = 0; i < rowPatterns.Count; i++)
        {
            float weight = rowPatterns[i].weight * (1f + rowPatterns[i].difficulty * t);
            effectiveWeights[i] = weight;
            totalWeight += weight;
        }
        float roll = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;
        for (int i = 0; i < rowPatterns.Count; i++)
        {
            cumulative += effectiveWeights[i];
            if (roll <= cumulative) return rowPatterns[i];
        }
        return rowPatterns[rowPatterns.Count - 1];
    }
    public void SpawnRow(GooseRunnerTile gooseRunnerTile, float localZ, RowPattern rowPattern)
    {
        for (int lane = 0; lane < laneCount; lane++)
        {
            LaneState laneState = rowPattern.lanes[lane];
            if (laneState == LaneState.Clear) continue;
            GameObject prefab = GetPrefabForLaneState(laneState);
            if (!prefab) continue;
            float laneX = GetLaneX(lane);
            Vector3 localPos = new Vector3(laneX, 0f, localZ);
            Vector3 worldPos = gooseRunnerTile.transform.TransformPoint(localPos);
            GameObject gameObject = Instantiate(prefab, worldPos, gooseRunnerTile.transform.rotation, gooseRunnerTile.transform);
            //gameObject.transform.localScale = new Vector3(laneX, 1f, 1f);
            GooseRunnerObstacle gooseRunnerObstacle = gameObject.GetComponent<GooseRunnerObstacle>();
            if (gooseRunnerObstacle)
            {
                gooseRunnerObstacle.type = laneState switch
                {
                    LaneState.Slide => GooseRunnerObstacleType.Slide,
                    LaneState.Jump => GooseRunnerObstacleType.Jump,
                    LaneState.Blocked => GooseRunnerObstacleType.FullBlock,
                    _ => gooseRunnerObstacle.type
                };
            }
            gooseRunnerTile.RegisterSpawned(gameObject);
        }
    }
    public GameObject GetPrefabForLaneState(LaneState laneState)
    {
        GameObject[] pool = laneState switch
        {
            LaneState.Slide => slidePrefabsHolder.gameObjects,
            LaneState.Jump => jumpPrefabsHolder.gameObjects,
            LaneState.Blocked => blockedPrefabsHolder.gameObjects,
            _ => null
        };
        if (pool == null || pool.Length == 0) return null;
        return pool[UnityEngine.Random.Range(0, pool.Length)];
    }
    public List<RowPattern> BuildDefaultPatterns()
    {
        var patterns = new List<RowPattern>();
        LaneState[] states = [LaneState.Clear, LaneState.Jump, LaneState.Slide, LaneState.Blocked];
        void GenerateCombinations(LaneState[] currentLanes, int depth)
        {
            if (depth == laneCount)
            {
                int nonClearCount = currentLanes.Count(lane => lane != LaneState.Clear);
                int difficulty = nonClearCount;
                int weight = Math.Max(1, 6 - difficulty);

                patterns.Add(new RowPattern
                {
                    lanes = (LaneState[])currentLanes.Clone(),
                    weight = weight,
                    difficulty = difficulty
                });
                return;
            }
            foreach (var state in states)
            {
                currentLanes[depth] = state;
                GenerateCombinations(currentLanes, depth + 1);
            }
        }
        GenerateCombinations(new LaneState[5], 0);
        return patterns;
    }
    public List<RowPattern> BuildDefaultPatternsOld()
    {
        LaneState C = LaneState.Clear;
        LaneState J = LaneState.Jump;
        LaneState S = LaneState.Slide;
        LaneState B = LaneState.Blocked;
        return
            [
                new RowPattern { lanes = [C, C, C], weight = 6, difficulty = 0 },
                new RowPattern { lanes = [J, C, C], weight = 4, difficulty = 1 },
                new RowPattern { lanes = [C, C, J], weight = 4, difficulty = 1 },
                new RowPattern { lanes = [C, S, C], weight = 4, difficulty = 1 },
                new RowPattern { lanes = [S, C, S], weight = 3, difficulty = 2 },
                new RowPattern { lanes = [J, C, J], weight = 3, difficulty = 2 },
                new RowPattern { lanes = [B, C, S], weight = 2, difficulty = 3 },
                new RowPattern { lanes = [J, C, B], weight = 2, difficulty = 3 },
                new RowPattern { lanes = [B, S, C], weight = 2, difficulty = 4 },
                new RowPattern { lanes = [C, J, B], weight = 2, difficulty = 4 },
            ];
    }
    public void PopulateSegmentWithCoins(GooseRunnerTile gooseRunnerTile, List<SpawnedRow> spanedRows)
    {
        if (!coinPrefab) return;
        foreach (SpawnedRow spawnedRow in spanedRows)
        {
            for (int lane = 0; lane < laneCount; lane++)
            {
                LaneState laneState = spawnedRow.pattern.lanes[lane];
                float height;
                switch (laneState)
                {
                    case LaneState.Clear:
                        height = groundHeight;
                        break;
                    case LaneState.Jump:
                        height = jumpArcHeight;
                        break;
                    default:
                        continue;
                }
                Vector3 localPos = new Vector3(GetLaneX(lane), height, spawnedRow.localZ);
                Vector3 worldPos = gooseRunnerTile.transform.TransformPoint(localPos);
                GameObject gameObject = Instantiate(coinPrefab, worldPos, gooseRunnerTile.transform.rotation, gooseRunnerTile.transform);
                gooseRunnerTile.RegisterSpawned(gameObject);
            }
        }
    }
}

