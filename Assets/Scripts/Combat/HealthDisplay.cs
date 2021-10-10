using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour {
    [SerializeField] private Health health;
    [SerializeField] private GameObject healthBarParent;
    [SerializeField] private Image healthBarImage;

    private void Awake() {
        health.ClientOnHealthUpdated += HandleHealthUpdated;
        healthBarParent.SetActive(false);
    }

    private void OnDestroy() {
        health.ClientOnHealthUpdated -= HandleHealthUpdated;
    }

    private void OnMouseEnter() {
        healthBarParent.SetActive(true);
    }

    private void OnMouseExit() {
        healthBarParent.SetActive(false);
    }

    private void HandleHealthUpdated(int currentHealth, int maxHealth) {
        healthBarImage.fillAmount = (float) currentHealth / maxHealth;
    }
}
