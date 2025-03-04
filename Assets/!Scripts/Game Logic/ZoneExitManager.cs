using System.Collections;
using UnityEngine;

public class ZoneExitManager : MonoBehaviour
{
    public static ZoneExitManager Instance { get; private set; }
    
    [SerializeField] private bool verboseLogging = false;

    private bool exitReceived = false;
    private bool firstExitWasTouched = false;
    private GameObject firstExitedBlock = null;
    private Coroutine waitCoroutine;

    private bool outcomeProcessed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
        if (verboseLogging)
            Debug.Log("[ZoneExitManager] Awake completed.");
    }

    private void Log(string message)
    {
        if (verboseLogging)
            Debug.Log(message);
    }

    public void RegisterExit(GameObject block)
    {
        Log($"[ZoneExitManager] RegisterExit called for block {block?.name ?? "null"}. OutcomeProcessed={outcomeProcessed}, ExitReceived={exitReceived}");
        
        if (TurnManager.Instance != null && TurnManager.Instance.TurnFailed)
        {
            Log("[ZoneExitManager] Current move already failed. Ignoring exit event.");
            return;
        }
        
        if (TurnManager.Instance != null && TurnManager.Instance.IsTurnResetInProgress)
        {
            Log("[ZoneExitManager] Turn reset in progress. Ignoring exit event.");
            return;
        }
        if (outcomeProcessed)
        {
            Log("[ZoneExitManager] Outcome already processed. Ignoring exit event.");
            return;
        }
        if (block == null)
            return;
        if (exitReceived)
        {
            if (block == firstExitedBlock)
            {
                Log("[ZoneExitManager] Duplicate exit from the same block ignored.");
                return;
            }
            if (waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
                waitCoroutine = null;
                Log("[ZoneExitManager] Stopped wait coroutine due to additional distinct exit.");
            }
            ProcessFailure("Additional exit detected within time window. Failure.");
            return;
        }
        exitReceived = true;
        firstExitedBlock = block;
        Log($"[ZoneExitManager] First exit recorded from block {block.name} with tag {block.tag}");
        if (block.CompareTag("Touched"))
        {
            firstExitWasTouched = true;
            waitCoroutine = StartCoroutine(WaitForAdditionalExits());
            Log("[ZoneExitManager] Started wait coroutine for additional exits (3 seconds).");
        }
        else if (block.CompareTag("Untouched"))
        {
            ProcessFailure("First exit is Untouched. Failure.");
        }
        else
        {
            ProcessFailure("First exit has an unexpected tag. Failure.");
        }
    }

    private IEnumerator WaitForAdditionalExits()
    {
        TurnManager.Instance.WaitTurnCanvas();
        yield return new WaitForSeconds(3f);
        ProcessSuccess("Only one Touched block exited within time window. Success.");
    }

    private void ProcessSuccess(string message)
    {
        if (outcomeProcessed)
            return;
        outcomeProcessed = true;
        Log($"[ZoneExitManager] ProcessSuccess: {message}");
        TurnManager.Instance.MarkTurnSuccessful();
        if (firstExitedBlock != null)
        {
            Log($"[ZoneExitManager] Destroying block {firstExitedBlock.name}.");
            Destroy(firstExitedBlock);
        }
        TurnManager.Instance.ResetTurn();
        TurnManager.Instance.ShowTurnCanvas();

        ResetPartialState();
    }

    private void ProcessFailure(string message)
    {
        if (outcomeProcessed)
            return;
        outcomeProcessed = true;

        if (TurnManager.Instance.player1Health == 1 && TurnManager.Instance.currentTurn == TurnManager.PlayerTurn.Player1)
        {
            TurnManager.Instance.DecreaseHealth();
            Log("Game over, don't execute any other reset logic");
            StopAllCoroutines();
            return;
        }
        if (TurnManager.Instance.player2Health == 1 && TurnManager.Instance.currentTurn == TurnManager.PlayerTurn.Player2)
        {
            TurnManager.Instance.DecreaseHealth();
            Log("Game over, don't execute any other reset logic");
            StopAllCoroutines();
            return;
        }
        TurnManager.Instance.waitCanvas.SetActive(false);
        TurnManager.Instance.DecreaseHealth();
        TurnManager.Instance.FailedAttemptCanvas();
        Log($"[ZoneExitManager] ProcessFailure: {message}");
        ActionManager.InvokeStickReset();
        ActionManager.InvokeFailedAttempt();
        ActionManager.InvokeRewind();
        TurnManager.Instance.ResetTurn();

        ResetAllState();
    }

    public void ResetPartialState()
    {
        Log("[ZoneExitManager] ResetPartialState called.");
        exitReceived = false;
        firstExitWasTouched = false;
        firstExitedBlock = null;
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

    }

    public void ResetAllState()
    {
        Log("[ZoneExitManager] ResetAllState called.");
        ResetPartialState();
        outcomeProcessed = false;
    }
}
