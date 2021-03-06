using Godot;

namespace GameFeel.Component.Subcomponent.Behavior
{
    public class Blackboard
    {
        public AnimatedSprite AnimatedSprite;
        public PathfindComponent PathfindComponent;
        public bool Aggressive = false;
        public Vector2 SpawnPosition = Vector2.Zero;
        public Vector2 AttackTargetPosition = Vector2.Zero;
        public Node2D Owner;
        public int AggroRange;
    }
}