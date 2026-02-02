using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.UIElements;


public enum PatternType {
	Spiral,
	Shotgun,
	Window,
	Arcs,
	Random
}

[System.Serializable]
public struct Pattern {
	public PatternType type;
	public int rounds;
	public int variations;		// how rings change per round
	public int rays;			// groups/clusters in round if applicable
	public int angle;			// ray angles if applicable
	public int bulletCount;		// bullets in a ring per round; kind of works like density since ccould be empty in certain spots of the ring
	public float bulletSpeed;
	public float delay;			// delay between each round or bullet
}


public class Boss : MonoBehaviour, IShootable {
    [SerializeField] private SpriteRenderer graphics;
	[SerializeField] private Animator animator;
    [SerializeField] private GameObject bulletPrefab;

	[SerializeField] private int health;
	[SerializeField] private float hitCooldown;
	[SerializeField] private float invincibleDuration;
	[SerializeField] private float invincibleCooldown;
	[SerializeField] private float startDelay;
	[SerializeField] private float patternDelay;

	[SerializeField] private List<Pattern> testPatterns;
	private List<Pattern> patterns;
	private Pattern startPattern;
	private Pattern currentPattern;
	private Coroutine currentPatternRoutine;
	private Coroutine patternSequence;
	private Coroutine darkAbilityRoutine;
	private bool darkAbility;
	private bool invincible;
	private int originalHealth;


	public void Initialize(Pattern startPattern, List<Pattern> patterns, bool darkAbility, RuntimeAnimatorController controller) {
		this.patterns = patterns;
		this.startPattern = startPattern;
		this.darkAbility = darkAbility;
		currentPatternRoutine = null;
		patternSequence = null;
		darkAbilityRoutine = null;
		invincible = false;
		originalHealth = health;

		animator.runtimeAnimatorController = controller;
		Func<List<Pattern>, IEnumerator> routine = (list) => RunPatterns(list);
		patternSequence = StartCoroutine(routine(patterns));
	}

	private void CheckDefeat() {
		if (health <= 0) {
			Debug.Log("boss defeated");

			if (currentPatternRoutine != null) {
				StopCoroutine(currentPatternRoutine);
			}
			if (patternSequence != null) {
				StopCoroutine(patternSequence);
			}
			if (darkAbilityRoutine != null) {
				StopCoroutine(darkAbilityRoutine);
			}

			RoomManager.Instance.StopBug();
			Character.Instance.Win();
			StartCoroutine(DefeatSequence());
			//graphics.gameObject.SetActive(false);
		}
	}

	private IEnumerator DefeatSequence() {
		//animator.SetTrigger("defeated");
		yield return new WaitForSeconds(3f);
		GameManager.Instance.LeaveRoom();
	}

	public bool GetShot() {
		if (invincible) {
			return false;
		}

		UpdateHealth(health - 3);
		StartCoroutine(HitCooldown());
		return true;
	}

	private void UpdateHealth(int health) {
		this.health = health;
		CheckDefeat();
		if (UIManager.Instance != null) {
			UIManager.Instance.UpdateBossHealthBar((float)health / originalHealth);
		}
	}

