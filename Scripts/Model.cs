using Godot;

public partial class Model : Node3D
{
    [Export]
    private AnimationPlayer _animationPlayer;

    [Export]
    private AnimationTree _animationTree;

    private AnimationNodeStateMachinePlayback _animationStateMachine;

    public override void _EnterTree()
    {
        if (_animationTree != null)
        {
            _animationStateMachine = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/playback");
        }
    }

    public void PlayAnimation(StringName name, double customBlend = -1, float customSpeed = 1, bool fromEnd = false)
    {
        _animationPlayer.Play(name, customBlend, customSpeed, fromEnd);
    }

    public void SetParameter(StringName property, Variant value)
    {
        _animationTree.Set(property, value);
    }

    public void ChangeState(StringName toNode)
    {
        _animationStateMachine.Travel(toNode);
    }
}
