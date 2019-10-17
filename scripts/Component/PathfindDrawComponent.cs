using System.Collections.Generic;
using Godot;
using GodotApiTools.Extension;

namespace GameFeel.Component
{
    public class PathfindDrawComponent : Node2D
    {
        public override void _Process(float delta)
        {
            Update();
        }

        public override void _Draw()
        {
            if (Engine.EditorHint)
            {
                return;
            }

            var points = Owner.GetFirstNodeOfType<PathfindComponent>().Curve.GetBakedPoints();
            List<Vector2> newPoints = new List<Vector2>();

            foreach (var point in points)
            {
                newPoints.Add(GetGlobalTransform().XformInv(point));
            }

            if (newPoints.Count > 1)
            {
                DrawPolyline(newPoints.ToArray(), new Color(1f, 1f, 1f), 1);
            }

            var targetPoint = Owner.GetFirstNodeOfType<PathfindComponent>().GetTargetPoint();
            DrawCircle(GetGlobalTransform().XformInv(targetPoint), 3f, new Color(0, 0, 1, 1));
        }
    }
}