	private IEnumerator HitCooldown() {
		invincible = true;
		graphics.color = Color.grey;
		yield return new WaitForSeconds(hitCooldown);
		invincible = false;
		graphics.color = Color.white;
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha7)) {
			UpdateHealth(1);
			Func<List<Pattern>, IEnumerator> routine = (list) => RunPatterns(list);
			patternSequence = StartCoroutine(routine(testPatterns));
		}
		if (Input.GetKeyDown(KeyCode.Alpha8)) {
			UpdateHealth(1);
			Func<List<Pattern>, IEnumerator> routine = (list) => RunPatterns(list);
			patternSequence = StartCoroutine(routine(patterns));
		}
		if (Input.GetKeyDown(KeyCode.Alpha9)) {
			UpdateHealth(0);
		}
		if (Input.GetKeyDown(KeyCode.Alpha0)) {
			GameManager.Instance.LeaveRoom();
		}

		Vector2 direction = Character.Instance.transform.position - transform.position;
		Vector2 discreteDirection = new Vector2(
			Mathf.Round(direction.x),
			Mathf.Round(direction.y)
		);
		animator.SetInteger("directionX", (int)discreteDirection.x);
		animator.SetInteger("directionY", (int)discreteDirection.y);
	}

	private IEnumerator DarkAbility() {
		while (true) {
			invincible = false;
			graphics.color = Color.white;
			yield return new WaitForSeconds(invincibleCooldown);
			invincible = true;
			graphics.color = Color.black;
			yield return new WaitForSeconds(invincibleDuration);
		}
	}

	private IEnumerator RunPatterns(List<Pattern> patternsList) {
		if (patternsList.Count == 0) {
			yield break;
		}

		yield return new WaitForSeconds(startDelay);
		if (darkAbility) darkAbilityRoutine = StartCoroutine(DarkAbility());
		yield return StartCoroutine(RunPattern(startPattern));

		while (true) {
			foreach (Pattern pattern in patternsList) {
				yield return StartCoroutine(RunPattern(pattern));
			}
		}
	}

	private IEnumerator RunPattern(Pattern pattern) {
		Debug.Log("run pattern " + pattern.type);
		yield return new WaitForSeconds(patternDelay);
		currentPattern = pattern;
		Func<IEnumerator> patternRoutine = GetRoutineFromEnum(pattern.type);
		currentPatternRoutine = StartCoroutine(patternRoutine());
		yield return currentPatternRoutine;
	}

	private IEnumerator ShotgunPattern() {
		for (int round = 0; round < currentPattern.rounds; round++) {
			int variation = round % currentPattern.variations;

			for (int count = 0; count < currentPattern.bulletCount; count++) {
				if (count % currentPattern.variations != variation) {
					continue;
				}

				float angleDegrees = 360f / currentPattern.bulletCount * count;
				FireBullet(angleDegrees);
			}
			yield return new WaitForSeconds(currentPattern.delay);
		}
	}

	private IEnumerator SpiralPattern() {
		for (int round = 0; round < currentPattern.rounds; round++) {
			for (int count = 0; count < currentPattern.bulletCount; count++) {
				float angleDegrees = 360f / currentPattern.bulletCount * count;
				FireBullet(angleDegrees);

				yield return new WaitForSeconds(currentPattern.delay);
			}
		}
	}

	private IEnumerator WindowPattern() {
		for (int round = 0; round < currentPattern.rounds; round++) {
			int variation = round % currentPattern.variations;

			List<float[]> ranges = new();
			for (int window = 0; window < currentPattern.rays; window++) {
				float angle = 360 / currentPattern.rays * window;
				float variatedAngle = angle + (360 / currentPattern.rays / currentPattern.variations * variation);
				ranges.Add(new float[2] { variatedAngle, variatedAngle + currentPattern.angle });
			}

			for (int count = 0; count < currentPattern.bulletCount; count++) {
				float angleDegrees = 360f / currentPattern.bulletCount * count;
				bool skip = false;
				foreach (float[] range in ranges) {
					if (angleDegrees > range[0] && angleDegrees < range[1]) {
						skip = true;
						break;
					}
				}
				if (skip) { continue; }

				FireBullet(angleDegrees);
			}
			yield return new WaitForSeconds(currentPattern.delay);
		}
	}

	private IEnumerator ArcsPattern() {
		for (int round = 0; round < currentPattern.rounds; round++) {
			float startAngle = 360f / currentPattern.rounds * round;
			float endAngle = startAngle + currentPattern.angle;

			for (int count = 0; count < currentPattern.bulletCount; count++) {
				float angleDegrees = 360f / currentPattern.bulletCount * count;
				angleDegrees += endAngle - 360f > angleDegrees ? 360 : 0;
				if (angleDegrees < startAngle || angleDegrees > endAngle) {
					continue;
				}

				FireBullet(angleDegrees);
			}
			yield return new WaitForSeconds(currentPattern.delay);
		}
	}

	private IEnumerator RandomPattern() {
		for (int count = 0; count < currentPattern.bulletCount; count++) {
			float angleDegrees = Random.Range(0, 360f);
			FireBullet(angleDegrees);

			yield return new WaitForSeconds(currentPattern.delay);
		}
	}

	private void FireBullet(float angleDegrees) {
		float angleRadians = angleDegrees * Mathf.Deg2Rad;
		Vector3 direction = new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians)).normalized;

		GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
		BossBullet bullet = bulletObject.GetComponent<BossBullet>();
		bullet.Initialize(direction, currentPattern.bulletSpeed);
	}

	private Func<IEnumerator> GetRoutineFromEnum(PatternType type) {
		return type switch {
			PatternType.Spiral => () => SpiralPattern(),
			PatternType.Shotgun => () => ShotgunPattern(),
			PatternType.Window => () => WindowPattern(),
			PatternType.Arcs => () => ArcsPattern(),
			PatternType.Random => () => RandomPattern(),
			_ => null
		};
	}
}
