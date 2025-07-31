using Godot;

// sync'd body position (always) and model rotation (always)
// sync'd _playerNameLabel.text (on change)
// TODO: this could inherit from MultiplayerSynchronizer to clean things up a little
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

    public bool IsLocalPlayer => ClientId == Multiplayer.GetUniqueId();

    [Export]
    private PlayerInput _input;

    public PlayerInput Input => _input;

    [Export]
    private PlayerController _controller;

    public PlayerController Controller => _controller;

    [Export]
    private Model _model;

    public Model Model => _model;

    [Export]
    private Label3D _playerNameLabel;

    public override void _Ready()
    {
        if (IsLocalPlayer)
        {
            var camera = new Camera3D
            {
                Position = new Vector3(0.0f, 5.0f, 10.0f),
                RotationDegrees = new Vector3(-30.0f, 0.0f, 0.0f)
            };
            Controller.AddChild(camera);
        }

        _playerNameLabel.Text = Name;
    }
}
