using UnityEngine;


public class Person : MonoBehaviour, IInteractable {
    public void Interact() {
		Debug.Log("interacted");
    }

	public void StartInteract() {
		Debug.Log("start interact");
	}

	public void StopInteract() {
		Debug.Log("stop interact");
	}
}
