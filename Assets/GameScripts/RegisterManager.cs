using UnityEngine;
using TMPro; // TextMeshPro ke liye
using UnityEngine.SceneManagement; // Scene change karne ke liye

public class RegisterManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;

    [Header("Message Display")]
    public TextMeshProUGUI messageText;

    void Start()
    {
        SetupCaret(usernameInput);
        SetupCaret(emailInput);
        SetupCaret(passwordInput);
        SetupCaret(confirmPasswordInput);
    }

    static void SetupCaret(TMP_InputField f)
    {
        if (f == null) return;
        f.customCaretColor = true;
        f.caretColor       = Color.white;
        f.caretBlinkRate   = 0.85f;
        f.caretWidth       = 2;
    }

    // Register Button dabane par yeh chalega
    public void OnRegisterButtonClicked()
    {
        string user = usernameInput.text.Trim();
        string email = emailInput.text.Trim();
        string pass = passwordInput.text;
        string confPass = confirmPasswordInput.text;

        // 1. Check karna ke koi box khali to nahi?
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(confPass))
        {
            messageText.text = "Error: Please fill all fields!";
            messageText.color = Color.red;
            return;
        }

        // 2. Check karna ke dono Passwords match kar rahe hain ya nahi?
        if (pass != confPass)
        {
            messageText.text = "Error: Passwords do not match!";
            messageText.color = Color.red;
            return;
        }

        // 3. Local Database (PlayerPrefs) mein check karna ke account pehle se to nahi hai?
        if (PlayerPrefs.HasKey(user))
        {
            messageText.text = "Error: Username already exists!";
            messageText.color = Color.red;
            return;
        }

        // 4. Data Local Computer par Save karna
        PlayerPrefs.SetString(user, pass); // Username ke sath Password save kar diya
        PlayerPrefs.SetString(user + "_email", email); // Email bhi save kar li
        PlayerPrefs.Save(); // Data ko hard drive par pakka save karna

        // Success Message
        Debug.Log("Account Locally Created for: " + user);
        messageText.text = "Success: Account Created! Redirecting...";
        messageText.color = Color.green;

        // 2 seconds ke baad Login scene par bhej dena
        Invoke("GoToLoginScene", 2f);
    }

    // "Already have an account?" wale text par click karne se yeh chalega
    public void GoToLoginScene()
    {
        // Apne Login scene ka exact naam yahan likhein
        SceneManager.LoadScene("LoginScene");
    }
}