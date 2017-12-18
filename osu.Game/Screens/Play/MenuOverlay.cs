﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    public abstract class MenuOverlay : OverlayContainer, IRequireHighFrequencyMousePosition
    {
        private const int transition_duration = 200;
        private const int button_height = 70;
        private const float background_alpha = 0.75f;

        protected override bool BlockPassThroughKeyboard => true;

        public Action OnRetry;
        public Action OnQuit;

        public abstract string Header { get; }
        public abstract string Description { get; }

        protected FillFlowContainer<DialogButton> Buttons;

        private FillFlowContainer retryCounterContainer;

        protected MenuOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            StateChanged += s => selectionIndex = -1;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = background_alpha,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 50),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 20),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = Header,
                                    Font = @"Exo2.0-Medium",
                                    Spacing = new Vector2(5, 0),
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    TextSize = 30,
                                    Colour = colours.Yellow,
                                    Shadow = true,
                                    ShadowColour = new Color4(0, 0, 0, 0.25f)
                                },
                                new OsuSpriteText
                                {
                                    Text = Description,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Shadow = true,
                                    ShadowColour = new Color4(0, 0, 0, 0.25f)
                                }
                            }
                        },
                        Buttons = new FillFlowContainer<DialogButton>
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Masking = true,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.6f),
                                Radius = 50
                            },
                        },
                        retryCounterContainer = new FillFlowContainer
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both,
                        }
                    }
                },
            };

            Retries = 0;
        }

        public int Retries
        {
            set
            {
                if (retryCounterContainer != null)
                {
                    // "You've retried 1,065 times in this session"
                    // "You've retried 1 time in this session"

                    retryCounterContainer.Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "You've retried ",
                            Shadow = true,
                            ShadowColour = new Color4(0, 0, 0, 0.25f),
                            TextSize = 18
                        },
                        new OsuSpriteText
                        {
                            Text = $"{value:n0}",
                            Font = @"Exo2.0-Bold",
                            Shadow = true,
                            ShadowColour = new Color4(0, 0, 0, 0.25f),
                            TextSize = 18
                        },
                        new OsuSpriteText
                        {
                            Text = $" time{(value == 1 ? "" : "s")} in this session",
                            Shadow = true,
                            ShadowColour = new Color4(0, 0, 0, 0.25f),
                            TextSize = 18
                        }
                    };
                }
            }
        }

        public override bool HandleInput => State == Visibility.Visible;

        protected override void PopIn() => this.FadeIn(transition_duration, Easing.In);
        protected override void PopOut() => this.FadeOut(transition_duration, Easing.In);

        // Don't let mouse down events through the overlay or people can click circles while paused.
        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args) => true;

        protected override bool OnMouseMove(InputState state) => true;

        protected void AddButton(string text, Color4 colour, Action action)
        {
            var button = new MenuOverlayButton
            {
                Text = text,
                ButtonColour = colour,
                Origin = Anchor.TopCentre,
                Anchor = Anchor.TopCentre,
                Height = button_height,
                Action = delegate
                {
                    action?.Invoke();
                    Hide();
                }
            };

            button.Selected.ValueChanged += s => buttonSelectionChanged(button, s);

            Buttons.Add(button);
        }

        private int _selectionIndex = -1;
        private int selectionIndex
        {
            get { return _selectionIndex; }
            set
            {
                if (_selectionIndex == value)
                    return;

                if (_selectionIndex != -1)
                    Buttons[_selectionIndex].Selected.Value = false;

                _selectionIndex = value;

                if (_selectionIndex != -1)
                    Buttons[_selectionIndex].Selected.Value = true;
            }
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat)
                return false;

            switch (args.Key)
            {
                case Key.Up:
                    if (selectionIndex == -1 || selectionIndex == 0)
                        selectionIndex = Buttons.Count - 1;
                    else
                        selectionIndex--;
                    return true;
                case Key.Down:
                    if (selectionIndex == -1 || selectionIndex == Buttons.Count - 1)
                        selectionIndex = 0;
                    else
                        selectionIndex++;
                    return true;
            }

            return false;
        }

        private void buttonSelectionChanged(DialogButton button, bool isSelected)
        {
            if (!isSelected)
                selectionIndex = -1;
            else
                selectionIndex = Buttons.IndexOf(button);
        }

        private class MenuOverlayButton : DialogButton
        {
            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                if (args.Repeat || args.Key != Key.Enter || !Selected)
                    return false;

                OnClick(state);
                return true;
            }
        }
    }
}
