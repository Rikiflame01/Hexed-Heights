using TMPro;
using UnityEngine;

public class EndGame : MonoBehaviour
{
    public GameObject endGamePanel;

    public TextMeshProUGUI TextMeshProUGUI;

    private void OnEnable()
    {
        ActionManager.OnEndGame += OnEndGame;
    }

    private void OnDisable()
    {
        ActionManager.OnEndGame -= OnEndGame;
    }

    private void OnEndGame(string winner)
    {
        SoundManager.Instance.PlayEndGameSoundtrack();
        endGamePanel.SetActive(true);
        TextMeshProUGUI.text = winner + " wins!";
    }

}
