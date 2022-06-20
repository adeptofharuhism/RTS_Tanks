using UnityEngine;

public class UnitMinimapIconActivator : MonoBehaviour
{
    [SerializeField] private GameObject _minimapIcon;

    private void Start() {
        _minimapIcon.SetActive(true);
    }
}
