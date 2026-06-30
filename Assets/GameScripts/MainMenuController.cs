using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Text ke liye zaroori

public class MainMenuController : MonoBehaviour
{
    public TMP_Text welcomeText; // Screen wala text yahan ayega

    void Start()
    {
        // 1. Memory se naam uthao (Agar koi naam nahi to "Commander" likho)
        string playerName = PlayerPrefs.GetString("CurrentPlayer", "Commander");

        // 2. Text update karo
        if (welcomeText != null)
        {
            welcomeText.text = "Welcome, " + playerName + "!";
        }
    }

    public void StartMission()
    {
        SceneManager.LoadScene("Level1_Transition");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}