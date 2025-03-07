using UnityEngine;
using UnityEngine.Timeline;

public class HexManager : MonoBehaviour
{
    public static HexManager Instance { get; private set; }

    public GameObject P1HexIncurred;
    public GameObject P1ProtectionIncurred;

    public GameObject P2HexIncurred;
    public GameObject P2ProtectionIncurred;
    
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

    public void ScheduleHex(TurnManager.PlayerTurn player){
        if (player == TurnManager.PlayerTurn.Player1){
            P1HexIncurred.SetActive(true);
            TurnManager.Instance.SetIsP1HexedBool();
        } else {
            TurnManager.Instance.SetIsP2HexedBool();
            P2HexIncurred.SetActive(true);
        }
    }

    public void ScheduleProtection(TurnManager.PlayerTurn player){
        if (player == TurnManager.PlayerTurn.Player1){
            P1ProtectionIncurred.SetActive(true);
        } else {
            P2ProtectionIncurred.SetActive(true);
        }
    }

    
    public void DisableHexP1Canvas(){
        P1HexIncurred.SetActive(false);
    }

    public void DisableHexP2Canvas(){
        P2HexIncurred.SetActive(false);
    }
    public void ActivateFreezeHex()
    {
        if (TurnManager.Instance.currentTurn == TurnManager.PlayerTurn.Player1)
        {
            TurnManager.Instance.player2HasFreezeHex = true;
        }
        else
        {
            TurnManager.Instance.player1HasFreezeHex = true;
        }
        TurnManager.Instance.applyHexCanvasP1.SetActive(false);
        TurnManager.Instance.applyHexCanvasP2.SetActive(false);
        TurnManager.Instance.ApplyHexOrProt();
    }

    public void ActivateSneezeHex()
    {
        if (TurnManager.Instance.currentTurn == TurnManager.PlayerTurn.Player1)
        {
            TurnManager.Instance.player2HasSneezeHex = true;
        }
        else
        {
            TurnManager.Instance.player1HasSneezeHex = true;
        }
        TurnManager.Instance.applyHexCanvasP1.SetActive(false);
        TurnManager.Instance.applyHexCanvasP2.SetActive(false);
        TurnManager.Instance.ApplyHexOrProt();
    }

    public void ActivateBDeletusProtection()
    {
        if (TurnManager.Instance.currentTurn == TurnManager.PlayerTurn.Player1)
        {
            TurnManager.Instance.player1ProtectionBlockusDeletus = true; 
        }
        else
        {
            TurnManager.Instance.player2ProtectionBlockusDeletus = true;
        }
        P1ProtectionIncurred.SetActive(false);
        P2ProtectionIncurred.SetActive(false);
        TurnManager.Instance.ApplyHexOrProt();
    }

    public void ActivateTimeFreezeProtection()
    {
        if (TurnManager.Instance.currentTurn == TurnManager.PlayerTurn.Player1)
        {
            TurnManager.Instance.player1ProtectionTimeFreeze = true; 
        }
        else
        {
            TurnManager.Instance.player2ProtectionTimeFreeze = true;
        }
        P1ProtectionIncurred.SetActive(false);
        P2ProtectionIncurred.SetActive(false);
        TurnManager.Instance.ApplyHexOrProt();
    }
}
