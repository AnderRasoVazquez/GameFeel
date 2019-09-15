using GameFeel.Component;
using Godot;
using GodotTools.Extension;

namespace GameFeel.GameObject.Loot
{
    public class Equipment : Node2D
    {
        [Export(PropertyHint.Enum, "1,2,3,4")]
        public int SlotIndex { get; private set; }

        private ProjectileSpawnComponent _projectileSpawnComponent;

        public override void _Ready()
        {
            var player = GetTree().GetFirstNodeInGroup<Player>(Player.GROUP);
            if (player != null)
            {
                player.Connect(nameof(Player.Attack), this, nameof(OnPlayerAttack));
            }
            _projectileSpawnComponent = GetNode<ProjectileSpawnComponent>("ProjectileSpawnComponent");
        }

        private void OnPlayerAttack(Player p)
        {
            if (p.Mana >= 1f)
            {
                var direction = Vector2.Right.Rotated(GlobalRotation * Mathf.Sign(GlobalScale.y));
                _projectileSpawnComponent.Spawn(direction);
                p.RemoveMana(1f);
            }
        }
    }
}