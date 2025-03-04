using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public static Timer Instance { get; private set; }

    [Header("Timer Settings")]
    public float totalGameTime = 80f;
    public float turnTimeThreshold = 10f;  
    private float currentTurnTime;  
    private float player1TotalTime;   
    private float player2TotalTime;   
    private float remainingGameTime;   
    private bool isTimerRunning = false;     
    public bool isGameTimerRunning = false; 
    private bool isPaused = false;          
    private TurnManager.PlayerTurn previousTurn;

    [Header("UI Elements")]
    public TextMeshProUGUI player1TimeText;
    public TextMeshProUGUI player2TimeText;
    public TextMeshProUGUI totalTimeText;  

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

    private void Start()
    {
        TurnManager.Instance.currentTurn = TurnManager.PlayerTurn.Player1;
        previousTurn = TurnManager.Instance.currentTurn;
        
        remainingGameTime = totalGameTime;
        totalTimeText.text = remainingGameTime.ToString("F2");
        isGameTimerRunning = false;
    }

    private void Update()
    {
        if (!TurnManager.Instance.IsGameOver() && isGameTimerRunning)
        {
            remainingGameTime -= Time.deltaTime;
            totalTimeText.text = Mathf.Max(remainingGameTime, 0).ToString("F2");
            if (remainingGameTime <= 0)
            {
                EndGameByTime();
                return;
            }
        }

        if (TurnManager.Instance.currentTurn != previousTurn)
        {
            currentTurnTime = 0f;
            previousTurn = TurnManager.Instance.currentTurn;
        }

        if (isTimerRunning && !isPaused && !TurnManager.Instance.IsTurnResetInProgress)
        {
            currentTurnTime += Time.deltaTime;
        }

        float player1DisplayTime = player1TotalTime +
            (TurnManager.Instance.currentTurn == TurnManager.PlayerTurn.Player1 ? currentTurnTime : 0);
        float player2DisplayTime = player2TotalTime +
            (TurnManager.Instance.currentTurn == TurnManager.PlayerTurn.Player2 ? currentTurnTime : 0);

        player1TimeText.text = player1DisplayTime.ToString("F2");
        player2TimeText.text = player2DisplayTime.ToString("F2");
    }

    public void OnTurnSwitch()
    {
        if (isTimerRunning)
        {
            EndTurnTimer();
        }
        
        currentTurnTime = 0f;
        StartTurnTimer();
    }

    public void StartTurnTimer()
    {
        isGameTimerRunning = true;
        currentTurnTime = 0f;
        isTimerRunning = true;
        isPaused = false;
        Debug.Log($"Timer started for {TurnManager.Instance.currentTurn}");
    }

    public void EndTurnTimer()
    {
        if (!isPaused)
        {
            float turnTime = currentTurnTime;
            Debug.Log($"{TurnManager.Instance.currentTurn} turn ended. Time taken: {turnTime:F2} seconds");

            if (TurnManager.Instance.currentTurn == TurnManager.PlayerTurn.Player1)
            {
                player1TotalTime += turnTime;
            }
            else
            {
                player2TotalTime += turnTime;
            }

            if (turnTime > turnTimeThreshold)
            {
                ApplyDebuff(TurnManager.Instance.currentTurn);
            }
            else
            {
                ApplyBuff(TurnManager.Instance.currentTurn);
            }
        }
        else
        {
            Debug.Log($"{TurnManager.Instance.currentTurn} turn ended but was paused - no time recorded.");
        }
        isTimerRunning = false;
    }

    private void EndGameByTime()
    {
        isGameTimerRunning = false;
        isTimerRunning = false;
        Debug.Log("Game time has run out!");

        TurnManager turnManager = TurnManager.Instance;
            if (turnManager.player1Health > turnManager.player2Health)
            {
                ActionManager.InvokeEndGame("Player 1");
            }
            else if (turnManager.player2Health > turnManager.player1Health)
            {
                ActionManager.InvokeEndGame("Player 2");
            }
            else
            {
                if (player1TotalTime < player2TotalTime)
                {
                    ActionManager.InvokeEndGame("Player 1");
                }
                else if (player2TotalTime < player1TotalTime)
                {
                    ActionManager.InvokeEndGame("Player 2");
                }
                else
                {
                    ActionManager.InvokeEndGame("Tie");
                }
            }
    }

    private void ApplyBuff(TurnManager.PlayerTurn player)
    {
        Debug.Log($"{player} received a buff!");
    }

    private void ApplyDebuff(TurnManager.PlayerTurn player)
    {
        Debug.Log($"{player} received a debuff!");
    }
}
