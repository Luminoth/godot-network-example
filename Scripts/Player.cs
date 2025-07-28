using Godot;

// sync'd body position (always) and model rotation (always)
public partial class Player : Node
{
    private long _clientId;

    // sync'd server -> client (on change)
    [Export]
    public long ClientId
    {
        get => _clientId;
        set
        {
            _clientId = value;
            Input?.SetMultiplayerAuthority((int)_clientId);
        }
    }

    [Export]
    private PlayerInput _input;

    public PlayerInput Input => _input;

    [Export]
    private PlayerController _controller;

    public PlayerController Controller => _controller;

    [Export]
    private Node3D _model;

    public Node3D Model => _model;
}
