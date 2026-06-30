using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

// =========================================================
//  Dark Sea — Auth UI  (Offline / PlayerPrefs)
//
//  Bilkul offline kaam karta hai — koi internet nahi chahiye.
//  Sab data Unity ke PlayerPrefs mein save hota hai
//  (game ke local storage mein, jaise settings save hoti hain).
//
//  Register:
//    - Username + Password check karta hai
//    - "PREF_USER_username" key mein password save karta hai
//    - Duplicate username allow nahi
//
//  Login:
//    - Username + Password match karta hai PlayerPrefs se
//    - Match ho to "CurrentPlayer" set karta hai
//    - MainMenu pe le jaata hai
//
//  Setup:
//    1. RegisterScene aur LoginScene dono mein:
//       - Empty GameObject "AuthManager" banao
//       - UI Document component lagao → AuthPanel.uxml assign karo
//       - Ye script (AuthUI) lagao — koi field assign nahi karna
// =========================================================
[RequireComponent(typeof(UIDocument))]
public class AuthUI : MonoBehaviour
{
    // PlayerPrefs key prefix — har username ka password yahan save hoga
    // e.g. username "Ali" ka password: PlayerPrefs.GetString("PREF_USER_Ali")
    const string USER_PREFIX = "PREF_USER_";

    enum AuthMode { Register, Login }
    AuthMode _mode;

    Button    _tabRegister, _tabLogin, _submitBtn, _forgotBtn;
    TextField _userField, _passField, _confirmField;
    Label     _labelConfirm, _feedback, _submitLabel;

    // ── Start ─────────────────────────────────────────────

    void OnEnable()
    {
        var root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root == null) { Debug.LogError("[AuthUI] rootVisualElement null hai."); return; }

        // Elements find karo
        _tabRegister  = root.Q<Button>("tab-register");
        _tabLogin     = root.Q<Button>("tab-login");
        _submitBtn    = root.Q<Button>("btn-submit");
        _forgotBtn    = root.Q<Button>("btn-forgot");
        _userField    = root.Q<TextField>("input-username");
        _passField    = root.Q<TextField>("input-password");
        _confirmField = root.Q<TextField>("input-confirm");
        _labelConfirm = root.Q<Label>("label-confirm");
        _feedback     = root.Q<Label>("feedback-label");
        _submitLabel  = root.Q<Label>("submit-label");

        // Password fields mask karo (*** dikhaye)
        if (_passField    != null) _passField.isPasswordField    = true;
        if (_confirmField != null) _confirmField.isPasswordField = true;

        // Placeholder text
        TrySetPlaceholder(_userField, "Enter your username");
        TrySetPlaceholder(_passField, "Password");

        // Tab buttons wire karo
        if (_tabRegister != null) _tabRegister.clicked += () => SwitchMode(AuthMode.Register);
        if (_tabLogin    != null) _tabLogin.clicked    += () => SwitchMode(AuthMode.Login);
        if (_submitBtn   != null) _submitBtn.clicked   += OnSubmit;
        if (_forgotBtn   != null) _forgotBtn.clicked   += OnForgotPassword;

        // Scene ke naam se decide karo kaunsa tab default ho
        bool startOnLogin = SceneManager.GetActiveScene().name == "LoginScene";
        SwitchMode(startOnLogin ? AuthMode.Login : AuthMode.Register);

        // Keyboard: Enter submits, Tab cycles fields
        root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

