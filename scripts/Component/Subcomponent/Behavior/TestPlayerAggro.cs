using GameFeel.GameObject;
using Godot;
using GodotTools.Extension;

namespace GameFeel.Component.Subcomponent.Behavior
{
    public class TestPlayerAggro : BehaviorNode
    {
        [Export]
        private int _aggroDistance = 50;

        protected override void InternalEnter()
        {
            if (_root.Blackboard.Aggressive || IsPlayerInAggroRange())
            {
                _root.Blackboard.Aggressive = true;
                Leave(Status.SUCCESS);
            }
            else
            {
                Leave(Status.FAIL);
            }
        }

        protected override void Tick()
        {

        }

        private bool IsPlayerInAggroRange()
        {
            var player = GetTree().GetFirstNodeInGroup<Player>(Player.GROUP);
            var owner = GetOwnerOrNull<Node2D>();
            return IsInstanceValid(player) && IsInstanceValid(owner) && owner.GlobalPosition.DistanceSquaredTo(player.GlobalPosition) < _aggroDistance * _aggroDistance;
        }
    }
}