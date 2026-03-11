using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Gate to control")]
    public GameObject gate;

    [Header("Activation settings")]
    public float interactRadius = 2f;

    [Tooltip("Lever animator parameter name (must match exactly)")]
    public string leverAnimatorParameterName = "Down";

    [Tooltip("Gate animator parameter name (must match exactly)")]
    public string animatorParameterName = "LeverDown";

    private bool isPulled = false;

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

        // If player is nearby and presses "E", pull lever
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Activate();
        }
    }

    void OpenGate()
    {
        isPulled = true;
        Animator anim = gate.GetComponent<Animator>();
        anim.SetBool(animatorParameterName, true);
    }

    public void CloseGate()
    {
        isPulled = false;
        Animator anim = gate.GetComponent<Animator>();
        anim.SetBool(animatorParameterName, false);
        
    }

    public bool Activate()
    {
        if (isPulled) {
            Animator anim = GetComponent<Animator>();
            if (anim) anim.SetBool(leverAnimatorParameterName, false);
            CloseGate();
            isPulled = false;
        }
        else
        {
            Animator anim = GetComponent<Animator>();
            if (anim) anim.SetBool(leverAnimatorParameterName, true);
            OpenGate();
            isPulled = true;
        }
        return isPulled;
    }
}
