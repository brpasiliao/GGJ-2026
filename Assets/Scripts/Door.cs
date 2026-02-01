using System.Collections.Generic;
using UnityEngine;


public class Door : MonoBehaviour, IInteractable {

	[SerializeField] private FearType fear;


	public void Interact() {
		GameManager.Instance.EnterRoom(fear);
    }

	public void StartInteract() {
		Debug.Log("start interact");
	}

	public void StopInteract() {
		Debug.Log("stop interact");
	}
}
