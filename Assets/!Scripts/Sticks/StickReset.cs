using UnityEngine;

public class StickReset : MonoBehaviour
{
    public Transform StickResetPoint;

    private void OnEnable()
    {
        ActionManager.OnStickReset += ResetStick;
    }

    private void ResetStick()
    {
        gameObject.transform.position = StickResetPoint.position;
    }
    private void OnDisable()
    {
        ActionManager.OnStickReset -= ResetStick;
    }
}
