using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;


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
	// angle offset?
}


public class Boss : MonoBehaviour, IShootable {
    [SerializeField] private GameObject bulletPrefab;

	[SerializeField] private int health;
	[SerializeField] private float patternDelay;

	[SerializeField] private List<Pattern> patterns;
	[SerializeField] private List<Pattern> testPatterns;
	private Pattern currentPattern;
	private Coroutine currentPatternRoutine;
	private Coroutine patternSequence;


	private void Awake() {
		currentPatternRoutine = null;
		patternSequence = null;
	}

	public void GetShot() {
		health--;

		if (health <= 0) {
			Debug.Log("boss defeated");
			// defeat boss
		}
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha8)) {
			Func<List<Pattern>, IEnumerator> routine = (list) => RunPatterns(list);
			patternSequence = StartCoroutine(routine(testPatterns));
		}
		if (Input.GetKeyDown(KeyCode.Alpha9)) {
			Func<List<Pattern>, IEnumerator> routine = (list) => RunPatterns(list);
			patternSequence = StartCoroutine(routine(patterns));
		}
		if (Input.GetKeyDown(KeyCode.Alpha0)) {
			health = 0;

			if (health <= 0) {
				if (currentPatternRoutine != null) {
					StopCoroutine(currentPatternRoutine);
				}
				if (patternSequence != null) {
					StopCoroutine(patternSequence);
				}
			}
		}
	}

	private IEnumerator RunPatterns(List<Pattern> patternsList) {
		if (patternsList.Count == 0) {
			yield break;
		}

		while (true) {
			foreach (Pattern pattern in patternsList) {
				yield return new WaitForSeconds(patternDelay);
				currentPattern = pattern;
				Func<IEnumerator> patternRoutine = GetRoutineFromEnum(pattern.type);
				currentPatternRoutine = StartCoroutine(patternRoutine());
				yield return currentPatternRoutine;
			}
		}
	}

    private IEnumerator ShotgunPattern() {
		for (int round = 0; round < currentPattern.rounds; round++) {
			int variation = round % currentPattern.variations;

			for (int count = 0; count < currentPattern.bulletCount; count++) {
				if (count % currentPattern.variations != variation) {
					continue;
				}
				//bool skip = false;
				//for (int variation = 0; variation < currentPattern.variations; variation++) {
				//	if (round % currentPattern.rounds != variation && count % currentPattern.rounds != variation) {
				//		skip = true;
				//		break;
				//	}
				//}
				//if (skip) { continue; }

				float angleDegrees = 360f / currentPattern.bulletCount * count;
				float angleRadians = angleDegrees * Mathf.Deg2Rad;
				Vector3 direction = new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));

				GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
				Bullet bullet = bulletObject.GetComponent<Bullet>();
				bullet.Initialize(direction, currentPattern.bulletSpeed, this);
			}
			yield return new WaitForSeconds(currentPattern.delay);
		}
	}

	private IEnumerator SpiralPattern() {
		for (int round = 0; round < currentPattern.rounds; round++) {
			for (int count = 0; count < currentPattern.bulletCount; count++) {
				float angleDegrees = 360f / currentPattern.bulletCount * count;
				float angleRadians = angleDegrees * Mathf.Deg2Rad;
				Vector3 direction = new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians)).normalized;

				GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
				Bullet bullet = bulletObject.GetComponent<Bullet>();
				bullet.Initialize(direction, currentPattern.bulletSpeed, this);

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

				float angleRadians = angleDegrees * Mathf.Deg2Rad;
				Vector3 direction = new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));

				GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
				Bullet bullet = bulletObject.GetComponent<Bullet>();
				bullet.Initialize(direction, currentPattern.bulletSpeed, this);
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

				float angleRadians = angleDegrees * Mathf.Deg2Rad;
				Vector3 direction = new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians)).normalized;

				GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
				Bullet bullet = bulletObject.GetComponent<Bullet>();
				bullet.Initialize(direction, currentPattern.bulletSpeed, this);
			}
			yield return new WaitForSeconds(currentPattern.delay);
		}
	}

	private IEnumerator RandomPattern() {
		for (int count = 0; count < currentPattern.bulletCount; count++) {
			float angleDegrees = Random.Range(0, 360f);
			float angleRadians = angleDegrees * Mathf.Deg2Rad;
			Vector3 direction = new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians)).normalized;

			GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
			Bullet bullet = bulletObject.GetComponent<Bullet>();
			bullet.Initialize(direction, currentPattern.bulletSpeed, this);

			yield return new WaitForSeconds(currentPattern.delay);
		}
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
