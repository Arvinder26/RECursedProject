using UnityEngine;
using UnityEngine.InputSystem;

/// Add this behaviour to BOTH IDLE and WALKING states.
/// Q presses latch a "forced idle" until the player starts moving.
public class ForceIdleUntilMove : StateMachineBehaviour
{
    [Header("Animator setup")]
    public string idleStateName = "IDLE";
    public string speedParam = "Speed";
    public string forcedIdleParam = "ForcedIdle";
    [Range(0f, 0.5f)] public float damp = 0.1f;

    [Header("Exit when character actually moves")]
    public bool useKeyPressToExit = true;        // W/A/S/D or Arrows
    public bool useControllerVelocityToExit = true;
    public float velocityThreshold = 0.05f;      // m/s

    private CharacterController cc;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!cc) cc = animator.GetComponentInParent<CharacterController>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var kb = Keyboard.current;

        // 1) Q toggles/latches "forced idle"
        if (kb != null && kb[Key.Q].wasPressedThisFrame)
            animator.SetBool(forcedIdleParam, true);

        bool forced = animator.GetBool(forcedIdleParam);

        if (forced)
        {
            // 2) Keep animation in IDLE and Speed at 0 while forced
            animator.SetFloat(speedParam, 0f, damp, Time.deltaTime);
            if (!stateInfo.IsName(idleStateName))
                animator.CrossFadeInFixedTime(idleStateName, 0.05f);

            // 3) Release the latch once the character moves
            bool exitByKeys = false, exitByVelocity = false;

            if (useKeyPressToExit && kb != null)
            {
                exitByKeys =
                    kb.wKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame ||
                    kb.sKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame ||
                    kb.upArrowKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame ||
                    kb.leftArrowKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame;
            }

            if (useControllerVelocityToExit && cc != null)
            {
                float horiz = new Vector3(cc.velocity.x, 0f, cc.velocity.z).magnitude;
                exitByVelocity = horiz > velocityThreshold;
            }

            if (exitByKeys || exitByVelocity)
                animator.SetBool(forcedIdleParam, false);
        }
    }
}
