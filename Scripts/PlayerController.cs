using Godot;

public partial class PlayerController : CharacterBody3D
{
    [Export]
    private Player _player;

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
    private ShapeCast3D _headCollision;

    [Export]
    private AnimationPlayer _animationPlayer;

    [Export]
    private float _speed = 5.0f;

    [Export]
    private float _jumpVelocity = 4.5f;

    private bool _jump;

    private bool _isCrouching;

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
            }
            else
            {
                _animationPlayer.Play("crouch", -1.0, -5.0f, true);
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
    }

    public override void _PhysicsProcess(double delta)
    {
        System.Diagnostics.Debug.Assert(Multiplayer.IsServer());

        var velocity = Velocity;

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

        GD.Print($"Player {Multiplayer.GetRemoteSenderId()} toggle crouch");

        if (IsCrouching && !_headCollision.IsColliding())
        {
            IsCrouching = false;
        }
        else if (!IsCrouching)
        {
            IsCrouching = true;
        }
    }
}
