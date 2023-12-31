using UnityEngine;
using SpellFlinger.Enum;
using System.Collections.Generic;

namespace SpellFlinger.PlayScene
{
    public static class PlayerAnimationController
    {        
        private enum LeftRightDirection
        {
            Left = -1,
            None = 0,
            Right = 1,
        }

        private enum ForwardDirection
        {
            Backward = -1,
            None = 0,
            Forward = 1,
        }

        private static Dictionary<(LeftRightDirection, ForwardDirection), PlayerAnimationState> _directionToAnimationMap = new Dictionary<(LeftRightDirection, ForwardDirection), PlayerAnimationState>
        {
            { (LeftRightDirection.None, ForwardDirection.None), PlayerAnimationState.Idle },
            { (LeftRightDirection.None, ForwardDirection.Forward), PlayerAnimationState.RunForward },
            { (LeftRightDirection.None, ForwardDirection.Backward), PlayerAnimationState.RunBack },
            { (LeftRightDirection.Left, ForwardDirection.None), PlayerAnimationState.StrafeLeft },
            { (LeftRightDirection.Left, ForwardDirection.Forward), PlayerAnimationState.StrafeForwardLeft },
            { (LeftRightDirection.Left, ForwardDirection.Backward), PlayerAnimationState.StrafeBackLeft },
            { (LeftRightDirection.Right, ForwardDirection.None), PlayerAnimationState.StrafeRight },
            { (LeftRightDirection.Right, ForwardDirection.Forward), PlayerAnimationState.StrafeForwardRight },
            { (LeftRightDirection.Right, ForwardDirection.Backward), PlayerAnimationState.StrafeForwardRight },
        };


        public static void AnimationUpdate(bool isGrounded, bool isDead, int leftRightDirection, int forwardDirection, ref PlayerAnimationState animationState, Animator animator)
        {
            if (isDead)
            {
                if (animationState != PlayerAnimationState.Dead)
                {
                    animator.SetTrigger("Dead");
                    animationState = PlayerAnimationState.Dead;
                }
                return;
            }

            if (!isGrounded)
            {
                if (animationState != PlayerAnimationState.Jumping)
                {
                    animator.SetTrigger("Jumping");
                    animationState = PlayerAnimationState.Jumping;
                }
                return;
            }

            PlayerAnimationState nextState = _directionToAnimationMap[((LeftRightDirection)leftRightDirection, (ForwardDirection)forwardDirection)];
            if (nextState != animationState)
            {
                animator.SetTrigger(nextState.ToString());
                animationState = nextState;
            }


            //if (isGrounded)
            //{
            //    switch ((leftRightDirection, forwardDirection))
            //    {
            //        case (1, 1):
            //            if(animationState != PlayerAnimationState.StrafingForwardRight)
            //            {
            //                animationState = PlayerAnimationState.StrafingForwardRight;
            //                animator.SetTrigger("StrafeForwardRight");
            //            }
            //            break;
            //        case (1, 0):
            //            if (animationState != PlayerAnimationState.StrafingRight)
            //            {
            //                animationState = PlayerAnimationState.StrafingRight;
            //                animator.SetTrigger("StrafeRight");
            //            }                        
            //            break;
            //        case (1, -1):
            //            if (animationState != PlayerAnimationState.StrafingBackRight)
            //            {
            //                animationState = PlayerAnimationState.StrafingBackRight;
            //                animator.SetTrigger("StrafeBackRight");
            //            }
            //            break;
            //        case (0, 1):
            //            if (animationState != PlayerAnimationState.RunningForward)
            //            {
            //                animationState = PlayerAnimationState.RunningForward;
            //                animator.SetTrigger("RunForward");
            //            }
            //            break;
            //        case (0, 0):
            //            if (animationState != PlayerAnimationState.Idle)
            //            {
            //                animationState = PlayerAnimationState.Idle;
            //                animator.SetTrigger("Idle");
            //            }
            //            break;
            //        case (0, -1):
            //            if (animationState != PlayerAnimationState.RunningBack)
            //            {
            //                animationState = PlayerAnimationState.RunningBack;
            //                animator.SetTrigger("RunBack");
            //            }
            //            break;
            //        case (-1, 1):
            //            if (animationState != PlayerAnimationState.StrafingForwardLeft)
            //            {
            //                animationState = PlayerAnimationState.StrafingForwardLeft;
            //                animator.SetTrigger("StrafeForwardLeft");
            //            }
            //            break;
            //        case (-1, 0):
            //            if (animationState != PlayerAnimationState.StrafingLeft)
            //            {
            //                animationState = PlayerAnimationState.StrafingLeft;
            //                animator.SetTrigger("StrafeLeft");
            //            }
            //            break;
            //        case (-1, -1):
            //            if (animationState != PlayerAnimationState.StrafingBackLeft)
            //            {
            //                animationState = PlayerAnimationState.StrafingBackLeft;
            //                animator.SetTrigger("StrafeBackLeft");
            //            }
            //            break;
            //        default:
            //            break;
            //    }
            //}
            //else
            //{
            //    if (animationState != PlayerAnimationState.Jumping)
            //    {
            //        animator.SetTrigger("Jumping", true);
            //        animationState = PlayerAnimationState.Jumping;
            //    }
            //}
        }
    }
}
