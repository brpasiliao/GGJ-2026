using UnityEngine;


public class Character : MonoBehaviour {
    private static Character instance;
    public static Character Instance => instance;

    [SerializeField] private float speed;

    private Vector3 currentVector;


	private void Awake() {
        instance = this;
	}

	private void Update() {
		currentVector = Vector3.zero;

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

		Vector3 target = Vector3.Normalize(currentVector) + transform.position;
		transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
	}
}
