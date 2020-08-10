using UnityEngine;

public class TurretBullet : PoolableObject
{
	private const float MAX_DISTANCE_SQUARED = 900f;

	public Rigidbody Rigidbody { get; private set; }

	private Vector3 initialPosition = Vector3.zero;

	private void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
		initialPosition = transform.position;
	}

	private void FixedUpdate()
	{
		if ((transform.position - initialPosition).sqrMagnitude >= MAX_DISTANCE_SQUARED)
		{
			InvokeOnDestory();
		}
	}

	public override void ResetState()
	{
		base.ResetState();
		Rigidbody.velocity = Vector3.zero;
		Rigidbody.angularVelocity = Vector3.zero;
	}

	private void OnCollisionEnter(Collision collision)
	{
		InvokeOnDestory();
	}
}
