using UnityEngine;

public class TurretController : MonoBehaviour
{
	private const string BULLET_PATH = "Prefabs/Bullet";
	private const float DELAY_BETWEEN_BULLETS = 60f / 1000f;
	private const float BULLET_VELOCITY = 20f;

	[SerializeField] private TurretCannon cannon1;
	[SerializeField] private TurretCannon cannon2;

	private bool turretActive = false;
	private float lastAttackTime = 0f;
	private TurretBullet turretBulletPrefab;

	private void Awake()
	{
		turretBulletPrefab = Resources.Load<TurretBullet>(BULLET_PATH);
	}

	private void Update()
	{
		RotateTowardsMouse();

		if (Input.GetKeyDown(KeyCode.Space))
		{
			turretActive = !turretActive;
		}

		if (turretActive)
		{
			if (Time.time - lastAttackTime >= DELAY_BETWEEN_BULLETS)
			{
				Attack();
				lastAttackTime = Time.time;
			}
		}
	}

	private void Attack()
	{
		var bullet1 = Instantiate(turretBulletPrefab, cannon1.CannonExit.position, Quaternion.identity);
		var bullet2 = Instantiate(turretBulletPrefab, cannon2.CannonExit.position, Quaternion.identity);

		bullet1.transform.up = cannon1.transform.forward;
		bullet2.transform.up = cannon2.transform.forward;

		bullet1.Rigidbody.AddForce(cannon1.transform.forward * BULLET_VELOCITY, ForceMode.Impulse);
		bullet2.Rigidbody.AddForce(cannon2.transform.forward * BULLET_VELOCITY, ForceMode.Impulse);
	}

	private void RotateTowardsMouse()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out var raycastInfo, 100f))
		{
			var direction = raycastInfo.point - transform.position;
			direction.y = 0;
			transform.rotation = Quaternion.LookRotation(direction);
		}
	}
}
