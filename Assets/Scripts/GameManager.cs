using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	private static GameManager instance;
	public static GameManager Instance => instance;

	[SerializeField] private Pattern firstRoomStart;
	[SerializeField] private List<Pattern> firstRoomPatterns;
	[SerializeField] private Pattern secondRoomStart;
	[SerializeField] private List<Pattern> secondRoomPatterns;
	[SerializeField] private Pattern thirdRoomStart;
	[SerializeField] private List<Pattern> thirdRoomPatterns;

	private FearType currentFear;


	private void Awake() {
		DontDestroyOnLoad(this);
		instance = this;
	}

	public void EnterRoom(FearType fear) {
		currentFear = fear;
		SceneManager.LoadScene("Boss Room");
	}

	public void LeaveRoom() {
		SceneManager.LoadScene("Main Room");
	}

	public FearType GetFear() {
		return currentFear;
	}

	public List<Pattern> GetBulletPattern() {
		return Character.Instance.Level switch {
			0 => firstRoomPatterns,
			1 => secondRoomPatterns,
			2 => thirdRoomPatterns,
			_ => null,
		};
	}

	public Pattern GetStartBulletPattern() {
		return Character.Instance.Level switch {
			0 => firstRoomStart,
			1 => secondRoomStart,
			2 => thirdRoomStart,
			_ => new Pattern(),
		};
	}

}
