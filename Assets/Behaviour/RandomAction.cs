using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Trigger Random Attack Delayed",
        description: "Sets random attack index, waits one frame, then triggers attack with cooldown.",
        story: "Trigger random attack on [Animator] with trigger [TriggerName] after delay with [AttackCooldown] seconds cooldown",
        category: "Action/Animation",
        id: "33f8bb6a3f9bc4606c6613be45ad709f")]
    internal partial class TriggerRandomAttackDelayedAction : Action
    {
        [SerializeReference] public BlackboardVariable<string> TriggerName;
        [SerializeReference] public BlackboardVariable<Animator> Animator;
        [SerializeReference] public BlackboardVariable<float> AttackCooldown = new BlackboardVariable<float>(1.0f);
        
        private bool hasSetIndex = false;
        private int selectedAttackIndex = 0;
        private static float lastAttackTime = -999f;

        protected override Status OnStart()
        {
            if (Animator.Value == null)
            {
                LogFailure("No Animator set.");
                return Status.Failure;
            }

            // Check cooldown first
            float timeSinceLastAttack = Time.time - lastAttackTime;
            if (timeSinceLastAttack < AttackCooldown.Value)
            {
                Debug.Log($"Attack on cooldown. Time remaining: {AttackCooldown.Value - timeSinceLastAttack:F1}s");
                return Status.Success; // Changed to Success - cooldown is not a failure
            }

            // Select and set attack index on first frame (only once per attack)
            selectedAttackIndex = UnityEngine.Random.Range(0, 8);
            Animator.Value.SetInteger("AttackIndex", selectedAttackIndex);
            hasSetIndex = true;
            
            Debug.Log($"Set AttackIndex to: {selectedAttackIndex}");
            
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (hasSetIndex)
            {
                // Trigger attack on next frame
                Animator.Value.SetTrigger(TriggerName.Value);
                Debug.Log($"Triggered: {TriggerName.Value}");
                
                // Update cooldown timer
                lastAttackTime = Time.time;
                
                return Status.Success;
            }
            
            return Status.Running;
        }

        protected override void OnEnd()
        {
            hasSetIndex = false;
            selectedAttackIndex = 0;
        }
    }
}