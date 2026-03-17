using UnityEngine;
using System.Collections.Generic;

public class PressurePlate : MonoBehaviour
{
    [Header("Gate to control")]
    public GameObject gate;

    [Header("Activation settings")]
    public float triggerRadius = 1f;
    
    [Tooltip("Animator parameter name (must match exactly)")]
    public string animatorParameterName = "Open";

    [Tooltip("Objects that are allowed to activate this plate")]
    public List<GameObject> allowedObjects = new List<GameObject>();

    public bool isActive = false;

    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, triggerRadius);

        bool platePressed = false;
        string agent = "";
        foreach (var hit in hits)
        {
            // Check if the hit object OR one of its parents is in allowedObjects
            foreach (var allowed in allowedObjects)
            {
                if (allowed == null) continue;

                if (hit.transform == allowed.transform ||
                    hit.transform.IsChildOf(allowed.transform))
                {
                    agent = hit.gameObject.name;
                    platePressed = true;
                    break;
                }
            }

            if (platePressed)
                break;
        }

        if (platePressed && !isActive)
        {
            Logger.Log($"EVENT", $"{gameObject.name} pressed by {agent}");
            isActive = true;
            OpenGate();
        }
        else if (!platePressed && isActive)
        {
            Logger.Log($"EVENT", $"{gameObject.name} unpressed");
            isActive = false;
            CloseGate();
        }
    }


    void OpenGate()
    {
        // Example: use Animator trigger if gate has Animator
        Animator anim = gate.GetComponent<Animator>();
        if (anim) anim.SetBool(animatorParameterName, true);
        else gate.SetActive(false); // fallback: hide
    }

    void CloseGate()
    {
        Animator anim = gate.GetComponent<Animator>();
        if (anim) anim.SetBool(animatorParameterName, false);
        else gate.SetActive(true); // fallback: show
    }
}
