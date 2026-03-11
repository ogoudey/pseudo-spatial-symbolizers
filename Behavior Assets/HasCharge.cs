using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "HasCharge", story: "Agent has enough [charge] to perform main loop", category: "Variable Conditions", id: "dc1ef0d32e44f89103d75c2298ae4619")]
public partial class HasCharge : Condition
{
    [SerializeReference] public BlackboardVariable<float> Charge;

    public override bool IsTrue()
    {

        UnityEngine.Debug.Log($"Has charge? {Charge > 20}");
        return Charge > 20;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
