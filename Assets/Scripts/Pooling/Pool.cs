using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public enum PoolSizeType
{
	Fixed,
	Flexible
}

[System.Serializable]
public class Pool
{
	public string Id { get; set; }
	public string PrefabPath { get; set; }
	public PoolSizeType PoolSizeType { get; set; } = PoolSizeType.Fixed;
	public int InitialSize { get; set; } = 10;
	public int MaximumSize { get; set; } = 100;
	public bool Prewarm { get; set; } = false;
	public int PrewarmPriority = 1;

	[JsonIgnore] public Queue<PoolableObject> PoolObjects { get; } = new Queue<PoolableObject>();
	private PoolableObject prefab;
	private bool prefabLoaded = false;
	public Transform PooledObjectsContainer { get; private set; }

	public void SetPooledObjectsContainer(Transform container)
	{
		PooledObjectsContainer = container;
	}

	public PoolableObject Get()
	{
		if (PoolObjects.Count > 0)
		{
			var pooledObject = PoolObjects.Dequeue();
			pooledObject.ResetState();
			return pooledObject;
		}

		return CreateNewPoolableObject();
	}

	public PoolableObject CreateNewPoolableObject()
	{
		if (!prefabLoaded)
		{
			prefab = Resources.Load<PoolableObject>(PrefabPath);
			prefabLoaded = true;
		}

		if (prefab == null)
		{
			Debug.LogError($"The prefab path {PrefabPath} fromt he pool with id {Id} could not be loaded!");
			return null;
		}

		var newObject = MonoBehaviour.Instantiate(prefab, PooledObjectsContainer);
		newObject.OnDestroy += () => { ReuseObject(newObject); };
		return newObject;
	}

	private void ReuseObject(PoolableObject pooledObject)
	{
		if (PoolSizeType == PoolSizeType.Fixed && PoolObjects.Count >= MaximumSize)
		{
			MonoBehaviour.Destroy(pooledObject);
		}
		else
		{
			pooledObject.gameObject.SetActive(false);
			pooledObject.transform.parent = PooledObjectsContainer;
			PoolObjects.Enqueue(pooledObject);
		}
	}
}