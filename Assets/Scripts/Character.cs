using UnityEngine;
using System;
using System.Collections.Generic;


public class Character : MonoBehaviour, IShootable {
    private static Character instance;
    public static Character Instance => instance;

	[SerializeField] private Collider2D interactTrigger;
	private List<Collider2D> interactableColliders;

    [SerializeField] private float speed;
	private Vector3 facingVector;

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
		facingVector = Vector3.down;
		level = 0;
		bulletCount = 0;
		health = 1;
	}

	public void ChangePosition(Vector3 newPosition) {
		transform.position = newPosition;
	}

	private void Update() {
		HandleMovement();
		HandleInteract();
		HandleShooting();
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
		facingVector = normalizedVector == Vector3.zero ? facingVector : normalizedVector;
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

	private void HandleShooting() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			if (bulletCount > 0) {
				ShootBullet();
			}
		}
	}

	private void HandleDebug() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			ObtainBullet();
		}
		if (Input.GetKeyDown(KeyCode.M)) {
			interactTrigger.enabled = !interactTrigger.enabled;
		}
	}

	public void ObtainBullet() {
		bulletCount++;
	}

	public void ShootBullet() {
		bulletCount--;
		GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
		Bullet bullet = bulletObject.GetComponent<Bullet>();
		bullet.Initialize(facingVector, bulletSpeed, this);
	}

	public bool GetShot() {
		health--;
		if (health <= 0) {
			Debug.Log("lose");
			// lose game;
		}
		return true;
	}
}
