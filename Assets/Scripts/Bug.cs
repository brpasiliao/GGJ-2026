using UnityEngine;
using System.Collections.Generic;

public class Bug : MonoBehaviour {

	[SerializeField] private Collider2D trigger;
	[SerializeField] private float startDelay;
	[SerializeField] private float recordTime;

	private Vector3 characterOriginalPosition;
	private float startDistance;
    private List<Vector3> positions;
	private bool following;

	private float timer;


	private void Awake() {
		following = false;
		positions = new List<Vector3>();
	}

	private void Start() {
		characterOriginalPosition = Character.Instance.transform.position;
		startDistance = Vector3.Distance(transform.position, characterOriginalPosition);
	}

	private void Update() {
		timer += Time.deltaTime;
		if (timer >= recordTime) {
			positions.Add(Character.Instance.transform.position);

			if (following) {
				transform.position = positions[0];
				RotateTowards(positions[0]);
				positions.RemoveAt(0);
			}
		}

		if (!following) {
			float speed = startDistance / startDelay * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, characterOriginalPosition, speed);
			RotateTowards(characterOriginalPosition);

			float distance = Vector3.Distance(transform.position, characterOriginalPosition);
			if (distance < 0.05) {
				following = true;
			}
		}

		List<Collider2D> colliders = new();
		trigger.Overlap(colliders);
		foreach (Collider2D collider in colliders) {
			if (collider.TryGetComponent(out IShootable shootable)) {
				Component component = shootable as Component;
				if (component.GetType() == typeof(Character)) {
					shootable.GetShot();
				}
			}
		}
	}

	private void RotateTowards(Vector3 target) {
		Vector3 direction = target - transform.position;
		float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		float angle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, Time.deltaTime);
		transform.rotation = Quaternion.Euler(0f, 0f, angle);
	}
}
