using Godot;

public partial class Model : Node3D
{
    [Export]
    private AnimationTree _animationTree;

    [Export]
    private AnimationPlayer _animationPlayer;
}
