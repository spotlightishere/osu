// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Timers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDocker : Mod, IApplicableToScoreProcessor, IApplicableFailOverride, IApplicableToDifficulty
    {
        // There's probably a better way to do this... I don't know C# :)
        // Cannot do get/init due to this being C# 8, that's since 9
        private static Timer endTimer = null;
        public static bool ShouldEnd = false;
        private static void theTimeIsNeigh(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The time has come for you to... fail for your life.");
            ShouldEnd = true;
        }

        public override string Name => "Docker";
        public override string Acronym => "NX";
        public override IconUsage? Icon => FontAwesome.Solid.Cloud;
        public override ModType Type => ModType.Automation;
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModDifficultyAdjust) };

        // Occasionally, we trip up and aren't fast enough to press mouse inputs.
        // This is so much of an issue that I just gave up. This difficulty is 0.
        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.OverallDifficulty = 1;
        }

        // We have no need for score processing.
        // This is called upon game start, allowing our build to run.
        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            endTimer = new Timer
            {
                // We'll give ourselves 35 seconds for Docker to have a headstart.
                Interval = 35000,
                AutoReset = false,
                Enabled = true,
            };
            endTimer.Elapsed += theTimeIsNeigh;

            // Run initial docker-compose ending command
            System.Diagnostics.Process.Start("tmux", "send-keys -t 0 \"docker-compose up --build\" C-m");

        }

        // Upon fail, ruin Docker.
        public bool PerformFail() {
            // Run Docker termination
            System.Diagnostics.Process.Start("killall", "-15 docker-compose");
            System.Diagnostics.Process.Start("tmux", "send-keys -t 0 \"docker system prune -af\" C-m");
            return true;
        }

        // We're not actually adjusting the rank.
        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public bool RestartOnFail => false;

        public override bool UserPlayable => false;
    }
}
