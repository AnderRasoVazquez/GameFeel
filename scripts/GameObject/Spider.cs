using GameFeel.Interface;
using Godot;
using GodotTools.Extension;
using GodotTools.Logic;

namespace GameFeel.GameObject
{
    public class Spider : KinematicBody2D, IDamageReceiver
    {
        private const float MAX_AHEAD = 20f;

        [Signal]
        public delegate void DamageReceived(float damage);

        private Area2D _hitboxArea;
        private Tween _shaderTween;
        private AnimatedSprite _animatedSprite;
        private ShaderMaterial _shaderMaterial;
        private Timer _pathfindTimer;

        private float _maxHp = 10f;
        private float _hp = 10f;

        private float _currentT;
        private float _speed = 75f;
        private Curve2D _curve = new Curve2D();
        private Vector2 _velocity;

        private StateMachine<State> _stateMachine = new StateMachine<State>();

        private enum State
        {
            PURSUE
        }

        public override void _Ready()
        {
            _stateMachine.AddState(State.PURSUE, StatePursue);
            _stateMachine.SetInitialState(State.PURSUE);

            _hitboxArea = GetNode<Area2D>("HitboxArea2D");
            _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
            _shaderTween = GetNode<Tween>("ShaderTween");
            _pathfindTimer = GetNode<Timer>("PathfindTimer");

            _shaderMaterial = _animatedSprite.Material as ShaderMaterial;
            _hitboxArea.Connect("body_entered", this, nameof(OnBodyEntered));
            _pathfindTimer.Connect("timeout", this, nameof(OnPathfindTimerTimeout));
        }

        public override void _Process(float delta)
        {
            _stateMachine.Update();
        }

        public void DealDamage(float damage)
        {
            PlayHitShadeTween();
            Camera.Shake();
            GameWorld.CreateDamageNumber(this, damage);
            _hp -= damage;
            EmitSignal(nameof(DamageReceived), damage);
        }

        public float GetCurrentHealthPercent()
        {
            return _hp / (_maxHp > 0f ? _maxHp : 1f);
        }

        private void StatePursue(bool isStateNew)
        {
            if (isStateNew)
            {
                UpdatePath();
            }
            var destinationPoint = _curve.InterpolateBaked(_currentT);
            if (GlobalPosition.DistanceSquaredTo(destinationPoint) < MAX_AHEAD * MAX_AHEAD)
            {
                _currentT += _speed * GetProcessDeltaTime();
            }

            if (_currentT < (_curve.GetBakedLength()))
            {
                _velocity = (destinationPoint - GlobalPosition).Normalized() * _speed;
            }
            else
            {
                _velocity = Vector2.Zero;
            }

            _velocity = MoveAndSlide(_velocity);
        }

        private void PlayHitShadeTween()
        {
            _shaderTween.ResetAll();
            _shaderTween.InterpolateProperty(
                _shaderMaterial,
                "shader_param/_hitShadePercent",
                1.0f,
                0f,
                .3f,
                Tween.TransitionType.Quad,
                Tween.EaseType.In
            );
            _shaderTween.Start();
        }

        private void UpdatePath()
        {
            var player = GetTree().GetFirstNodeInGroup<Player>(Player.GROUP);
            if (player != null)
            {
                _curve = GameWorld.GetPathCurve(GlobalPosition, player.GlobalPosition);
                _currentT = 0f;
            }
        }

        private void OnBodyEntered(PhysicsBody2D body)
        {
            if (body is IDamageDealer dd)
            {
                dd.RegisterHit(this);
            }
        }

        private void OnPathfindTimerTimeout()
        {
            if (_stateMachine.GetCurrentState() == State.PURSUE)
            {
                UpdatePath();
            }
        }
    }
}