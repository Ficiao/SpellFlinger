using UnityEngine;
using SpellFlinger.Enum;

namespace SpellFlinger.PlayScene
{
    public static class PlayerAnimationController
    {
        private static float _currentAngle = 0;
        private static float _deltaAngle = 0.3f;
        private static float _lerpCutoff = 0.01f;
        private static bool _isLeftAttack = false;
        private static int _animationStateParameterId = 0;

        public static void Init(ref PlayerAnimationState animationState, Animator animator)
        {
            _animationStateParameterId = Animator.StringToHash("AnimationState");
            animationState = PlayerAnimationState.Idle;
            animator.SetInteger(_animationStateParameterId, (int)animationState);
        }

        public static void SetDeadState(ref PlayerAnimationState animationState, Animator animator)
        {
            animationState = PlayerAnimationState.Dead;
            animator.SetBool("DeadState", true);
            animator.SetInteger(_animationStateParameterId, (int)animationState);
        }

        public static void SetAliveState(ref PlayerAnimationState animationState, Animator animator)
        {
            animationState = PlayerAnimationState.Idle;
            animator.SetBool("DeadState", false);
            animator.SetInteger(_animationStateParameterId, (int)animationState);
        }

        public static void PlayShootAnimation(Animator animator)
        {
            animator.SetLayerWeight(1, 1);
            if (_isLeftAttack)
            {
                animator.SetBool("AttackLeft", true);
                animator.SetBool("AttackRight", false);
                _isLeftAttack = false;
            }
            else
            {
                animator.SetBool("AttackLeft", false);
                animator.SetBool("AttackRight", true);
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
                animator.SetInteger(_animationStateParameterId, (int)currentAnimation);
            }

            modelTransform.Rotate(0, rotation, 0);
        }
    }
}
