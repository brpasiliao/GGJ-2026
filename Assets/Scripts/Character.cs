using UnityEngine;


public class Character : MonoBehaviour {
    private static Character instance;
    public static Character Instance => instance;

    [SerializeField] private float speed;
	private Vector3 facingVector;

	[SerializeField] private GameObject ammoPrefab;
	private int ammoCount;
	private int health;


	private void Awake() {
        instance = this;
		facingVector = Vector3.down;
	}

	private void Update() {
		HandleMovement();
		HandleShooting();
		Debug();
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

	private void HandleShooting() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			if (ammoCount > 0) {
				ShootAmmo();
			}
		}
	}

	private void Debug() {
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
