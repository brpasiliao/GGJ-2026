using UnityEngine;

public class MainRoomManager : MonoBehaviour {
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private Vector3 startingPosition;

    private static bool initializedGame = false;


    private void Start() {
        if (!initializedGame) {
			Instantiate(characterPrefab);
			Instantiate(gameManagerPrefab);
            initializedGame = true;
		}

        Character.Instance.transform.position = startingPosition;
    }
}
