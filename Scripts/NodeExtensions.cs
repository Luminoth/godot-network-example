using Godot;

public static class NodeExtensions
{
    public static bool IsNetworkAuthority(this Node node)
    {
        return node.GetMultiplayerAuthority() == node.Multiplayer.GetUniqueId();
    }
}
