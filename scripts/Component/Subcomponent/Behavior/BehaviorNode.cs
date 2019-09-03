using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotTools.Extension;

namespace GameFeel.Component.Subcomponent.Behavior
{
    public abstract class BehaviorNode : Node
    {
        [Signal]
        public delegate void StatusUpdated(Status status);
        [Signal]
        public delegate void Aborted();

        public enum Status
        {
            SUCCESS,
            FAIL,
            RUNNING
        }

        public bool IsRunning { get; private set; } = false;

        protected List<BehaviorNode> _children;
        protected BehaviorTreeComponent _root;
        private bool _aborting;

        public override void _Ready()
        {
            SetProcess(false);
            _children = this.GetChildren<BehaviorNode>().Where(x => IsInstanceValid(x)).ToList();
            foreach (var child in _children)
            {
                child.Connect(nameof(StatusUpdated), this, nameof(ChildStatusUpdated));
            }
            GetParentOrNull<BehaviorNode>()?.Connect(nameof(Aborted), this, nameof(OnAborted));
            UpdateRoot();
        }

        public override void _Process(float delta)
        {
            Tick();
        }

        public void Enter()
        {
            IsRunning = true;
            InternalEnter();
        }

        protected abstract void InternalEnter();
        protected abstract void Tick();
        protected virtual void InternalLeave()
        {

        }

        protected virtual void PostLeave()
        {

        }

        protected void Leave(Status status)
        {
            InternalLeave();
            IsRunning = false;
            SetProcess(false);
            var aborting = _aborting;
            _aborting = false;
            if (!aborting)
            {
                EmitSignal(nameof(StatusUpdated), status);
            }
            PostLeave();
        }

        protected void Abort()
        {
            _aborting = true;
            Leave(Status.FAIL);
        }

        protected virtual void ChildStatusUpdated(Status status) { }

        private void UpdateRoot()
        {
            if (this is BehaviorTreeComponent bht)
            {
                _root = bht;
                return;
            }

            var root = GetParent();
            while (!(root is BehaviorTreeComponent))
            {
                root = root.GetParent();
            }
            _root = root as BehaviorTreeComponent;
        }

        private void OnAborted()
        {
            EmitSignal(nameof(Aborted));
            Abort();
        }
    }
}