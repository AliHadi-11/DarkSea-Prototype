using UnityEngine;
using TMPro;
using System.Collections;

// =========================================================
//  Dark Sea - On-Screen Message System
//  Console ke Debug.Log ki tarah, par screen par dikhta hai
//  aur thodi der baad apne aap gayab ho jata hai.
//
//  Kahin se bhi call karo:  GameMessage.Show("Tank collected!");
//
//  Setup (HAR level mein):
//   1. Canvas par right-click > UI > Text - TextMeshPro -> naam "MessageText"
//      (screen ke center-neeche rakho, font size thoda bada)
//   2. Canvas par yeh script (GameMessage) Add Component karo
//   3. Message Text field mein MessageText drag karo
// =========================================================
public class GameMessage : MonoBehaviour
{
    public static GameMessage instance;

    [Header("UI")]
    public TMP_Text messageText;   // center-neeche wala text
    public float showTime = 2.5f;  // kitni der dikhega (seconds)

    private Coroutine hideRoutine;

    void Awake()
    {
        instance = this;
        if (messageText != null) messageText.text = "";
    }

    // Static call - kahin se bhi: GameMessage.Show("...")
    public static void Show(string msg)
    {
        if (instance != null) instance.Display(msg);
        else Debug.Log("[GameMessage] " + msg); // agar setup nahi, console par
    }

    void Display(string msg)
    {
        if (messageText == null) return;
        messageText.text = msg;

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfter());
    }

    IEnumerator HideAfter()
    {
        // Realtime taake timeScale = 0 (pause) par bhi sahi chale
        yield return new WaitForSecondsRealtime(showTime);
        if (messageText != null) messageText.text = "";
    }
}