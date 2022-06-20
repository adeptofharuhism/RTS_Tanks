using TMPro;
using UnityEngine;

public class PlayerNameSetter : MonoBehaviour
{
    private const string PLACEHOLDER = "Waiting for player...";

    [SerializeField] private TMP_Text playerNameText = null;
    [SerializeField] private TMP_Text placeholderText = null;

    public void SetPlayerName(string name) {
        playerNameText.text = name;

        placeholderText.text = (name == string.Empty) ? PLACEHOLDER : string.Empty;
    }
}
