// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.StateChanges;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDocker : ModDocker, IApplicableFailOverride, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Description => @"Build to the beat";

        private OsuInputManager inputManager;

        private IFrameStableClock gameplayClock;

        private List<OsuReplayFrame> replayFrames;

        private int currentFrame;

        public void Update(Playfield playfield)
        {
            if (currentFrame == replayFrames.Count - 1) return;

            double time = gameplayClock.CurrentTime;

            // Very naive implementation of autopilot based on proximity to replay frames.
            // TODO: this needs to be based on user interactions to better match stable (pausing until judgement is registered).
            if (Math.Abs(replayFrames[currentFrame + 1].Time - time) <= Math.Abs(replayFrames[currentFrame].Time - time) && !ModDocker.ShouldEnd)
            {
                currentFrame++;
                // We'll fake mouse input.
                // I lack the coordination to play osu!, sorry.
                MousePositionAbsoluteInput move = new MousePositionAbsoluteInput { Position = playfield.ToScreenSpace(replayFrames[currentFrame].Position) };
                MouseButtonInput click = new MouseButtonInput(osuTK.Input.MouseButton.Left, true);

                move.Apply(inputManager.CurrentState, inputManager);
                click.Apply(inputManager.CurrentState, inputManager);
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            gameplayClock = drawableRuleset.FrameStableClock;

            // Grab the input manager to disable the user's cursor, and for future use
            inputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
            inputManager.AllowUserCursorMovement = false;

            // Generate the replay frames the cursor should follow
            replayFrames = new OsuAutoGenerator(drawableRuleset.Beatmap, drawableRuleset.Mods).Generate().Frames.Cast<OsuReplayFrame>().ToList();
        }
    }
}
