using UnityEngine;
using UnityEngine.UI;

public class HealthUIUpdater : MonoBehaviour
{
    [Header("Player 1 Hearts (Left to Right)")]
    public Image[] player1Hearts;

    [Header("Player 2 Hearts (Left to Right)")]
    public Image[] player2Hearts;

    private void OnEnable()
    {
        ActionManager.OnUpdatePlayerHP += OnUpdatePlayerHP;
    }

    private void OnDisable()
    {
        ActionManager.OnUpdatePlayerHP -= OnUpdatePlayerHP;
    }

    private void OnUpdatePlayerHP(string player)
    {
        if (player == "Player 1")
        {
            int health = TurnManager.Instance.player1Health;
            UpdateHearts(player1Hearts, health);
        }
        else if (player == "Player 2")
        {
            int health = TurnManager.Instance.player2Health;
            UpdateHearts(player2Hearts, health);
        }
        else
        {
            Debug.LogWarning("Unrecognized player: " + player);
        }
    }

    private void UpdateHearts(Image[] hearts, int currentHealth)
    {
        //   currentHealth = 3 -> disable 0 hearts
        //   currentHealth = 2 -> disable 1 heart (index 0)
        //   currentHealth = 1 -> disable 2 hearts (indices 0 and 1)
        //   currentHealth = 0 -> disable all hearts
        int heartsToDisable = hearts.Length - currentHealth;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < heartsToDisable)
            {
                hearts[i].enabled = false;
            }
            else
            {
                hearts[i].enabled = true;
            }
        }
    }
}
