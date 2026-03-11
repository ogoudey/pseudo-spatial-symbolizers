using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "notYetPickedUp", story: "Agent not yet [payloadPickedUp]", category: "Conditions", id: "5fbcae32605db68f9dbdb751040dd3c0")]
public partial class NotYetPickedUpCondition : Condition
{
    [SerializeReference] public BlackboardVariable<bool> PayloadPickedUp;

    public override bool IsTrue()
    {
        return !PayloadPickedUp.Value;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
