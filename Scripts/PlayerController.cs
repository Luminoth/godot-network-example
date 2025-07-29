using Godot;

public partial class PlayerController : CharacterBody3D
{
    [Export]
    private Player _player;

    [Export]
    private AnimationPlayer _animationPlayer;

    [Export]
    private float _speed = 5.0f;

    [Export]
    private float _jumpVelocity = 4.5f;

    private bool _jump;

    public override void _Ready()
    {
        // arguably we may not want to do this because
        // letting it run on clients might help with prediction later?
        SetPhysicsProcess(Multiplayer.IsServer());
    }

    public override void _PhysicsProcess(double delta)
    {
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
        RpcId(1, MethodName.RpcJump);
    }

    // client -> server
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcJump()
    {
        _jump = true;
    }
}
