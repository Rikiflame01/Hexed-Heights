using UnityEngine;

public class ProtectionSpells : MonoBehaviour
{
    public static ProtectionSpells Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void BlockusDeletus()
    {
        Debug.Log("Blockus Deletus protection is active.");

    }

    public void TimeFreeze()
    {

        Debug.Log("Time Freeze protection is active.");
        Timer.Instance.PauseTimer();

    }
}