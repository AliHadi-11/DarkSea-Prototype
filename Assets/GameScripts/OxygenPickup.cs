using UnityEngine;

public class OxygenPickup : MonoBehaviour
{
    public float refillAmount = 40f; // Kitni oxygen milegi?

    private void OnTriggerEnter(Collider other)
    {
        // Check karo ke takranay wala "Player" hai?
        if (other.CompareTag("Player"))
        {
            // Player ke upar se "OxygenSystem" script dhoondo
            OxygenSystem playerOxygen = other.GetComponentInParent<OxygenSystem>();

            if (playerOxygen != null)
            {
                // Oxygen barhao
                playerOxygen.RefillOxygen(refillAmount);

                // Sound effect (Optional - agar lagana ho)
                // AudioSource.PlayClipAtPoint(sound, transform.position);

                // Cylinder ko dunya se mita do (Ghayab)
                Destroy(gameObject);
            }
        }
    }
}