using System.Collections.Generic;
using UnityEngine;

public class CharacterBullet : MonoBehaviour, IShootable {

	[SerializeField] private float maxDistance;

	[SerializeField] private Collider2D triggerL1;
	[SerializeField] private Collider2D triggerL2;
	[SerializeField] private Collider2D triggerL3;

	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Sprite playerBulletSpriteL1;
	[SerializeField] private Sprite playerBulletSpriteL2;
	[SerializeField] private Sprite playerBulletSpriteL3;

	private Collider2D currentTrigger;

	private int level;
	private Vector3 direction;
	private float speed;
	private float distanceTraveled;


	public void Initialize(Vector3 direction, float speed, int level) {
		this.level = level;
		this.direction = direction;
		this.speed = speed;
		distanceTraveled = 0;

		UpdateBulletLevel();
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
		if (currentTrigger == null) {
			Debug.Log("current trigger null, level: " + level);
		}

		List<Collider2D> colliders = new();
		currentTrigger.Overlap(colliders);
		foreach (Collider2D collider in colliders) {
			if (collider.TryGetComponent(out IShootable shootable)) {
				Component component = shootable as Component;
				if (component.GetType() != GetType() && component.GetType() != typeof(Character)) {
					if (shootable.GetShot()) {
						UpdateBulletLevel();
					}
				}
			}
		}
	}

	public bool GetShot() {
		level--;
		if (level <= 0) {
			Destroy(gameObject);
			return true;
		}

		UpdateBulletLevel();
		return true;
	}

	private void UpdateBulletLevel() {
		currentTrigger = level switch {
			1 => triggerL1,
			2 => triggerL2,
			3 => triggerL3,
			_ => null,
		};
		triggerL1.enabled = currentTrigger == triggerL1;
		triggerL2.enabled = currentTrigger == triggerL2;
		triggerL3.enabled = currentTrigger == triggerL3;

		spriteRenderer.sprite = level switch {
			1 => playerBulletSpriteL1,
			2 => playerBulletSpriteL2,
			3 => playerBulletSpriteL3,
			_ => null,
		};
	}
}
