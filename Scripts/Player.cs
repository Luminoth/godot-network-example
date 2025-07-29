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
    private Model _model;

    public Model Model => _model;

    public override void _Ready()
    {
        if (ClientId == Multiplayer.GetUniqueId())
        {
            var camera = new Camera3D
            {
                Position = new Vector3(0.0f, 5.0f, 10.0f),
                RotationDegrees = new Vector3(-30.0f, 0.0f, 0.0f)
            };
            Controller.AddChild(camera);
        }
    }
}
