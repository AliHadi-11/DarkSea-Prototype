using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

// Static singleton — kahin se bhi call karo:
//   NotificationUI.Show("ENEMY APPROACHING!", NotificationUI.NotifType.Danger);
[RequireComponent(typeof(UIDocument))]
public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }

    VisualElement container;
    bool initialized;

    public enum NotifType { Kill, Warning, Danger }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        EnsureInitialized();
    }

    bool EnsureInitialized()
    {
        if (initialized) return container != null;
        var doc = GetComponent<UIDocument>();
        if (doc == null) return false;
        var root = doc.rootVisualElement;
        if (root == null) return false;
        container = root.Q<VisualElement>("notif-container");
        if (container == null) Debug.LogError("[NotificationUI] 'notif-container' not found in NotificationPanel.uxml");
        initialized = true;
        return container != null;
    }

    public static void Show(string message, NotifType type = NotifType.Warning)
    {
        if (Instance == null) return;
        if (!Instance.EnsureInitialized()) return;
        Instance.StartCoroutine(Instance.ShowCard(message, type));
    }

    IEnumerator ShowCard(string message, NotifType type)
    {
        if (container == null) yield break;

        var card = new VisualElement();
        card.AddToClassList("notif-card");
        card.AddToClassList("notif-" + type.ToString().ToLower());
        card.pickingMode = PickingMode.Ignore;

        var dot = new VisualElement();
        dot.AddToClassList("notif-dot");
        dot.pickingMode = PickingMode.Ignore;

        var text = new Label(message);
        text.AddToClassList("notif-text");
        text.pickingMode = PickingMode.Ignore;

        card.Add(dot);
        card.Add(text);
        container.Add(card);

        // Ek frame wait karo taake initial state (opacity:0, margin-right:-300px) apply ho
        yield return null;

        // Slide in + fade in
        card.AddToClassList("notif-show");

        yield return new WaitForSecondsRealtime(2.8f);

        // Slide out + fade out
        card.RemoveFromClassList("notif-show");

        yield return new WaitForSecondsRealtime(0.4f);

        if (container.Contains(card))
            container.Remove(card);
    }
}
