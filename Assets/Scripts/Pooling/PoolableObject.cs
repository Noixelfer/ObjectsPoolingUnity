using System;
using UnityEngine;

public abstract class PoolableObject : MonoBehaviour
{
	public event Action OnDestroy;
	protected void InvokeOnDestory()
	{
		OnDestroy?.Invoke();
	}

	public virtual void ResetState()
	{
		gameObject.SetActive(true);
	}
}
