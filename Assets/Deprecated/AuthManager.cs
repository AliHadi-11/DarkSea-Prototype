using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro ke liye zaroori hai
using UnityEngine.SceneManagement; // Level change karne ke liye

public class AuthManager : MonoBehaviour
{
    [Header("UI Fields")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI feedbackText; // Error ya Success msg ke liye

    [Header("Scene Settings")]
    public string levelToLoad = "Level1"; // Apne Game Level ka naam yahan likhein

    // --- SIGN UP FUNCTION ---
    public void RegisterUser()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        // 1. Check agar fields khali hain
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Error: Fields cannot be empty!";
            feedbackText.color = Color.red;
            return;
        }

        // 2. Check agar user pehle se bana hua hai
        if (PlayerPrefs.HasKey(username))
        {
            feedbackText.text = "Error: Username already taken!";
            feedbackText.color = Color.red;
            return;
        }

        // 3. Save Data (Database Simulation)
        PlayerPrefs.SetString(username, password);
        PlayerPrefs.Save();

        feedbackText.text = "Success: Account Created! Please Login.";
        feedbackText.color = Color.green;

        // Fields clear kar dein
        usernameInput.text = "";
        passwordInput.text = "";
    }

    // --- LOGIN FUNCTION ---
    public void LoginUser()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        // 1. Check agar user exist karta hai
        if (!PlayerPrefs.HasKey(username))
        {
            feedbackText.text = "Error: User does not exist!";
            feedbackText.color = Color.red;
            return;
        }

        // 2. Password Match karein
        string savedPassword = PlayerPrefs.GetString(username);

        if (savedPassword == password)
        {
            feedbackText.text = "Login Successful! Loading Game...";
            feedbackText.color = Color.green;

            // 3. Game Load karein (Thora delay taake msg parh sakein)
            Invoke("LoadGameScene", 1.5f);
        }
        else
        {
            feedbackText.text = "Error: Incorrect Password!";
            feedbackText.color = Color.red;
        }
    }

    void LoadGameScene()
    {
        Debug.Log("Login Successful!");
        PlayerPrefs.SetString("CurrentPlayer", usernameInput.text); // <--- Ye line add karni hai
        SceneManager.LoadScene("MainMenu");
    }
}