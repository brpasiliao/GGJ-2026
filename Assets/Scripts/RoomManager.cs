using System.Collections.Generic;
using UnityEngine;


public enum FearType {
	Bugs,
	Dark,
	TightSpaces
}

public class RoomManager : MonoBehaviour {

	[SerializeField] private GameObject normalRoom;
	[SerializeField] private GameObject smallerRoom;
	[SerializeField] private GameObject bug;
	[SerializeField] private Vector3 startingPosition;
	[SerializeField] private Boss boss;

    public void Awake() {
		FearType type = GameManager.Instance.GetFear();
		Pattern startPattern = GameManager.Instance.GetStartBulletPattern();
		List<Pattern> patterns = GameManager.Instance.GetBulletPattern();
		bool darkAbility = false;

		switch (type) {
			case FearType.Bugs:
				bug.SetActive(true);
				break;

			case FearType.Dark:
				darkAbility = true;
				break;

			case FearType.TightSpaces:
				normalRoom.SetActive(false);
				smallerRoom.SetActive(true);
				break;
		}

		boss.Initialize(startPattern, patterns, darkAbility);
		Character.Instance.ChangePosition(startingPosition);
	}
}
