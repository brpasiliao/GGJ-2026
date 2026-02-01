using UnityEngine;
using System;
using System.Collections.Generic;


public class Character : MonoBehaviour, IShootable {
    private static Character instance;
    public static Character Instance => instance;

	[SerializeField] private Collider2D bodyCollider;
	[SerializeField] private Collider2D interactTrigger;
	private List<Collider2D> interactableColliders;

	[SerializeField] private Transform weaponPivot;
	[SerializeField] private Collider2D weaponTrigger;
	[SerializeField] private SpriteRenderer weaponGraphics;

    [SerializeField] private float speed;
	private Vector3 facingAngle;

	private int level;
	public int Level => level;

	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private int bulletSpeed;
	private int bulletCount;
	private int health;


	private void Awake() {
		DontDestroyOnLoad(this);
        instance = this;
		interactableColliders = new();
		facingAngle = Vector3.down;
		level = 2;
		UpdateBulletCount(0);
		health = 1;
	}

	public void ChangePosition(Vector3 newPosition) {
		transform.position = newPosition;
	}

	private void Update() {
		HandleMovement();
		HandleInteract();
		HandleAiming();
		HandleCollecting();
		HandleDebug();
	}

	private void HandleMovement() {
		Vector3 currentVector = Vector3.zero;

		if (Input.GetKey(KeyCode.W)) {
			currentVector.y += 1f;
		}
		if (Input.GetKey(KeyCode.A)) {
			currentVector.x -= 1f;
		}
		if (Input.GetKey(KeyCode.S)) {
			currentVector.y -= 1f;
		}
		if (Input.GetKey(KeyCode.D)) {
			currentVector.x += 1f;
		}

		Vector3 normalizedVector = currentVector.normalized;
		Vector3 target = normalizedVector + transform.position;
		transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
		facingAngle = normalizedVector == Vector3.zero ? facingAngle : normalizedVector;
	}

	private void HandleInteract() {
		interactableColliders.RemoveAll(collider => collider == null);

		List<Collider2D> colliders = new ();
		interactTrigger.Overlap(colliders);
		foreach (Collider2D collider in colliders) {
			if (collider.TryGetComponent(out IInteractable interactable)) {
				interactable.StartInteract();
				if (!interactableColliders.Contains(collider)) {
					interactableColliders.Add(collider);
				}

				if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Space)) {
					interactable.Interact();
				}
			}
		}

		foreach (Collider2D collider in interactableColliders) {
			if (!colliders.Contains(collider)) {
				IInteractable interactable = collider.GetComponent<IInteractable>();
				interactable.StopInteract();
			}
		}
		interactableColliders.RemoveAll(collider => !colliders.Contains(collider));
	}

	private void HandleAiming() {
		float distanceFromCamera = 10f; // how far in front of camera
		Vector3 mouseScreenPosition = Input.mousePosition;
		mouseScreenPosition.z = distanceFromCamera;
		Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

		Vector3 direction = mouseWorldPosition - transform.position;
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		weaponPivot.rotation = Quaternion.Euler(0, 0, angle - 90f);

		HandleShooting(direction.normalized);
	}

	private void HandleShooting(Vector3 direction) {
		if (Input.GetKeyDown(KeyCode.Space)) {
			if (bulletCount > 0) {
				ShootBullet(direction);
			}
		}
	}

	public void ShootBullet(Vector3 direction) {
		if (bulletCount <= 0) {
			return;
		}

		GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
		CharacterBullet bullet = bulletObject.GetComponent<CharacterBullet>();
		bullet.Initialize(direction, bulletSpeed, bulletCount);
		UpdateBulletCount(0);
	}
	// CHANGE FACING ANGLE TO BE FROM MOUSE INPUT

	public bool GetShot() {
		health--;
		if (health <= 0) {
			Debug.Log("lose");
			// lose game;
		}
		return true;
	}

	private void HandleCollecting() {
		List<Collider2D> colliders = new();
		weaponTrigger.Overlap(colliders);
		foreach (Collider2D collider in colliders) {
			if (collider.TryGetComponent(out BossBullet bullet)) {
				CollectBullet(bullet);
			}
		}
	}

	public void CollectBullet(BossBullet bullet) {
		if (bulletCount + 1 > 3) {
			return;
		}
		UpdateBulletCount(bulletCount + 1);

		if (bullet != null) {
			bullet.GetShot();
		}
	}

	private void UpdateBulletCount(int count) {
		Debug.Log("bullet count " + bulletCount);
		bulletCount = count;

		if (bulletCount == 3) {
			Color color = weaponGraphics.color;
			color.a = 0.2f;
			weaponGraphics.color = color;
		} else {
			weaponGraphics.color = Color.white;
		}
	}

	private void HandleDebug() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			CollectBullet(null);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			bodyCollider.enabled = !bodyCollider.enabled;
		}
	}
}
