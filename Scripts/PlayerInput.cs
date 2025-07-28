using Godot;

public partial class PlayerInput : MultiplayerSynchronizer
{
    [Export]
    private Player _player;

    // sync'd (always)
    [Export]
    private Vector2 _direction;

    public Vector2 Direction => _direction;

    public override void _Ready()
    {
        // multiplayer authority set by parent (sync'd from server)
        bool isAuthority = GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
        GD.Print($"[Player {_player.ClientId}] Input authority: {isAuthority}");
        SetProcess(isAuthority);
        SetProcessInput(isAuthority);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("jump"))
        {
            _player.Controller.Jump();
        }

        _direction = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
    }
}
