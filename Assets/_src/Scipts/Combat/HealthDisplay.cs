using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject healthBarParent = null;
    [SerializeField] private Image healthBarImage = null;

    private void OnEnable() {
        health.ClientOnHealthUpdated += HandleHealthUpdated;
    }

    private void Start() {
        healthBarParent.SetActive(true);
    }

    private void OnDisable() {
        health.ClientOnHealthUpdated -= HandleHealthUpdated;
    }

    private void HandleHealthUpdated(int currentHealth, int maxHealth) {
        healthBarImage.fillAmount = (float)currentHealth / maxHealth;
    }
}
