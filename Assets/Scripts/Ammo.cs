using UnityEngine;

public class Bullet : MonoBehaviour {

	[SerializeField] private float maxDistance;

	private Vector3 direction;
	private float speed;
	private float distanceTraveled;


	public void Initialize(Vector3 direction, float speed) {
		this.direction = direction;
		this.speed = speed;
		distanceTraveled = 0;
	}

	private void Update() {
		Vector3 target = direction + transform.position;
		Vector3 originalPosition = transform.position;
		transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
		distanceTraveled += Vector3.Distance(originalPosition, transform.position);

		if (distanceTraveled > maxDistance) {
			Destroy(gameObject);
		}
	}
}
