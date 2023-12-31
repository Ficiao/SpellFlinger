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


        public static void AnimationUpdate(bool isGrounded, bool isDead, int leftRightDirection, int forwardDirection, ref PlayerAnimationState animationState, Animator animator, Transform modelTransform)
        {
            LeftRightDirection leftRightDirectionType = (LeftRightDirection)leftRightDirection;
            ForwardDirection forwardDirectionType = (ForwardDirection)forwardDirection;
            modelTransform.rotation = Quaternion.identity;

            if (isDead)
            {
                if (animationState != PlayerAnimationState.Dead)
                {
                    animationState = PlayerAnimationState.Dead;
                    animator.SetTrigger(animationState.ToString());
                }
                return;
            }

            if (!isGrounded)
            {
                if (animationState != PlayerAnimationState.Jumping)
                {
                    animationState = PlayerAnimationState.Jumping;
                    animator.SetTrigger(animationState.ToString());
                }
                return;
            }

            if(leftRightDirectionType == LeftRightDirection.None && forwardDirectionType == ForwardDirection.None)
            {
                if(animationState != PlayerAnimationState.Idle)
                {
                    animationState = PlayerAnimationState.Idle;
                    animator.SetTrigger(animationState.ToString());
                }

                return;
            }

            if(forwardDirectionType == ForwardDirection.Forward)
            {
                if (animationState != PlayerAnimationState.RunForward)
                {
                    animationState = PlayerAnimationState.RunForward;
                    animator.SetTrigger(animationState.ToString());
                }

                if (leftRightDirectionType == LeftRightDirection.Left) modelTransform.rotation = Quaternion.Euler(0, -35, 0);
                else if (leftRightDirectionType == LeftRightDirection.Right) modelTransform.rotation = Quaternion.Euler(0, 35, 0);
                else modelTransform.rotation = Quaternion.identity;

                return;
            }

            if (forwardDirectionType == ForwardDirection.Backward)
            {
                if (animationState != PlayerAnimationState.RunBack)
                {
                    animationState = PlayerAnimationState.RunBack;
                    animator.SetTrigger(animationState.ToString());
                }

                if (leftRightDirectionType == LeftRightDirection.Left) modelTransform.rotation = Quaternion.Euler(0, 35, 0);
                else if (leftRightDirectionType == LeftRightDirection.Right) modelTransform.rotation = Quaternion.Euler(0, -35, 0);
                else modelTransform.rotation = Quaternion.identity;

                return;
            }

            if(leftRightDirectionType == LeftRightDirection.Left && animationState != PlayerAnimationState.StrafeLeft)
            {
                animationState = PlayerAnimationState.StrafeLeft;
                animator.SetTrigger(animationState.ToString());
                return;
            }

            if (leftRightDirectionType == LeftRightDirection.Right && animationState != PlayerAnimationState.StrafeRight)
            {
                animationState = PlayerAnimationState.StrafeRight;
                animator.SetTrigger(animationState.ToString());
                return;
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
