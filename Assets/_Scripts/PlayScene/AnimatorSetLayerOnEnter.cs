using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class AnimatorSetLayerOnEnter : StateMachineBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("AttackLeft", false);
            animator.SetBool("AttackRight", false);
            animator.SetLayerWeight(layerIndex, 0);
        }
    }
}