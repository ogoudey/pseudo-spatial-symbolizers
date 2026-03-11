using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;

public class Interact : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float reachRadius = 1.5f;
    public Transform holdPoint;
    public string interactableTag = "Interactable";
    public string electricTag = "Electric";

    private Rigidbody heldObject;
    private List<Rigidbody> rigidBodies;

    private List<Transform> electrics;
    public int port = 5007;
    private JsonTcpServer server;
    private ConcurrentQueue<InputMsg> inboundMessageQueue = new();
    private ConcurrentQueue<OutputMsg> outboundMessageQueue = new();

    void Start()
    {
        rigidBodies = GetObjectsWithinReach();
        // Communication
        server = new JsonTcpServer(port, outboundMessageQueue);
        server.OnMessageReceived += msg =>
        {
            // background thread → queue
            inboundMessageQueue.Enqueue(msg);
        };
        server.Start();
    }

    /// <summary>
    /// Returns all Interactable objects within reach.
    /// </summary>
    public List<Rigidbody> GetObjectsWithinReach()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, reachRadius);
        List<Rigidbody> results = new List<Rigidbody>();

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(interactableTag))
                continue;

            Rigidbody rb = hit.attachedRigidbody;
            if (rb != null && rb != heldObject)
                results.Add(rb);
        }

        return results;
    }

    /// <summary>
    /// Returns all Interactable objects within reach.
    /// </summary>
    public List<Transform> GetElectricsWithinReach()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, reachRadius);
        List<Transform> results = new List<Transform>();

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(electricTag))
                continue;

            Transform t = hit.transform;
            if (t != null && t != heldObject)
                results.Add(t);
        }

        return results;
    }

    /// <summary>
    /// Picks up the given rigidbody if possible.
    /// </summary>
    public bool PickUp(Rigidbody target)
    {
        if (heldObject != null)
            return false;

        if (target == null || !target.CompareTag(interactableTag))
            return false;
        Debug.Log($"Picking up {target.gameObject.name} at {target.gameObject.transform}");
        heldObject = target;

        heldObject.isKinematic = true;
        heldObject.detectCollisions = false;

        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        return true;
    }

    void Update()
    {
        // Communications
        while (inboundMessageQueue.TryDequeue(out var msg))
        {
            //Debug.Log($"Interact received method={msg.method}, arg={msg.arg}");
            if (msg.method == "PickUp")
            {
                rigidBodies = GetObjectsWithinReach();
                Rigidbody targetRigidBody = rigidBodies.Find(rb => rb.gameObject.name == msg.arg);
                bool success = PickUp(targetRigidBody);
                if (success){
                    outboundMessageQueue.Enqueue(new OutputMsg { type = "status", content = new string[] { $"picked up {msg.arg}" } });
                } else {
                    outboundMessageQueue.Enqueue(new OutputMsg { type = "status", content = new string[] { $"failed to pick up {msg.arg}" } });
                }
            }
            if (msg.method == "Switch")
            {
                electrics = GetElectricsWithinReach();
                Debug.Log($"Finding {msg.arg}");
                Transform targetT = electrics.Find(t => t.gameObject.name == msg.arg);
                Lever lever = targetT.GetComponent<Lever>();
                bool isNowOpen = lever.Activate();
                if (isNowOpen){
                    outboundMessageQueue.Enqueue(new OutputMsg { type = "status", content = new string[] { $"switched {msg.arg} to keep gate open" } });
                } else {
                    outboundMessageQueue.Enqueue(new OutputMsg { type = "status", content = new string[] { $"switched {msg.arg} to OFF" } });
                }
            }
            if (msg.method == "Drop")
            {
                UnityEngine.Debug.Log($"dropping {heldObject}");
                
                outboundMessageQueue.Enqueue(new OutputMsg { type = "status", content = new string[] { $"dropped {heldObject}" } });
                Drop();
            }
            if (msg.method == "GetAvailableObjects")
            {
                
                rigidBodies = GetObjectsWithinReach();
                //Debug.Log($"Got rigidbodies [{string.Join(", ", rigidBodies.ConvertAll(rb => rb.gameObject.name))}]");
                string[] names = new string[rigidBodies.Count];
                for (int i = 0; i < rigidBodies.Count; i++)
                {
                    names[i] = rigidBodies[i].gameObject.name;
                }

                electrics = GetElectricsWithinReach();
                string[] eNames = new string[electrics.Count];
                for (int i = 0; i < electrics.Count; i++)
                {
                    Animator leverAnimator = electrics[i].gameObject.GetComponent<Animator>();
                    bool isDown = leverAnimator.GetBool("Down");
                    string extraInfo;
                    if (leverAnimator.GetBool("Down"))
                    {
                        extraInfo = "Pulled (down)";
                    }
                    else
                    {
                        extraInfo = "Unpulled (closed)";
                    }
                    eNames[i] = electrics[i].gameObject.name + extraInfo;
                    
                }
                //Debug.Log($"Got names [{string.Join(", ", names)}]");
                outboundMessageQueue.Enqueue(new OutputMsg { type = "available_objects", content = names});
                //Debug.Log($"Got eNames [{string.Join(", ", eNames)}]");
                outboundMessageQueue.Enqueue(new OutputMsg { type = "available_electric_objects", content = eNames});
            }
        }
    }

    /// <summary>
    /// Drops the currently held object.
    /// </summary>
    public void Drop()
    {
        if (heldObject == null)
            return;

        heldObject.transform.SetParent(null);

        heldObject.isKinematic = false;
        heldObject.detectCollisions = true;

        heldObject = null;
    }

    /// <summary>
    /// Helper: picks up the closest interactable within reach.
    /// </summary>
    public bool PickUpClosest()
    {   
        List<Rigidbody> objects = GetObjectsWithinReach();
        if (objects.Count == 0)
            return false;

        Rigidbody closest = null;
        float bestDist = float.MaxValue;

        foreach (Rigidbody rb in objects)
        {
            float d = Vector3.Distance(transform.position, rb.position);
            if (d < bestDist)
            {
                bestDist = d;
                closest = rb;
            }
        }

        return PickUp(closest);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, reachRadius);
    }
}
