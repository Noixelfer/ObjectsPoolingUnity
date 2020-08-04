using UnityEngine;

public class TurretBullet : MonoBehaviour
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
		if ((transform.position - initialPosition).sqrMagnitude >=  MAX_DISTANCE_SQUARED)
		{
			Destroy(gameObject);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		Destroy(gameObject);
	}
}
