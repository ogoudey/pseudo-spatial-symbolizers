using UnityEngine;

public class ATM : MonoBehaviour
{
    [Header("Activation settings")]
    public float interactRadius = 2f;

    [Tooltip("Lever animator parameter name (must match exactly)")]
    void Update()
    {
        // Check for player proximity
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius);
        bool playerNearby = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerNearby = true;
                break;
            }
        }

        // If player is nearby and presses "E", win
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Logger.PlayerWin();
        }
    }
}