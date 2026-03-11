using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "HasTask", story: "Agent has [task]", category: "Conditions", id: "3e281c0c5fb7790f9a922e0fbb0d0d91")]
public partial class HasTaskCondition : Condition
{
    [SerializeReference] public BlackboardVariable<bool> Task;

    public override bool IsTrue()
    {
        return Task.Value;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
