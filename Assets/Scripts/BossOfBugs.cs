using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class BossOfBugs : MonoBehaviour {
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;

	[SerializeField] private int health;

	[SerializeField] private float patternDelay;
	[SerializeField] private int shotgunCount;
	[SerializeField] private int shotgunRounds;
	[SerializeField] private float shotgunDelay;
	[SerializeField] private int spiralRounds;
	[SerializeField] private int spiralCount;
	[SerializeField] private float spiralDelay;

	private List<Func<IEnumerator>> patterns;
	private Coroutine currentPattern;
	private Coroutine patternRun;


	private void Awake() {
		patterns = new List<Func<IEnumerator>> {
			() => Spiral(),
			() => Shotgun(),
		};
		currentPattern = null;
		patternRun = null;
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha9)) {
			patternRun = StartCoroutine(RunPatterns());
		}
		if (Input.GetKeyDown(KeyCode.Alpha0)) {
			health = 0;
		}

		if (health <= 0) {
			if (currentPattern != null) {
				StopCoroutine(currentPattern);
			}
			if (patternRun != null) {
				StopCoroutine(patternRun);
			}
		}
	}

	private IEnumerator RunPatterns() {
		while (true) {
			foreach (Func<IEnumerator> pattern in patterns) {
				yield return new WaitForSeconds(patternDelay);
				currentPattern = StartCoroutine(pattern());
				yield return currentPattern;
			}
		}
	}

    private IEnumerator Shotgun() {
		for (int round = 0; round < shotgunRounds; round++) {
			for (int count = 0; count < shotgunCount; count++) {
				float angleDegrees = 360f / shotgunCount * count;
				float angleRadians = angleDegrees * Mathf.Deg2Rad;
				Vector3 direction = new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));

				GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
				Bullet bullet = bulletObject.GetComponent<Bullet>();
				bullet.Initialize(direction, bulletSpeed);
			}
			yield return new WaitForSeconds(shotgunDelay);
		}
	}

	private IEnumerator Spiral() {
		for (int round = 0; round < spiralRounds; round++) {
			for (int count = 0; count < spiralCount; count++) {
				float angleDegrees = 360f / spiralCount * count;
				float angleRadians = angleDegrees * Mathf.Deg2Rad;

				Vector3 direction = new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians)).normalized;

				GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
				Bullet bullet = bulletObject.GetComponent<Bullet>();
				bullet.Initialize(direction, bulletSpeed);

				yield return new WaitForSeconds(spiralDelay);
			}
		}
	}
}
