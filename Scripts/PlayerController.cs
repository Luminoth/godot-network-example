using Godot;

public partial class PlayerController : CharacterBody3D
{
    [Export]
    private Player _player;

    [Export]
    private ShapeCast3D _headCollision;

    [Export]
    private AnimationPlayer _animationPlayer;

    [Export]
    private float _walkSpeed = 5.0f;

    [Export]
    private float _crouchSpeedModifier = 0.5f;

    private float _moveSpeed;

    [Export]
    private float _jumpVelocity = 4.5f;

    private bool _jump;

    private bool _isCrouching;

    private bool _shouldCrouch;

    // sync'd (on change)
    [Export]
    private bool IsCrouching
    {
        get => _isCrouching;
        set
        {
            _isCrouching = value;

            _player.Model.SetParameter("parameters/conditions/is_crouching", _isCrouching);
            _player.Model.SetParameter("parameters/conditions/is_not_crouching", !_isCrouching);

            if (_isCrouching)
            {
                _animationPlayer.Play("crouch", -1.0, 5.0f);
                _moveSpeed = _walkSpeed * _crouchSpeedModifier;
            }
            else
            {
                _animationPlayer.Play("crouch", -1.0, -5.0f, true);
                _moveSpeed = _walkSpeed;
            }
        }
    }

    public override void _EnterTree()
    {
        _headCollision.ExcludeParent = true;
    }

    public override void _Ready()
    {
        // arguably we may not want to do this because
        // letting it run on clients might help with prediction later?
        SetPhysicsProcess(Multiplayer.IsServer());

        _player.Model.SetParameter("parameters/crouch/TimeScale/scale", 5.0f);
        _player.Model.SetParameter("parameters/uncrouch/TimeScale/scale", 5.0f);

        _moveSpeed = _walkSpeed;
    }

    public override void _PhysicsProcess(double delta)
    {
        System.Diagnostics.Debug.Assert(Multiplayer.IsServer());

        var velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        if (_jump)
        {
            _jump = false;

            if (IsOnFloor() && !_headCollision.IsColliding())
            {
                velocity.Y = _jumpVelocity;

                if (IsCrouching)
                {
                    IsCrouching = false;
                }
            }
        }

        Vector3 direction = (Transform.Basis * new Vector3(_player.Input.Direction.X, 0, _player.Input.Direction.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * _moveSpeed;
            velocity.Z = direction.Z * _moveSpeed;

            _player.Model.LookAt(GlobalPosition + direction);
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, _moveSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _moveSpeed);
        }

        Velocity = velocity;
        MoveAndSlide();

        // correct for hold crouch uncrouching under something
        if (IsCrouching && !_shouldCrouch && !_headCollision.IsColliding())
        {
            IsCrouching = false;
        }
    }

    public void Jump()
    {
        RpcId(1, MethodName.RpcClientJump);
    }

    public void ToggleCrouch()
    {
        RpcId(1, MethodName.RpcClientToggleCrouch);
    }

    public void Crouch(bool crouch)
    {
        RpcId(1, MethodName.RpcClientCrouch, crouch);
    }

    private void DoCrouch(bool crouch)
    {
        _shouldCrouch = crouch;
        if (crouch && !IsCrouching)
        {
            IsCrouching = true;
        }
        else if (!crouch && IsCrouching && !_headCollision.IsColliding())
        {
            IsCrouching = false;
        }
    }

    // client -> server
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcClientJump()
    {
        System.Diagnostics.Debug.Assert(Multiplayer.IsServer());

        GD.Print($"Player {Multiplayer.GetRemoteSenderId()} jump");

        _jump = true;
    }

    // client -> server
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcClientToggleCrouch()
    {
        System.Diagnostics.Debug.Assert(Multiplayer.IsServer());

        GD.Print($"Player {Multiplayer.GetRemoteSenderId()} toggle crouch");

        DoCrouch(!IsCrouching);
    }

    // client -> server
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcClientCrouch(bool crouch)
    {
        System.Diagnostics.Debug.Assert(Multiplayer.IsServer());

        GD.Print($"Player {Multiplayer.GetRemoteSenderId()} crouch: {crouch}");

        DoCrouch(crouch);
    }
}
