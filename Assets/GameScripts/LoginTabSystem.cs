using UnityEngine;
using TMPro; // TMP Input fields ke liye
using UnityEngine.UI; // Buttons ke liye
using UnityEngine.EventSystems; // Focus check karne ke liye
using UnityEngine.SceneManagement; // Scene change karne ke liye

public class LoginTabSystem : MonoBehaviour
{
    [Header("Login UI")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TextMeshProUGUI loginMessageText; // Success/Error message dikhane ke liye

    [Header("Forgot Password UI")]
    public GameObject forgotPasswordPanel; // Naya popup box
    public TMP_InputField forgotUsernameInput;
    public TMP_InputField forgotEmailInput;
    public TextMeshProUGUI forgotMessageText;

    void Start()
    {
        SetupCaret(usernameInput);
        SetupCaret(passwordInput);
        SetupCaret(forgotUsernameInput);
        SetupCaret(forgotEmailInput);
    }

    static void SetupCaret(TMP_InputField f)
    {
        if (f == null) return;
        f.customCaretColor = true;
        f.caretColor       = Color.white;
        f.caretBlinkRate   = 0.85f;
        f.caretWidth       = 2;
    }

    void Update()
    {
        // --- 1. Agar Forgot Password Panel BAND hai (Normal Login chal raha hai) ---
        if (forgotPasswordPanel != null && !forgotPasswordPanel.activeSelf)
        {
            // TAB Key Logic
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (usernameInput.isFocused) passwordInput.Select();
                else if (passwordInput.isFocused) usernameInput.Select();
            }

            // ENTER Key Logic
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnLoginButtonClicked(); // Direct function call kar diya
            }
        }
        // --- 2. Agar Forgot Password Panel KHULA hua hai ---
        else if (forgotPasswordPanel != null && forgotPasswordPanel.activeSelf)
        {
            // TAB Key Logic (Popup ke andar)
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (forgotUsernameInput.isFocused) forgotEmailInput.Select();
                else if (forgotEmailInput.isFocused) forgotUsernameInput.Select();
            }

            // ENTER Key Logic (Popup ke andar Recover karna)
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                RecoverPassword();
            }
        }
    }

    // ==========================================
    //            LOGIN LOGIC
    // ==========================================
    public void OnLoginButtonClicked()
    {
        string user = usernameInput.text.Trim();
        string pass = passwordInput.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            loginMessageText.text = "Error: Username and Password required!";
            loginMessageText.color = Color.red;
            return;
        }

        // Local Database check
        if (PlayerPrefs.HasKey(user))
        {
            string savedPassword = PlayerPrefs.GetString(user);
            if (pass == savedPassword)
            {
                loginMessageText.text = "Success: Logging in...";
                loginMessageText.color = Color.green;
                PlayerPrefs.SetString("CurrentPlayer", user);
                PlayerPrefs.Save();
                Invoke("LoadMainGame", 1.5f);
            }
            else
            {
                loginMessageText.text = "Error: Incorrect Password!";
                loginMessageText.color = Color.red;
            }
        }
        else
        {
            loginMessageText.text = "Error: Account not found! Please register.";
            loginMessageText.color = Color.red;
        }
    }

    public void GoToRegisterScene()
    {
        SceneManager.LoadScene("RegisterScene");
    }

    void LoadMainGame()
    {
        // Apne level ka naam theek se yahan likhein
        SceneManager.LoadScene("MainMenu");
    }

    // ==========================================
    //         FORGOT PASSWORD LOGIC
    // ==========================================

    // Popup open karne ke liye (Forgot button par lagana hai)
    public void OpenForgotPanel()
    {
        forgotPasswordPanel.SetActive(true);
        forgotMessageText.text = "Enter Username & Email to recover password.";
        forgotMessageText.color = Color.white;

        // Boxes ko pehle se khali kar do
        forgotUsernameInput.text = "";
        forgotEmailInput.text = "";
    }

    // Popup close karne ke liye (Close/X button par lagana hai)
    public void CloseForgotPanel()
    {
        forgotPasswordPanel.SetActive(false);
    }

    // Recover button dabane par
    public void RecoverPassword()
    {
        string recUser = forgotUsernameInput.text.Trim();
        string recEmail = forgotEmailInput.text.Trim();

        if (string.IsNullOrEmpty(recUser) || string.IsNullOrEmpty(recEmail))
        {
            forgotMessageText.text = "Error: Enter both fields!";
            forgotMessageText.color = Color.red;
            return;
        }

        if (PlayerPrefs.HasKey(recUser))
        {
            string savedEmail = PlayerPrefs.GetString(recUser + "_email");
            if (recEmail == savedEmail)
            {
                // Result mil gaya!
                string savedPassword = PlayerPrefs.GetString(recUser);
                forgotMessageText.text = "Your Password is: " + savedPassword;
                forgotMessageText.color = Color.green;
            }
            else
            {
                forgotMessageText.text = "Error: Email does not match!";
                forgotMessageText.color = Color.red;
            }
        }
        else
        {
            forgotMessageText.text = "Error: Username not found.";
            forgotMessageText.color = Color.red;
        }
    }
}