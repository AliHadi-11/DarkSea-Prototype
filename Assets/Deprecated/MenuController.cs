using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// =========================================================
//  Dark Sea - Simple Menu Controller (Optional / Alternative)
//  NOTE: Agar tum "MainMenuController.cs" use kar rahe ho to
//  is script ki zaroorat nahi - delete kar sakte ho. Yeh sirf
//  ek simple naam-input wala menu hai.
//
//  FIX: pehle yeh "GameLevel" scene load karta tha jo exist
//  hi nahi karta. Ab "DarkSea" (Level 1) load karta hai.
// =========================================================
public class MenuController : MonoBehaviour
{
    public TMP_InputField nameInput;

    public void StartGame()
    {
        string playerName = nameInput != null ? nameInput.text : "";
        if (string.IsNullOrEmpty(playerName)) playerName = "Player 1";

        PlayerPrefs.SetString("SavedName", playerName);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Level1_Transition"); // Level 1
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Band ho gayi");
    }
}