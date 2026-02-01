using System.Collections.Generic;
using UnityEngine;


public enum FearType {
	Bugs,
	Dark,
	TightSpaces
}

public class RoomManager : MonoBehaviour {

	private static RoomManager instance;
	public static RoomManager Instance => instance;

	[SerializeField] private GameObject normalRoom;
	[SerializeField] private GameObject smallerRoom;
	[SerializeField] private GameObject bug;
	[SerializeField] private Vector3 startingPosition;
	[SerializeField] private Boss boss;

	[SerializeField] RuntimeAnimatorController bugController;
	[SerializeField] RuntimeAnimatorController darkController;
	[SerializeField] RuntimeAnimatorController spaceController;

    public void Awake() {
		instance = this;
		FearType type = GameManager.Instance.GetFear();
		Pattern startPattern = GameManager.Instance.GetStartBulletPattern();
		List<Pattern> patterns = GameManager.Instance.GetBulletPattern();
		bool darkAbility = false;
		RuntimeAnimatorController bossController = null;

		switch (type) {
			case FearType.Bugs:
				bug.SetActive(true);
				bossController = bugController;
				break;

			case FearType.Dark:
				darkAbility = true;
				bossController = darkController;
				break;

			case FearType.TightSpaces:
				normalRoom.SetActive(false);
				smallerRoom.SetActive(true);
				bossController = spaceController;
				break;
		}


		boss.Initialize(startPattern, patterns, darkAbility, bossController);
		Character.Instance.ChangePosition(startingPosition);
	}

	public void StopBug() {
		bug.SetActive(false);
	}
}
