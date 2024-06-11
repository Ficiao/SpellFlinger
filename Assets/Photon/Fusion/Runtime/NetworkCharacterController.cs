namespace Fusion {
    using System;
    using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using UnityEngine;

  [StructLayout(LayoutKind.Explicit)]
  [NetworkStructWeaved(WORDS + 4)]
  public unsafe struct NetworkCCData : INetworkStruct {
    public const int WORDS = NetworkTRSPData.WORDS + 4;
    public const int SIZE  = WORDS * 4;

    [FieldOffset(0)]
    public NetworkTRSPData TRSPData;

    [FieldOffset((NetworkTRSPData.WORDS + 0) * Allocator.REPLICATE_WORD_SIZE)]
    int _grounded;

    [FieldOffset((NetworkTRSPData.WORDS + 1) * Allocator.REPLICATE_WORD_SIZE)]
    Vector3Compressed _velocityData;

    public bool Grounded {
      get => _grounded == 1;
      set => _grounded = (value ? 1 : 0);
    }

    public Vector3 Velocity {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _velocityData;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => _velocityData = value;
    }
  }

  [DisallowMultipleComponent]
  [RequireComponent(typeof(CharacterController))]
  [NetworkBehaviourWeaved(NetworkCCData.WORDS)]
  // ReSharper disable once CheckNamespace
  public sealed unsafe class NetworkCharacterController : NetworkTRSP, INetworkTRSPTeleport, IBeforeAllTicks, IAfterAllTicks, IBeforeCopyPreviousState {
    new ref NetworkCCData Data => ref ReinterpretState<NetworkCCData>();

    [Header("Character Controller Settings")]
    public float gravity = -20.0f;
    public float jumpImpulse   = 8.0f;
    public float rotationSpeed = 15.0f;
    public float moveSpeed = 0f;
    public float gravityBurst = 0f;
    public float slowAmount = 0f;
    public float doubleJumpBoost = 0f;
    public float doubleJumpDelay = 0f;
    public float slopeRaycastDistance = 0f;

    private float _squareOfTwo = Mathf.Sqrt(2);
    private float _jumpTime = 0f;
    private bool _doubleJumpAvailable = false;
    private int _updatesSinceLastGrounded = 0;

    Tick                _initial;
    CharacterController _controller;

    public Vector3 Velocity {
      get => Data.Velocity;
      set => Data.Velocity = value;
    }

    public bool Grounded {
      get => Data.Grounded;
      set => Data.Grounded = value;
    }

    public void Teleport(Vector3? position = null, Quaternion? rotation = null) {
      _controller.enabled = false;
      NetworkTRSP.Teleport(this, transform, position, rotation);
      _controller.enabled = true;
    }


    public float Jump(bool ignoreGrounded = false, bool doubleJump = false) {
      if (Data.Grounded || ignoreGrounded) {
        var newVel = Data.Velocity;
        newVel.y = doubleJump ? doubleJumpBoost : jumpImpulse;
        Data.Velocity =  newVel;
        return newVel.y;
      }

      return Data.Velocity.y;
    }

    public void Move(Vector2 direction, bool isSlowed, bool jump, float rotation)
    {
        var deltaTime = Runner.DeltaTime;
        var previousPos = transform.position;
        var moveVelocity = Data.Velocity;

        bool isGrounded = _controller.isGrounded;
        if (isGrounded) _updatesSinceLastGrounded = 0;
        else if (_updatesSinceLastGrounded < 3)
        {
            isGrounded = true;
            _updatesSinceLastGrounded++;
        }

        Vector3 _moveDirection = transform.right * direction.x + transform.forward * direction.y;
        _moveDirection *= moveSpeed;
        if (isSlowed) _moveDirection *= slowAmount;
        if (direction.x != 0 && direction.y != 0) _moveDirection /= _squareOfTwo;

        float deltaGravity = gravity * deltaTime;
        moveVelocity.y += deltaGravity;
        if (Math.Abs(moveVelocity.y) < deltaGravity)
        {
            moveVelocity.y += gravity * gravityBurst;
        }

        if (_controller.isGrounded)
        {
            moveVelocity.y = 0f;
            if (jump)
            {
                moveVelocity.y = Jump();
                _jumpTime = Time.time;   
            }
        }

        if (isGrounded) _doubleJumpAvailable = true;
        else if (jump && _doubleJumpAvailable && Time.time - _jumpTime >= doubleJumpDelay)
        {
            _doubleJumpAvailable = false;
            moveVelocity.y = Jump(ignoreGrounded: true, doubleJump: true);
            Debug.Log("Double jumped");
        }

        _moveDirection = AdjustVelocityToSlope(_moveDirection);
        _moveDirection.y += moveVelocity.y;
        _controller.Move(_moveDirection * deltaTime);

        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + rotation, 0);

        moveVelocity = (transform.position - previousPos) * Runner.TickRate;
        //moveVelocity.y = _moveDirection.y;
        Data.Velocity = moveVelocity;
        Data.Grounded = isGrounded;
    }

    private Vector3 AdjustVelocityToSlope(Vector3 velocity)
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, slopeRaycastDistance))
        {
            if (hit.collider.tag == "Ground")
            {
                Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Vector3 adjustedVelocity = slopeRotation * velocity;

                if (adjustedVelocity.y < 0) return adjustedVelocity;
            }
        }

        return velocity;
    }

    //public void Move(Vector3 direction, float slopeRaycastDistance, bool isGroundedFrameNormalized) {
    //  var deltaTime    = Runner.DeltaTime;
    //  var previousPos  = transform.position;
    //  var moveVelocity = Data.Velocity;

    //  if (Data.Grounded && moveVelocity.y < 0) {
    //    moveVelocity.y = 0f;
    //  }

    //  moveVelocity.y += gravity * Runner.DeltaTime;

    //  var horizontalVel = default(Vector3);
    //  horizontalVel.x = moveVelocity.x;
    //  horizontalVel.z = moveVelocity.z;

    //  if (direction == default) {
    //    horizontalVel = Vector3.Lerp(horizontalVel, default, braking * deltaTime);
    //  } else {
    //    horizontalVel      = Vector3.ClampMagnitude(horizontalVel + direction * acceleration * deltaTime, maxSpeed);
    //    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Runner.DeltaTime);
    //  }

    //  moveVelocity.x = horizontalVel.x;
    //  moveVelocity.z = horizontalVel.z;

    //  _controller.Move(AdjustVelocityToSlope(moveVelocity, slopeRaycastDistance) * deltaTime);

    //  Data.Velocity = (transform.position - previousPos) * Runner.TickRate;
    //  Data.Grounded = _controller.isGrounded;
    //}

    public override void Spawned() {
      _initial = default;
      TryGetComponent(out _controller);
      CopyToBuffer();
    }

    public override void Render() {
      NetworkTRSP.Render(this, transform, false, false, false, ref _initial);
    }

    void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount) {
      CopyToEngine();
    }

    void IAfterAllTicks.AfterAllTicks(bool resimulation, int tickCount) {
      CopyToBuffer();
    }

    void IBeforeCopyPreviousState.BeforeCopyPreviousState() {
      CopyToBuffer();
    }
    
    void Awake() {
      TryGetComponent(out _controller);
    }

    void CopyToBuffer() {
      Data.TRSPData.Position = transform.position;
      Data.TRSPData.Rotation = transform.rotation;
    }

    void CopyToEngine() {
      // CC must be disabled before resetting the transform state
      _controller.enabled = false;

      // set position and rotation
      transform.SetPositionAndRotation(Data.TRSPData.Position, Data.TRSPData.Rotation);

      // Re-enable CC
      _controller.enabled = true;
    }
  }
}