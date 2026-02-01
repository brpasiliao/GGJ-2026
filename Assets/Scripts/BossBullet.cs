using System.Collections.Generic;
using UnityEngine;

public class BossBullet : MonoBehaviour, IShootable {

	[SerializeField] private Collider2D trigger;
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
			return;
		}

		List<Collider2D> colliders = new();
		trigger.Overlap(colliders);
		foreach (Collider2D collider in colliders) {
			if (collider.TryGetComponent(out IShootable shootable)) {
				Component component = shootable as Component;
				if (component.GetType() != GetType() && component.GetType() != typeof(Boss)) {
					if (shootable.GetShot()) {
						Destroy(gameObject);
					}
				}
			}
		}
	}

	public bool GetShot() {
		Destroy(gameObject);
		return true;
	}
}
