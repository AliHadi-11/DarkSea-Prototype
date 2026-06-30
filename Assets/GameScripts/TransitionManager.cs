using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public float waitTime = 3f; // Screen kitni dair dikhani hai (3 second)
    public string nextSceneName; // Yahan Inspector se agle scene ka naam aayega

    void Start()
    {
        // waitTime ke mutabiq utne second baad agla scene load hoga
        Invoke("LoadNextScene", waitTime);
    }

    void LoadNextScene()
    {
        // Jo naam aap Inspector mein likhenge, yeh wo scene chala dega
        SceneManager.LoadScene(nextSceneName);
    }
}