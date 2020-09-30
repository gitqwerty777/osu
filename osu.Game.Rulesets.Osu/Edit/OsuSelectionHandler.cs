// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuSelectionHandler : SelectionHandler
    {
        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            bool canOperate = SelectedHitObjects.Count() > 1 || SelectedHitObjects.Any(s => s is Slider);

            SelectionBox.CanRotate = canOperate;
            SelectionBox.CanScaleX = canOperate;
            SelectionBox.CanScaleY = canOperate;
        }

        protected override void OnDragOperationEnded()
        {
            base.OnDragOperationEnded();
            referenceOrigin = null;
        }

        public override bool HandleMovement(MoveSelectionEvent moveEvent) =>
            moveSelection(moveEvent.InstantDelta);

        /// <summary>
        /// During a transform, the initial origin is stored so it can be used throughout the operation.
        /// </summary>
        private Vector2? referenceOrigin;

        public override bool HandleScaleY(in float scale, Anchor reference)
        {
            int direction = (reference & Anchor.y0) > 0 ? -1 : 1;

            if (direction < 0)
            {
                // when resizing from a top drag handle, we want to move the selection first
                if (!moveSelection(new Vector2(0, scale)))
                    return false;
            }

            return scaleSelection(new Vector2(0, direction * scale));
        }

        public override bool HandleScaleX(in float scale, Anchor reference)
        {
            int direction = (reference & Anchor.x0) > 0 ? -1 : 1;

            if (direction < 0)
            {
                // when resizing from a left drag handle, we want to move the selection first
                if (!moveSelection(new Vector2(scale, 0)))
                    return false;
            }

            return scaleSelection(new Vector2(direction * scale, 0));
        }

        public override bool HandleRotation(float delta)
        {
            Quad quad = getSelectionQuad();

            referenceOrigin ??= quad.Centre;

            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                h.Position = rotatePointAroundOrigin(h.Position, referenceOrigin.Value, delta);

                if (h is IHasPath path)
                {
                    foreach (var point in path.Path.ControlPoints)
                    {
                        point.Position.Value = rotatePointAroundOrigin(point.Position.Value, Vector2.Zero, delta);
                    }
                }
            }

            // todo: not always
            return true;
        }

        private bool scaleSelection(Vector2 scale)
        {
            Quad quad = getSelectionQuad();

            Vector2 minPosition = quad.TopLeft;

            Vector2 size = quad.Size;
            Vector2 newSize = size + scale;

            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                if (scale.X != 1)
                    h.Position = new Vector2(minPosition.X + (h.X - minPosition.X) / size.X * newSize.X, h.Y);
                if (scale.Y != 1)
                    h.Position = new Vector2(h.X, minPosition.Y + (h.Y - minPosition.Y) / size.Y * newSize.Y);
            }

            return true;
        }

        private bool moveSelection(Vector2 delta)
        {
            Vector2 minPosition = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPosition = new Vector2(float.MinValue, float.MinValue);

            // Go through all hitobjects to make sure they would remain in the bounds of the editor after movement, before any movement is attempted
            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                // Stacking is not considered
                minPosition = Vector2.ComponentMin(minPosition, Vector2.ComponentMin(h.EndPosition + delta, h.Position + delta));
                maxPosition = Vector2.ComponentMax(maxPosition, Vector2.ComponentMax(h.EndPosition + delta, h.Position + delta));
            }

            if (minPosition.X < 0 || minPosition.Y < 0 || maxPosition.X > DrawWidth || maxPosition.Y > DrawHeight)
                return false;

            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                h.Position += delta;
            }

            return true;
        }

        /// <summary>
        /// Returns a gamefield-space quad surrounding the current selection.
        /// </summary>
        private Quad getSelectionQuad()
        {
            if (!SelectedHitObjects.Any())
                return new Quad();

            Vector2 minPosition = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPosition = new Vector2(float.MinValue, float.MinValue);

            // Go through all hitobjects to make sure they would remain in the bounds of the editor after movement, before any movement is attempted
            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                // Stacking is not considered
                minPosition = Vector2.ComponentMin(minPosition, Vector2.ComponentMin(h.EndPosition, h.Position));
                maxPosition = Vector2.ComponentMax(maxPosition, Vector2.ComponentMax(h.EndPosition, h.Position));
            }

            Vector2 size = maxPosition - minPosition;

            return new Quad(minPosition.X, minPosition.Y, size.X, size.Y);
        }

        /// <summary>
        /// Rotate a point around an arbitrary origin.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="origin">The centre origin to rotate around.</param>
        /// <param name="angle">The angle to rotate (in degrees).</param>
        private static Vector2 rotatePointAroundOrigin(Vector2 point, Vector2 origin, float angle)
        {
            angle = -angle;

            point.X -= origin.X;
            point.Y -= origin.Y;

            Vector2 ret;
            ret.X = (float)(point.X * Math.Cos(MathUtils.DegreesToRadians(angle)) + point.Y * Math.Sin(angle / 180f * Math.PI));
            ret.Y = (float)(point.X * -Math.Sin(MathUtils.DegreesToRadians(angle)) + point.Y * Math.Cos(angle / 180f * Math.PI));

            ret.X += origin.X;
            ret.Y += origin.Y;

            return ret;
        }
    }
}
