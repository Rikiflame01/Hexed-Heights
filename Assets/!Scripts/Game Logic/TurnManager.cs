using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TurnManager : MonoBehaviour
{   
    public static TurnManager Instance { get; private set; }

    [Header("Turn Settings")]
    public bool hasTouched = false;
    public Material touchedMaterial;
    public int player1Health = 3;
    public int player2Health = 3;

    public enum PlayerTurn { Player1, Player2 }
    public PlayerTurn currentTurn = PlayerTurn.Player1;

    private bool lastTurnSuccessful = false;

    public bool rewindRoutineRunning = false;
    private bool turnFailed = false;
    public bool TurnFailed { get { return turnFailed; } }

    public bool IsTurnResetInProgress { get { return rewindRoutineRunning; } }

    public GameObject player1TurnCanvas;
    public GameObject player2TurnCanvas;

    public GameObject waitCanvas;

    public GameObject failCanvas;

    private struct RendererMaterialPair
    {
        public Renderer renderer;
        public Material originalMaterial;
        public RendererMaterialPair(Renderer renderer, Material originalMaterial)
        {
            this.renderer = renderer;
            this.originalMaterial = originalMaterial;
        }
    }
    private Dictionary<GameObject, List<RendererMaterialPair>> touchedBlocks = new Dictionary<GameObject, List<RendererMaterialPair>>();

    private Timer timer;

    private void Awake()
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

    void Start()
    {
        timer = Timer.Instance;
    }

    public void MarkBlockAsTouched(GameObject block)
    {
        turnFailed = false;
        if (ZoneExitManager.Instance != null)
            ZoneExitManager.Instance.ResetAllState();
        
        if (player1Health <= 0 || player2Health <= 0)
        {
            Debug.Log("Cannot mark block as touched. A player's health is 0 or less.");
            return;
        }
        if (!hasTouched && block != null)
        {
            hasTouched = true;
            Renderer[] renderers = block.GetComponentsInChildren<Renderer>();
            List<RendererMaterialPair> pairs = new List<RendererMaterialPair>();
            foreach (Renderer rend in renderers)
            {
                pairs.Add(new RendererMaterialPair(rend, rend.material));
                if (touchedMaterial != null)
                {
                    rend.material = touchedMaterial;
                }
            }
            touchedBlocks.Add(block, pairs);
            block.tag = "Touched";
            Debug.Log("Block marked as touched.");
        }
    }

    public void MarkTurnSuccessful()
    {
        lastTurnSuccessful = true;
        ActionManager.InvokeStickReset();
    }

    public void ShowTurnCanvas(){
        if (PlayerTurn.Player1 == currentTurn)
        {
            ActivatePlayer1Canvas();
        }
        else
        {
            ActivatePlayer2Canvas();
        }
    }

    public void WaitTurnCanvas()
    {
        waitCanvas.SetActive(true);
        CanvasGroup canvasGroup = waitCanvas.GetComponentInChildren<CanvasGroup>();
        StartCoroutine(LerpCanvasAlphaSequence(canvasGroup, 0, 1, 1f, 1f));
    }

    private IEnumerator LerpCanvasAlphaSequence(CanvasGroup canvasGroup, float start, float end, float duration, float waitTime)
    {
        yield return StartCoroutine(LerpCanvasAlpha(canvasGroup, start, end, duration));
        yield return new WaitForSeconds(waitTime);
        yield return StartCoroutine(LerpCanvasAlpha(canvasGroup, end, start, duration));
        waitCanvas.SetActive(false);
    }

    private IEnumerator LerpCanvasAlpha(CanvasGroup canvasGroup, float start, float end, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = end;
    }

    public void FailedAttemptCanvas()
    {
        failCanvas.SetActive(true);
        StartCoroutine(DeactivateFailCanvas());
    }

    private IEnumerator DeactivateFailCanvas()
    {
        yield return new WaitForSeconds(2f);
        failCanvas.SetActive(false);
    }

    public void ResetTurn()
    {
        if (player1Health <= 0 || player2Health <= 0)
        {
            Debug.Log("Turn reset aborted. A player's health is 0 or less.");
            return;
        }
        if (rewindRoutineRunning)
        {
            Debug.Log("ResetTurn already in progress.");
            return;
        }

        foreach (var entry in touchedBlocks)
        {
            GameObject block = entry.Key;
            foreach (var pair in entry.Value)
            {
                if (pair.renderer != null)
                    pair.renderer.material = pair.originalMaterial;
            }
            block.tag = "Untouched";
        }
        touchedBlocks.Clear();
        hasTouched = false;


        rewindRoutineRunning = true;
        StartCoroutine(RewindEndRoutine());   

    }

    private IEnumerator RewindEndRoutine()
    {
        if (!lastTurnSuccessful)
        {

            while (TimeRewindManager.Instance != null && TimeRewindManager.Instance.IsRewinding)
                yield return null;
            ActionManager.InvokeRewindEnd();
            turnFailed = true;
            Debug.Log($"[TurnManager] Failed turn: damage applied. Current turn remains: {currentTurn}");

            if (ZoneExitManager.Instance != null)
                ZoneExitManager.Instance.ResetPartialState();
        }
        else
        {
            Debug.Log("[TurnManager] Successful turn: no rewind, switching turn.");
            SwitchTurn();
        }
        lastTurnSuccessful = false;
        rewindRoutineRunning = false;
        Debug.Log("[TurnManager] RewindEndRoutine finished. State reset for new move.");
    }

    public void SwitchTurn()
    {
        if (player1Health <= 0 || player2Health <= 0)
        {
            Debug.Log("[TurnManager] SwitchTurn aborted. A player's health is 0 or less.");
            return;
        }
        timer.OnTurnSwitch();
        currentTurn = (currentTurn == PlayerTurn.Player1) ? PlayerTurn.Player2 : PlayerTurn.Player1;

        if (ZoneExitManager.Instance != null)
            ZoneExitManager.Instance.ResetAllState();


    }

    private void ActivatePlayer1Canvas(){
        player1TurnCanvas.SetActive(true);
        StartCoroutine(DeactivatePlayer1Canvas());
    }

    private IEnumerator DeactivatePlayer1Canvas()
    {
        yield return new WaitForSeconds(2f);
        player1TurnCanvas.SetActive(false);
    }

    private void ActivatePlayer2Canvas(){
        player2TurnCanvas.SetActive(true);
        StartCoroutine(DeactivatePlayer2Canvas());
    }

    private IEnumerator DeactivatePlayer2Canvas()
    {
        yield return new WaitForSeconds(2f);
        player2TurnCanvas.SetActive(false);
    }

    public void DecreaseHealth()
    {
        if (currentTurn == PlayerTurn.Player1)
        {
            player1Health = Mathf.Max(0, player1Health - 1);
            Debug.Log("Player 1 health decreased to " + player1Health);
            ActionManager.InvokeUpdatePlayerHP("Player 1");
            if (player1Health == 0)
                ActionManager.InvokeEndGame("Player 2");
                StopAllCoroutines();
        }
        else
        {
            player2Health = Mathf.Max(0, player2Health - 1);
            Debug.Log("Player 2 health decreased to " + player2Health);
            ActionManager.InvokeUpdatePlayerHP("Player 2");
            if (player2Health == 0)
                ActionManager.InvokeEndGame("Player 1");
                StopAllCoroutines();
        }
    }

    public bool IsGameOver()
    {
        return player1Health <= 0 || player2Health <= 0 || (failCanvas != null && failCanvas.activeSelf);
    }

}
