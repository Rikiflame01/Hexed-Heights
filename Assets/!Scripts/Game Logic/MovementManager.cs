using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovementManager : MonoBehaviour
{
    [Header("UI Toggles (TextMeshPro Toggles)")]
    public Toggle movement1Toggle;
    public Toggle movement2Toggle;

    [Header("Controller References")]
    public PlayerController playerController;
    public GrabberController grabberController;

    private void Start()
    {
        if (movement1Toggle != null) movement1Toggle.isOn = true;
        if (movement2Toggle != null) movement2Toggle.isOn = false;
        SetMovementMode(PlayerController.MovementMode.Movement1);

        if (movement1Toggle != null)
            movement1Toggle.onValueChanged.AddListener(OnMovement1ToggleChanged);
        if (movement2Toggle != null)
            movement2Toggle.onValueChanged.AddListener(OnMovement2ToggleChanged);
    }

    private void OnDestroy()
    {
        if (movement1Toggle != null)
            movement1Toggle.onValueChanged.RemoveListener(OnMovement1ToggleChanged);
        if (movement2Toggle != null)
            movement2Toggle.onValueChanged.RemoveListener(OnMovement2ToggleChanged);
    }

    private void OnMovement1ToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (movement2Toggle != null) movement2Toggle.isOn = false;
            SetMovementMode(PlayerController.MovementMode.Movement1);
        }
    }

    private void OnMovement2ToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (movement1Toggle != null) movement1Toggle.isOn = false;
            SetMovementMode(PlayerController.MovementMode.Movement2);
        }
    }
    private void SetMovementMode(PlayerController.MovementMode mode)
    {
        if (playerController != null)
            playerController.currentMovementMode = mode;
        if (grabberController != null)

            grabberController.currentMovementMode = (GrabberController.MovementMode)mode;
    }
}
