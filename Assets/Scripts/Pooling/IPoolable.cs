using System;

public interface IPoolable
{
	event Action OnDestroy;
	void Reset();
}
