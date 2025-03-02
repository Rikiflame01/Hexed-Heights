using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    private bool rewindRoutineRunning = false;
    private bool turnFailed = false;
    public bool TurnFailed { get { return turnFailed; } }

    public bool IsTurnResetInProgress { get { return rewindRoutineRunning; } }

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
        Debug.Log("[TurnManager] RewindEndRoutine started.");
        yield return new WaitForSeconds(5f);

        if (!lastTurnSuccessful)
        {

            while (TimeRewindManager.Instance != null && TimeRewindManager.Instance.IsRewinding)
                yield return null;
            ActionManager.InvokeRewindEnd();
            DecreaseHealth();
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
        ActionManager.InvokeStickReset();
        currentTurn = (currentTurn == PlayerTurn.Player1) ? PlayerTurn.Player2 : PlayerTurn.Player1;

        if (ZoneExitManager.Instance != null)
            ZoneExitManager.Instance.ResetAllState();
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
        }
        else
        {
            player2Health = Mathf.Max(0, player2Health - 1);
            Debug.Log("Player 2 health decreased to " + player2Health);
            ActionManager.InvokeUpdatePlayerHP("Player 2");
            if (player2Health == 0)
                ActionManager.InvokeEndGame("Player 1");
        }
    }
}
