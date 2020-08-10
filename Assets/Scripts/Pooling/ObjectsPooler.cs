using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ObjectsPooler : MonoBehaviour
{
	private const int MAX_OBJECTS_PER_FRAME = 388;
	private const bool VERBOSE = true;
	private const float DELAY_BETWEEN_LOGS = 1f;
	private static ObjectsPooler instance;

	public static ObjectsPooler Instance
	{
		get
		{
			if (instance == null)
			{
				instance = GameObject.FindObjectOfType<ObjectsPooler>() ?? new GameObject("ObjectsPooler").AddComponent<ObjectsPooler>();
			}

			return instance;
		}
	}
	public PoolableObject Get(string id, Transform newParent, bool resetTransform = false)
	{
		var poolableObject = Get(id);
		if (poolableObject != null)
		{
			poolableObject.transform.parent = newParent;

			if (resetTransform)
			{
				poolableObject.transform.localPosition = Vector3.zero;
				poolableObject.transform.localRotation = Quaternion.identity;
			}
		}

		return poolableObject;
	}

	public PoolableObject Get(string id, Vector3 position, Quaternion rotation)
	{
		var poolableObject = Get(id);
		if (poolableObject != null)
		{
			poolableObject.transform.position = position;
			poolableObject.transform.rotation = rotation;
		}

		return poolableObject;
	}

	public PoolableObject Get(string id, Vector3 position)
	{
		var poolableObject = Get(id);
		if (poolableObject != null)
		{
			poolableObject.transform.position = position;
		}

		return poolableObject;
	}

	public PoolableObject Get(string id)
	{
		if (pools.ContainsKey(id))
		{
			return pools[id].Get();
		}

		return null;
	}

	private static Dictionary<string, Pool> pools = new Dictionary<string, Pool>();

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			DestroyImmediate(gameObject);
		}

		instance = this;

		var filePath = Path.Combine(Application.streamingAssetsPath, "pools.json");
		if (File.Exists(filePath))
		{
			var json = File.ReadAllText(filePath);
			pools = JsonConvert.DeserializeObject<Pool[]>(json).ToDictionary(pool => pool.Id);
		}

		SetPoolContainers();
		StartCoroutine(PrewarmPools());
	}

	private void SetPoolContainers()
	{
		foreach (var pool in pools.Values)
		{
			var container = new GameObject($"Pool {pool.Id}").transform;
			container.parent = transform;
			pool.SetPooledObjectsContainer(container);
		}
	}

	private IEnumerator PrewarmPools()
	{
		int startFrameCount;
		int endFrameCount;
		int prewarmedPrefabs = 0;
		var lastDebugTime = 0f;
		var stopwatch = new System.Diagnostics.Stopwatch();
		var orderedPools = new Stack<Pool>(pools.Values.OrderBy(v => v.PrewarmPriority));
		bool prewarmDone = false;
		Pool currentPool = null;
		PoolableObject currentPrefab = null;
		int i;
		int count = 0;

		startFrameCount = Time.frameCount;
		stopwatch.Start();

		if (orderedPools.Count == 0)
		{
			if (VERBOSE)
			{
				Debug.LogWarning("There are no pools...");
			}
		}
		else
		{
			if (MoveToNextValidPool(orderedPools, ref currentPool, ref currentPrefab, ref count))
			{
				while (!prewarmDone)
				{
					i = 0;
					while (i < MAX_OBJECTS_PER_FRAME || MAX_OBJECTS_PER_FRAME == -1)
					{
						if (orderedPools.Count == 0 && currentPool.PoolObjects.Count == currentPool.MaximumSize)
						{
							prewarmDone = true;
							break;
						}

						if (count <= 0)
						{
							if (!MoveToNextValidPool(orderedPools, ref currentPool, ref currentPrefab, ref count))
							{
								break;
							}
						}

						currentPool.CreateNewPoolableObject();
						count--;
						prewarmedPrefabs++;
						i++;
					}

					if (VERBOSE && Time.time - lastDebugTime > DELAY_BETWEEN_LOGS)
					{
						lastDebugTime = Time.time;
						Debug.Log($"Current prewarmed prefabs : {prewarmedPrefabs}");
					}
					yield return null;
				}

				stopwatch.Stop();
				if (VERBOSE)
				{
					Debug.Log($"Total prewarm time: {stopwatch.ElapsedMilliseconds} ms");
				}
			}
		}

		endFrameCount = Time.frameCount;
		if (VERBOSE)
		{
			Debug.Log($"Pooling prewarming took {endFrameCount - startFrameCount} frames");
		}
	}

	private bool MoveToNextValidPool(Stack<Pool> pool, ref Pool currentPool, ref PoolableObject currentGameObject, ref int count)
	{
		while (pool.Count > 0)
		{
			currentPool = pool.Pop();

			if (string.IsNullOrEmpty(currentPool.PrefabPath))
			{
				if (VERBOSE)
				{
					Debug.LogWarning($"The prefab path for the pool {currentPool.Id} was null or empty");
				}
			}
			else
			{
				currentGameObject = Resources.Load<PoolableObject>(currentPool.PrefabPath);

				if (currentGameObject == null)
				{
					if (VERBOSE)
					{
						Debug.LogWarning($"There was no prefab found at the path {currentPool.PrefabPath} from the pool {currentPool.Id}");
					}
				}
				else
				{
					count = currentPool.InitialSize;
					return true;
				}
			}
		}

		return false;
	}
}
