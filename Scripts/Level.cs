using Godot;

using System.Globalization;

public partial class Level : Node3D
{
    [Export]
    private int _defaultPort = 12345;

    [Export]
    private CanvasLayer _networkUI;

    [Export]
    private LineEdit _addressInput;

    [Export]
    private MultiplayerSpawner _playerSpawner;

    [Export]
    private Node3D _playerSpawnRoot;

    [Export]
    private PackedScene _playerScene;

    // changing details on the root multiplayer object
    // will cascade down to every node in the tree
    private MultiplayerApi RootMultiplayer => GetTree().Root.Multiplayer;

    private string DefaultAddress => $"127.0.0.1:{_defaultPort}";

    public override void _EnterTree()
    {
        _playerSpawner.AddSpawnableScene(_playerScene.ResourcePath);

        _addressInput.PlaceholderText = DefaultAddress;

        // clear the default "offline" multiplayer peer
        RootMultiplayer.MultiplayerPeer = null;
    }

    private void SpawnPlayer(long id)
    {
        System.Diagnostics.Debug.Assert(Multiplayer.IsServer());

        GD.Print($"Spawning player {id} ...");

        var player = _playerScene.Instantiate<Player>();
        player.ClientId = id;
        _playerSpawnRoot.AddChild(player, true);
    }

    private void OnPlayerConnected(long id)
    {
        System.Diagnostics.Debug.Assert(Multiplayer.IsServer());

        GD.Print($"Player {id} connected ...");

        SpawnPlayer(id);
    }

    private void _on_host_pressed()
    {
        GD.Print($"Hosting on {_defaultPort} ...");

        var peer = new ENetMultiplayerPeer();
        peer.CreateServer(_defaultPort, 8);
        RootMultiplayer.MultiplayerPeer = peer;

        Multiplayer.PeerConnected += OnPlayerConnected;

        SpawnPlayer(1);

        _networkUI.Hide();
    }

    private void _on_join_pressed()
    {
        var address = _addressInput.Text;
        if (string.IsNullOrWhiteSpace(address))
        {
            address = DefaultAddress;
        }

        GD.Print($"Joining {address} ...");

        var parts = address.Split(':');

        var peer = new ENetMultiplayerPeer();
        peer.CreateClient(parts[0], short.Parse(parts[1], CultureInfo.InvariantCulture));
        RootMultiplayer.MultiplayerPeer = peer;

        _networkUI.Hide();
    }
}
