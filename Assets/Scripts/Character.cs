using UnityEngine;
using System;
using System.Collections.Generic;


public class Character : MonoBehaviour {
    private static Character instance;
    public static Character Instance => instance;

	[SerializeField] private Collider2D interactTrigger;
	private List<Collider2D> interactableColliders;

    [SerializeField] private float speed;
	private Vector3 facingVector;

	[SerializeField] private GameObject ammoPrefab;
	private int ammoCount;
	private int health;


	private void Awake() {
        instance = this;
		interactableColliders = new();
		facingVector = Vector3.down;
		ammoCount = 0;
		health = 1;
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

		Vector3 normalizedVector = Vector3.Normalize(currentVector);
		Vector3 target = normalizedVector + transform.position;
		transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
		facingVector = normalizedVector == Vector3.zero ? facingVector : normalizedVector;
	}

	private void HandleInteract() {
		List<Collider2D> colliders = new ();
		interactTrigger.Overlap(colliders);

		foreach (Collider2D collider in colliders) {
			if (collider.TryGetComponent(out IInteractable interactable)) {
				interactable.StartInteract();
				if (!interactableColliders.Contains(collider)) {
					interactableColliders.Add(collider);
				}

				if (Input.GetKeyDown(KeyCode.E)) {
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
			if (ammoCount > 0) {
				ShootAmmo();
			}
		}
	}

	private void HandleDebug() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			GetAmmo();
		}
	}

	public void GetAmmo() {
		ammoCount++;
	}

	public void ShootAmmo() {
		ammoCount--;
		GameObject ammoObject = Instantiate(ammoPrefab, transform.position, Quaternion.identity);
		Ammo ammo = ammoObject.GetComponent<Ammo>();
		ammo.Initialize(facingVector);
	}
}
