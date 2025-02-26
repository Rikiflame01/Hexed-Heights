using UnityEngine;
using System;
using System.Diagnostics;
using Unity.VisualScripting;

public static class ActionManager
{
    public static event Action OnPaused;
    public static event Action OnUnpaused;
    public static event Action OnPlaying;
    public static event Action OnChooseCard;

    public static event Action OnMainMenu;

    public static void InvokeMainMenu()
    {
        OnMainMenu?.Invoke();
    }
    public static void InvokePaused()
    {
        OnPaused?.Invoke();
    }

    public static void InvokeUnpaused()
    {
        OnUnpaused?.Invoke();
    }

    public static void InvokeChooseCard()
    {
        OnChooseCard?.Invoke();
    }

    public static void InvokePlaying()
    {
        OnPlaying?.Invoke();
    }
}
