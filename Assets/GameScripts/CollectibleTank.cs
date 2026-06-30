using UnityEngine;
using TMPro;

// =========================================================
//  Dark Sea - Collectible Mission Tank (Level 2)
//  Yeh un 3 "hidden oxygen tanks" ke liye hai jo Level 2 mein
//  dhoondne hain. Yeh oxygen bhi refill karta hai AUR ginti
//  (counter) bhi barhata hai taake exit gate khul sake.
//
//  Setup:
//   - Tank GameObject par Collider lagao + "Is Trigger" tick.
//   - Yeh script tank par lagao.
//   - Level 2 ke Exit Gate ke LevelExit script mein
//     "Require Tanks" tick karo aur Required Tanks = 3 rakho.
//
//  (Normal refill tanks ke liye purani OxygenPickup.cs use karo.
//   Yeh sirf mission-objective tanks ke liye hai.)
// =========================================================

// Saari tanks ki ginti rakhne wala simple counter
public static class TankCollector
{
    public static int Collected = 0;
}

public class CollectibleTank : MonoBehaviour
{
    [Header("Oxygen")]
    public float refillAmount = 30f;    // Tank uthane par kitni oxygen milegi

    [Header("UI (Optional)")]
    public TMP_Text tankCounterText;    // "Tanks: 1/3" dikhane ke liye (optional)
    public int totalTanks = 3;

    void Start()
    {
        // Show starting count so the player sees "Tanks: 0/3" immediately
        if (tankCounterText != null)
            tankCounterText.text = "Tanks: " + TankCollector.Collected + "/" + totalTanks;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 1. Ginti barhao
        TankCollector.Collected++;

        // 2. Oxygen refill karo
        OxygenSystem ox = other.GetComponentInParent<OxygenSystem>();
        if (ox != null) ox.RefillOxygen(refillAmount);

        // 3. UI update (agar laga hua hai)
        if (tankCounterText != null)
            tankCounterText.text = "Tanks: " + TankCollector.Collected + "/" + totalTanks;

        Debug.Log("Tank collected! (" + TankCollector.Collected + "/" + totalTanks + ")");

        // 4. Tank ghayab
        Destroy(gameObject);
    }
}