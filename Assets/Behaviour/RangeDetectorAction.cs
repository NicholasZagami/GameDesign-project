using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Range Detector", story: "Update Range [Detector] and assign [Target]", category: "Action", id: "1cd0f6fa673a8bf797ca5f1d4ed9842c")]
public partial class RangeDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<RangeDetector> Detector;
    [SerializeReference] public BlackboardVariable<GameObject> Target;


    protected override Status OnUpdate()
    {
        Target.Value = Detector.Value.UpdateDetector();
        return Detector.Value.UpdateDetector() == null ? Status.Failure : Status.Success;
    }

}

