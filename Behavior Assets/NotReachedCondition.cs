using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "NotReached", story: "[Agent] not yet reached [target]", category: "Conditions", id: "395ec21e311eb1ba42fcca3c50d7a380")]
public partial class NotReachedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeField] private float stopDistance = 1.0f;
    public override bool IsTrue()
    {

        Vector3 selfPos = Agent.Value.transform.position;
        Vector3 targetPos = Target.Value.transform.position;

        float sqrDistance = (selfPos - targetPos).sqrMagnitude;
        Debug.Log($"Distance to Pickup Point? {sqrDistance} > {stopDistance * stopDistance}");
        return sqrDistance > stopDistance * stopDistance;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
