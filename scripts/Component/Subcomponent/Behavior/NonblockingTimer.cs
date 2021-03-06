using Godot;

namespace GameFeel.Component.Subcomponent.Behavior
{
    public class NonblockingTimer : BehaviorNode
    {
        [Export]
        private float _minTime = 1f;
        [Export]
        private float _maxTime = 2f;

        private Timer _timer;
        private bool _reportSuccess = false;

        public override void _Ready()
        {
            base._Ready();
            _timer = GetNode<Timer>("Timer");
            _timer.Connect("timeout", this, nameof(OnTimerTimeout));
        }

        protected override void InternalEnter()
        {
            if (_timer.IsStopped())
            {
                _timer.WaitTime = Main.RNG.RandfRange(_minTime, _maxTime);
                _timer.Start();
                if (_reportSuccess)
                {
                    _reportSuccess = false;
                    Leave(Status.SUCCESS);
                }
                else
                {
                    Leave(Status.FAIL);
                }
            }
            else
            {
                Leave(Status.FAIL);
            }
        }

        protected override void Tick()
        {

        }

        protected override void InternalLeave()
        {

        }

        private void OnTimerTimeout()
        {
            _reportSuccess = true;
        }
    }
}