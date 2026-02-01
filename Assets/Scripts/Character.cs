using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class Character : MonoBehaviour, IShootable {
    private static Character instance;
    public static Character Instance => instance;

	[SerializeField] private SpriteRenderer characterGraphics;
	[SerializeField] private Rigidbody2D characterRigidbody;
	[SerializeField] private Collider2D bodyCollider;
	[SerializeField] private Collider2D interactTrigger;
	private List<Collider2D> interactableColliders;

	[SerializeField] private Transform weaponPivot;
	[SerializeField] private Collider2D weaponTrigger;
	[SerializeField] private SpriteRenderer weaponGraphics;

	[SerializeField] private Animator characterAnimator;
	[SerializeField] private Animator weaponAnimator;

	[SerializeField] private float hitCooldown;
    [SerializeField] private float speed;
	private Vector2 directionVector;

	private int level;
	public int Level => level;

	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private int bulletSpeed;
	private int bulletCount;
	private int health;
	private bool invincible;
	private bool lost;


	private void Awake() {
		DontDestroyOnLoad(this);
        instance = this;
		interactableColliders = new();
		level = 0;
		UpdateBulletCount(0);
		health = 3;
		invincible = false;
		lost = false;
	}

	public void ChangePosition(Vector3 newPosition) {
		transform.position = newPosition;
	}

	private void Update() {
		HandleMovementInput();
		HandleInteract();
		HandleAiming();
		HandleCollecting();
		HandleDebug();
	}

	private void FixedUpdate() {
		HandleMovement();
	}

	private void HandleMovementInput() {
		if (lost) {
			return;
		}

		directionVector = Vector2.zero;
		if (Input.GetKey(KeyCode.W)) {
			directionVector.y += 1f;
		}
		if (Input.GetKey(KeyCode.A)) {
			directionVector.x -= 1f;
		}
		if (Input.GetKey(KeyCode.S)) {
			directionVector.y -= 1f;
		}
		if (Input.GetKey(KeyCode.D)) {
			directionVector.x += 1f;
		}

		characterAnimator.SetInteger("directionX", (int)directionVector.x);
		characterAnimator.SetInteger("directionY", (int)directionVector.y);
	}

	private void HandleMovement() {
		if (lost) {
			return;
		}

		Vector2 step = directionVector.normalized * Time.fixedDeltaTime * speed;
		characterRigidbody.MovePosition(characterRigidbody.position + step);
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
		weaponAnimator.SetTrigger("shoot");
		UpdateBulletCount(0);
	}

	public bool GetShot() {
		if (invincible) {
			return false;
		}

		UpdateHealth(health - 1);
		StartCoroutine(HitCooldown());
		return true;
	}

	private void UpdateHealth(int health) {
		this.health = health;

		if (UIManager.Instance != null) {
			UIManager.Instance.UpdateCharacterHealth(health);
		}

		if (health <= 0) {
			Debug.Log("lose");
			lost = true;
			StartCoroutine(LoseSequence());
		}
	}

	private IEnumerator HitCooldown() {
		invincible = true;
		characterGraphics.color = Color.grey;
		yield return new WaitForSeconds(hitCooldown);
		invincible = false;
		characterGraphics.color = Color.white;
	}

	private IEnumerator LoseSequence() {
		characterAnimator.SetTrigger("lost");
		yield return new WaitForSeconds(3f);
		GameManager.Instance.LeaveRoom();
		lost = false;
	}

	public void Win() {
		level++;
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
		weaponAnimator.SetInteger("level", bulletCount);

		//if (bulletCount == 3) {
		//	Color color = weaponGraphics.color;
		//	color.a = 0.2f;
		//	weaponGraphics.color = color;
		//} else {
		//	weaponGraphics.color = Color.white;
		//}
	}

	private void HandleDebug() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			CollectBullet(null);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			bodyCollider.enabled = !bodyCollider.enabled;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3)) {
			level++;
			if (level >= 3) {
				level = 0;
			}
		}
	}
}
