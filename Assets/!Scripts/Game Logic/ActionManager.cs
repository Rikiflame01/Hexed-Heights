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
    public static event Action OnStickReset;
    public static event Action OnEndTurn;
    public static event Action OnFailedAttempt;

    public static event Action OnRewind;

    public static event Action OnRewindEnd;

    public static event Action<string> OnUpdatePlayerHP;

    public static event Action OnMainMenu;

    public static event Action<string> OnEndGame;

    public static void InvokeRewindEnd()
    {
        OnRewindEnd?.Invoke();
    }
    public static void InvokeRewind()
    {
        OnRewind?.Invoke();
    }
    public static void InvokeUpdatePlayerHP(string player)
    {
        OnUpdatePlayerHP?.Invoke(player);
    }
    public static void InvokeEndGame(string player)
    {
        OnEndGame?.Invoke(player);
    }
    public static void InvokeFailedAttempt()
    {
        OnFailedAttempt?.Invoke();
    }

    public static void InvokeEndTurn()
    {
        OnEndTurn?.Invoke();
    }

    public static void InvokeStickReset()
    {
        OnStickReset?.Invoke();
    }

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
