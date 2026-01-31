using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	[SerializeField] private Collider2D trigger;
	[SerializeField] private float maxDistance;

	private IShootable owner;
	private Vector3 direction;
	private float speed;
	private float distanceTraveled;


	public void Initialize(Vector3 direction, float speed, IShootable owner) {
		this.direction = direction;
		this.speed = speed;
		this.owner = owner;
		distanceTraveled = 0;
	}

	private void Update() {
		Vector3 target = direction + transform.position;
		Vector3 originalPosition = transform.position;
		transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
		distanceTraveled += Vector3.Distance(originalPosition, transform.position);

		if (distanceTraveled > maxDistance) {
			Destroy(gameObject);
			return;
		}

		List<Collider2D> colliders = new();
		trigger.Overlap(colliders);
		foreach (Collider2D collider in colliders) {
			if (collider.TryGetComponent(out IShootable shootable) && shootable != owner) {
				shootable.GetShot();
				Destroy(gameObject);
			}
		}
	}
}
