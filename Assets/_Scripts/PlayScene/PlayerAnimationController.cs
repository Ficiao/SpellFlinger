using UnityEngine;
using SpellFlinger.Enum;

namespace SpellFlinger.PlayScene
{
    public static class PlayerAnimationController
    {
        private static float _currentAngle = 0;
        private static float _deltaAngle = 0.35f;
        private static float _lerpCutoff = 0.01f;
        private static bool _isLeftAttack = false;

        public static void SetDeadState(ref PlayerAnimationState animationState, Animator animator)
        {
            if (animationState != PlayerAnimationState.Dead)
            {
                animationState = PlayerAnimationState.Dead;
                animator.SetTrigger(animationState.ToString());
            }
        }

        public static void SetIdleState(ref PlayerAnimationState animationState, Animator animator)
        {
            animationState = PlayerAnimationState.Idle;
            animator.SetTrigger(animationState.ToString());
        }

        public static void PlayShootAnimation(Animator animator)
        {
            animator.SetLayerWeight(1, 1);
            if (_isLeftAttack)
            {
                animator.SetTrigger("AttackLeft");
                _isLeftAttack = false;
            }
            else
            {
                animator.SetTrigger("AttackRight");
                _isLeftAttack = true;
            }
        }

        public static void AnimationUpdate(bool isGrounded, int leftRightDirection, int forwardDirection, ref PlayerAnimationState animationState, Animator animator, Transform modelTransform, Transform referenceTransform)
        {
            modelTransform.rotation = referenceTransform.rotation;
            float rotation;
            PlayerAnimationState newPlayerAnimationState;

            switch ((leftRightDirection, forwardDirection))
            {
                case (1, 1):
                    rotation = 40f;
                    newPlayerAnimationState = PlayerAnimationState.RunForward;
                    break;
                case (1, 0):
                    rotation = 90f;
                    newPlayerAnimationState = PlayerAnimationState.RunForward;
                    break;
                case (1, -1):
                    rotation = -40f;
                    newPlayerAnimationState = PlayerAnimationState.RunBack;
                    break;
                case (0, 1):
                    rotation = 0f;
                    newPlayerAnimationState = PlayerAnimationState.RunForward;
                    break;
                case (0, 0):
                    rotation = 0f;
                    newPlayerAnimationState = PlayerAnimationState.Idle;
                    break;
                case (0, -1):
                    rotation = 0f;
                    newPlayerAnimationState = PlayerAnimationState.RunBack;
                    break;
                case (-1, 1):
                    rotation = -40f;
                    newPlayerAnimationState = PlayerAnimationState.RunForward;
                    break;
                case (-1, 0):
                    rotation = -90f;
                    newPlayerAnimationState = PlayerAnimationState.RunForward;
                    break;
                case (-1, -1):
                    rotation = 40f;
                    newPlayerAnimationState = PlayerAnimationState.RunBack;
                    break;
                default:
                    rotation = 0f;
                    newPlayerAnimationState = PlayerAnimationState.Idle;
                    break;
            }

            if (!isGrounded) newPlayerAnimationState = PlayerAnimationState.Jumping;

            if(Mathf.Abs(_currentAngle - rotation) < _lerpCutoff) _currentAngle = rotation;
            else _currentAngle = Mathf.Lerp(_currentAngle, rotation, _deltaAngle);

            ApplyAnimation(newPlayerAnimationState, ref animationState, _currentAngle, animator, modelTransform);
        }

        private static void ApplyAnimation(PlayerAnimationState playerAnimation, ref PlayerAnimationState currentAnimation, float rotation, Animator animator, Transform modelTransform)
        {
            if(currentAnimation != playerAnimation)
            {
                currentAnimation = playerAnimation;
                animator.SetTrigger(currentAnimation.ToString());
            }

            modelTransform.Rotate(0, rotation, 0);
        }
    }
}
