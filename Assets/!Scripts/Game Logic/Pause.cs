using System;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool isPaused = false;
    public bool IsPaused => isPaused;

    private void OnEnable()
    {
        ActionManager.OnPaused += TogglePause;
    }

    private void OnDisable()
    {
        ActionManager.OnPaused -= TogglePause;
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            Time.timeScale = 1;
            pauseMenu.SetActive(false);
            isPaused = false;
        }
        else
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
            isPaused = true;
        }
    }



}
