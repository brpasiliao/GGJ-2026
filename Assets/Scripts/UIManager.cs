using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour {
    private static UIManager instance;
    public static UIManager Instance => instance;

    [SerializeField] private Image bossHealthBar;
    [SerializeField] private Transform playerHealth;


    private void Awake() {
        instance = this;
    }

    public void UpdateBossHealthBar(float percent) {
        bossHealthBar.fillAmount = percent;
    }

    public void UpdateCharacterHealth(int heartCount) {
        int count = 0;
        foreach(Transform child in playerHealth) {
            child.gameObject.SetActive(false);
            if (count < heartCount) {
                child.gameObject.SetActive(true);
                count++;
            }
        }
    }
}