        // Auto-focus first field (slight delay so UI finishes layout)
        root.schedule.Execute(() => _userField?.Focus()).StartingIn(80);
    }

    // ── Keyboard input ───────────────────────────────────

    void OnKeyDown(KeyDownEvent e)
    {
        if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
        {
            OnSubmit();
            e.StopPropagation();
            return;
        }
        if (e.keyCode == KeyCode.Tab)
        {
            CycleFieldFocus(e.shiftKey);
            e.StopPropagation();
        }
    }

    void CycleFieldFocus(bool backwards)
    {
        var fields = new System.Collections.Generic.List<TextField> { _userField, _passField };
        if (_mode == AuthMode.Register && _confirmField != null)
            fields.Add(_confirmField);

        // Find currently focused field index
        int cur = 0;
        var focusedEl = fields[0]?.panel?.focusController?.focusedElement;
        for (int i = 0; i < fields.Count; i++)
            if (fields[i] != null && (focusedEl == fields[i] || IsChildOf(focusedEl, fields[i])))
            { cur = i; break; }

        int next = backwards
            ? (cur - 1 + fields.Count) % fields.Count
            : (cur + 1) % fields.Count;
        fields[next]?.Focus();
    }

    static bool IsChildOf(UnityEngine.UIElements.Focusable el, UnityEngine.UIElements.VisualElement parent)
    {
        if (el == null || parent == null) return false;
        var ve = el as UnityEngine.UIElements.VisualElement;
        while (ve != null) { if (ve == parent) return true; ve = ve.parent; }
        return false;
    }

    // ── Tab toggle ────────────────────────────────────────

    void SwitchMode(AuthMode mode)
    {
        _mode = mode;
        ClearFeedback();
        ClearFields();

        bool isRegister = (mode == AuthMode.Register);

        // Confirm password field sirf Register mein dikhao
        SetVisible(_labelConfirm, isRegister);
        SetVisible(_confirmField, isRegister);

        // Submit button ka text badlo
        if (_submitLabel != null)
            _submitLabel.text = isRegister ? "CREATE ACCOUNT" : "LOGIN";

        // Forgot password button sirf Login mein dikhao
        SetVisible(_forgotBtn, !isRegister);

        // Tab highlight update karo
        SetTabHighlight(_tabRegister,  isRegister);
        SetTabHighlight(_tabLogin,    !isRegister);
    }

    void SetTabHighlight(Button tab, bool active)
    {
        if (tab == null) return;
        if (active) tab.AddToClassList("tab-active");
        else        tab.RemoveFromClassList("tab-active");
    }

    void SetVisible(VisualElement el, bool show)
    {
        if (el != null)
            el.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void ClearFields()
    {
        if (_userField    != null) _userField.value    = "";
        if (_passField    != null) _passField.value    = "";
        if (_confirmField != null) _confirmField.value = "";
    }

    // ── Feedback message ──────────────────────────────────

    void ClearFeedback()
    {
        if (_feedback == null) return;
        _feedback.text = "";
        _feedback.RemoveFromClassList("feedback-error");
        _feedback.RemoveFromClassList("feedback-success");
        _feedback.RemoveFromClassList("feedback-loading");
        _feedback.RemoveFromClassList("feedback-reveal");
    }

    void ShowFeedback(string msg, bool success)
    {
        if (_feedback == null) return;
        _feedback.text = msg;
        _feedback.RemoveFromClassList("feedback-error");
        _feedback.RemoveFromClassList("feedback-success");
        _feedback.RemoveFromClassList("feedback-loading");
        _feedback.RemoveFromClassList("feedback-reveal");
        _feedback.AddToClassList(success ? "feedback-success" : "feedback-error");
    }

    // ── Submit button press ───────────────────────────────

    void OnSubmit()
    {
        string username = _userField?.value.Trim() ?? "";
        string password = _passField?.value ?? "";

        // Common validation
        if (username.Length < 3)
        {
            ShowFeedback("Username must be at least 3 characters.", false);
            return;
        }

        if (password.Length < 4)
        {
            ShowFeedback("Password must be at least 4 characters.", false);
            return;
        }

        if (_mode == AuthMode.Register)
            DoRegister(username, password);
        else
            DoLogin(username, password);
    }

    // ── Register logic (PlayerPrefs) ──────────────────────

    void DoRegister(string username, string password)
    {
        string confirm = _confirmField?.value ?? "";

        if (password != confirm)
        {
            ShowFeedback("Passwords do not match. Please try again.", false);
            return;
        }

        string prefKey = USER_PREFIX + username;

        if (PlayerPrefs.HasKey(prefKey))
        {
            ShowFeedback("This username is already taken. Please choose another.", false);
            return;
        }

        PlayerPrefs.SetString(prefKey, password);
        PlayerPrefs.SetString("SavedName", username);
        PlayerPrefs.Save();

        ShowFeedback("Account created! Please login.", true);

        // 1.2 second baad Login tab pe switch karo
        StartCoroutine(DelayedSwitch(1.2f, AuthMode.Login));
    }

    // ── Login logic (PlayerPrefs) ─────────────────────────

    void DoLogin(string username, string password)
    {
        string prefKey = USER_PREFIX + username;

        if (!PlayerPrefs.HasKey(prefKey))
        {
            ShowFeedback("Username not found. Please register first.", false);
            return;
        }

        string savedPassword = PlayerPrefs.GetString(prefKey);
        if (savedPassword != password)
        {
            ShowFeedback("Incorrect password. Please try again.", false);
            return;
        }

        PlayerPrefs.SetString("CurrentPlayer", username);
        PlayerPrefs.Save();

        ShowFeedback("Login successful! Welcome, " + username, true);
        StartCoroutine(DelayedLoad(0.9f, "MainMenu"));
    }

    // ── Coroutine helpers ─────────────────────────────────

    IEnumerator DelayedSwitch(float delay, AuthMode mode)
    {
        yield return new WaitForSeconds(delay);
        SwitchMode(mode);
    }

    IEnumerator DelayedLoad(float delay, string sceneName)
    {
        yield return new WaitForSeconds(delay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // ── Forgot Password ───────────────────────────────────

    void OnForgotPassword()
    {
        string username = _userField?.value.Trim() ?? "";

        if (username.Length < 3)
        {
            ShowFeedback("Enter your username above, then press FORGOT PASSWORD.", false);
            return;
        }

        string prefKey = USER_PREFIX + username;

        if (!PlayerPrefs.HasKey(prefKey))
        {
            ShowFeedback("No account found for \"" + username + "\". Please register first.", false);
            return;
        }

        string savedPassword = PlayerPrefs.GetString(prefKey);
        ShowFeedbackReveal("Password for \"" + username + "\": " + savedPassword);
    }

    void ShowFeedbackReveal(string msg)
    {
        if (_feedback == null) return;
        ClearFeedback();
        _feedback.text = msg;
        _feedback.AddToClassList("feedback-reveal");
    }

    // ── Placeholder helper ────────────────────────────────

    static void TrySetPlaceholder(TextField field, string text)
    {
        if (field == null) return;
        try { field.textEdition.placeholder = text; } catch { }
    }
}
