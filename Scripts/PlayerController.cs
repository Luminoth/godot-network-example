using Godot;

public partial class PlayerController : CharacterBody3D
{
    [Export]
    private Player _player;

    [Export]
    private AnimationPlayer _animationPlayer;

    [Export]
    private CollisionShape3D _collisionShape;

    private CapsuleShape3D CollisionShape => (CapsuleShape3D)_collisionShape.Shape;

    // sync'd (on change)
    [Export]
    private float CollisionHeight
    {
        get => CollisionShape.Height;
        set => CollisionShape.Height = value;
    }

    [Export]
    private float _speed = 5.0f;

    [Export]
    private float _jumpVelocity = 4.5f;

    private bool _jump;

    // sync'd (on change)
    [Export]
    private bool _isCrouching;

    public override void _Ready()
    {
        // arguably we may not want to do this because
        // letting it run on clients might help with prediction later?
        SetPhysicsProcess(Multiplayer.IsServer());
    }

    public override void _PhysicsProcess(double delta)
    {
        System.Diagnostics.Debug.Assert(Multiplayer.IsServer());

        var velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        if (_jump && IsOnFloor())
        {
            velocity.Y = _jumpVelocity;
            _jump = false;
        }

        Vector3 direction = (Transform.Basis * new Vector3(_player.Input.Direction.X, 0, _player.Input.Direction.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * _speed;
            velocity.Z = direction.Z * _speed;

            _player.Model.LookAt(GlobalPosition + direction);
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, _speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public void Jump()
    {
        RpcId(1, MethodName.RpcClientJump);
    }

    public void ToggleCrouch()
    {
        RpcId(1, MethodName.RpcClientToggleCrouch);
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

        GD.Print($"Player {Multiplayer.GetRemoteSenderId()} toggle crouch ({_player.Name})");

        Rpc(MethodName.RpcServerBroadcastToggleCrouch, !_isCrouching);
    }

    // server broadcast
    [Rpc(CallLocal = true)]
    private void RpcServerBroadcastToggleCrouch(bool crouch)
    {
        GD.Print($"Server toggle crouch ({_player.Name})");

        if (crouch)
        {
            _animationPlayer.Play("crouch", -1.0, 5.0f);
            _player.Model.ChangeState("crouch");
        }
        else
        {
            _animationPlayer.Play("crouch", -1.0, -5.0f, true);
            _player.Model.ChangeState("uncrouch");
        }

        if (this.IsNetworkAuthority())
        {
            _isCrouching = crouch;
        }
    }
}
