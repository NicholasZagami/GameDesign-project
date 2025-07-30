using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Trigger Random Attack",
        description: "Sets random attack index and triggers attack with cooldown.",
        story: "Trigger random attack on [Animator] with trigger [TriggerName] and [AttackCooldown] seconds cooldown",
        category: "Action/Animation",
        id: "33f8bb6a3f9bc4606c6613be45ad708f")]
    internal partial class TriggerRandomAttackAction : Action
    {
        [SerializeReference] public BlackboardVariable<string> TriggerName;
        [SerializeReference] public BlackboardVariable<Animator> Animator;
        [SerializeReference] public BlackboardVariable<float> AttackCooldown = new BlackboardVariable<float>(1.0f);

        private static float lastAttackTime = 0f;

        protected override Status OnStart()
        {
            if (Animator.Value == null)
            {
                LogFailure("No Animator set.");
                return Status.Failure;
            }

            // Check if enough time has passed since last attack
            if (Time.time - lastAttackTime < AttackCooldown.Value)
            {
                return Status.Failure; // Attack is on cooldown
            }

            // Set random attack index (1-8)
            int randomAttackIndex = UnityEngine.Random.Range(1, 9);
            Animator.Value.SetInteger("AttackIndex", randomAttackIndex);

            // Trigger the attack
            Animator.Value.SetTrigger(TriggerName.Value);

            // Update last attack time
            lastAttackTime = Time.time;

            return Status.Success;
        }
    }
}