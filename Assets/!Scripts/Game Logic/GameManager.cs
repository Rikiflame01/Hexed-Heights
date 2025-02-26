using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Start()
    {
        GameStateManager.Initialize();

    }


    public static void ExitGame(){
        Application.Quit();
    }

    public static void RestartGame(){
        SceneManager.LoadScene("GameScene");
    }

}

