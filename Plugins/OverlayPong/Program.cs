using ETS2LA.Overlay;
using ETS2LA.Shared;
using ETS2LA.Controls;

using Hexa.NET.ImGui;
using System.Numerics;

namespace OverlayPong {
    public class OverlayPong : Plugin {
        public override PluginInformation Info => new PluginInformation {
            Id = "OverlayPong",
            Name = "Overlay Pong",
            Description = "A plugin to add a game of pong to the overlay",
            Version = "1.0.0",
            AuthorName = "DylanBPY",
        };
        public override float TickRate => 30f;

        private const int windowWidth = 600;
        private const int windowHeight = 500;
        private WindowDefinition _windowDefinition = new WindowDefinition {
            Title = "Overlay Pong",
            Width = windowWidth,
            Height = windowHeight,
        };

        public ControlDefinition PaddleUpControl = new ControlDefinition {
            Id = "OverlayPong.PaddleUp",
            Name = "Paddle Up",
            Description = "Move your paddle up in the pong game",
            DefaultKeybind = "W",
            Type = ControlType.Boolean
        };

        public ControlDefinition PaddleDownControl = new ControlDefinition {
            Id = "OverlayPong.PaddleDown",
            Name = "Paddle Down",
            Description = "Move your paddle down in the pong game",
            DefaultKeybind = "S",
            Type = ControlType.Boolean
        };

        enum PaddleMovement {
            Up,
            Down,
            None
        }

        private Random random = new Random();
        private const int paddleHeight = 80;
        private const int paddleWidth = 20;
        private const int titlebarHeight = 24;
        private const int margin = 10;
        private const int ballRadius = 10;
        private const int userPaddleSpeed = 5;
        private const int botPaddleSpeed = 4;

        private int userPaddlePosition; // Y coordinate of the paddle's bottom edge
        private int botPaddlePosition;
        private Vector2 ballPosition; // Center of the ball
        private Vector2 ballVelocity;
        private PaddleMovement userPaddleMovement;
        private int userScore;
        private int botScore;

        private void reset(bool resetScore = false) {
            userPaddlePosition = windowHeight / 2 + paddleHeight / 2 + titlebarHeight;
            botPaddlePosition = windowHeight / 2 + paddleHeight / 2 + titlebarHeight;
            ballPosition = new Vector2(windowWidth / 2, windowHeight / 2 + titlebarHeight);
            ballVelocity.X = (random.Next(0, 2) == 0 ? 1 : -1) * 5; // -5 OR 5
            ballVelocity.Y = random.Next(-4, 5); // -4 TO 4
            userPaddleMovement = PaddleMovement.None;

            if (resetScore) { userScore = 0; botScore = 0; }
        }

        private void OnPaddleUpControlChanged(object? sender, ControlChangeEventArgs e) {
            userPaddleMovement = (bool)e.NewValue! ? PaddleMovement.Up : PaddleMovement.None;
        }

        private void OnPaddleDownControlChanged(object? sender, ControlChangeEventArgs e) {
            userPaddleMovement = (bool)e.NewValue! ? PaddleMovement.Down : PaddleMovement.None;
        }

        public override void Init() {
            base.Init();

            ControlsBackend.Current.RegisterControl(PaddleUpControl);
            ControlsBackend.Current.RegisterControl(PaddleDownControl);
            ControlsBackend.Current.On(PaddleUpControl.Id, OnPaddleUpControlChanged);
            ControlsBackend.Current.On(PaddleDownControl.Id, OnPaddleDownControlChanged);
        }

        public override void OnEnable() {
            base.OnEnable();
            OverlayHandler.Current.RegisterWindow(_windowDefinition, RenderWindow);
            reset(true);
        }

        public override void Tick()
        {
            base.Tick();

            ballPosition += ballVelocity;

            // Move user paddle based on input
            switch (userPaddleMovement) {
                case PaddleMovement.Up:
                    userPaddlePosition = Math.Max(paddleHeight + titlebarHeight + margin, userPaddlePosition - userPaddleSpeed);
                    break;
                case PaddleMovement.Down:
                    userPaddlePosition = Math.Min(windowHeight - margin, userPaddlePosition + userPaddleSpeed);
                    break;
            }
            
            // Move bot paddle based on ball position
            PaddleMovement botPaddleMovement = (ballPosition.Y < botPaddlePosition - paddleHeight / 2) ? PaddleMovement.Up :
                                              (ballPosition.Y > botPaddlePosition - paddleHeight / 2) ? PaddleMovement.Down :
                                              PaddleMovement.None;
            switch (botPaddleMovement) {
                case PaddleMovement.Up:
                    botPaddlePosition = Math.Max(paddleHeight + titlebarHeight + margin, botPaddlePosition - botPaddleSpeed);
                    break;
                case PaddleMovement.Down:
                    botPaddlePosition = Math.Min(windowHeight - margin, botPaddlePosition + botPaddleSpeed);
                    break;
            }

            // Collision with top and bottom walls
            if (ballPosition.Y - ballRadius <= titlebarHeight || ballPosition.Y + ballRadius >= windowHeight) {
                ballVelocity.Y = -ballVelocity.Y;
            }

            // Collision with paddles
            if (ballPosition.X - ballRadius <= margin + paddleWidth && 
                    ballPosition.Y >= userPaddlePosition - paddleHeight && ballPosition.Y <= userPaddlePosition) {
                ballVelocity.X *= -1.1f; 
                ballVelocity.Y += random.Next(-3, 4);
                ballPosition.X = margin + paddleWidth + ballRadius;
            } else if (ballPosition.X + ballRadius >= windowWidth - margin - paddleWidth && 
                    ballPosition.Y >= botPaddlePosition - paddleHeight && ballPosition.Y <= botPaddlePosition) {
                ballVelocity.X *= -1.1f;
                ballVelocity.Y += random.Next(-3, 4);
                ballPosition.X = windowWidth - margin - paddleWidth - ballRadius;
            }

            // Check for scoring
            if (ballPosition.X < 0) { botScore++; reset(); }
            else if (ballPosition.X > windowWidth) { userScore++; reset(); }
        }

        private void RenderWindow() {
            var drawList = ImGui.GetWindowDrawList();
            var windowPos = ImGui.GetWindowPos();

            drawList.AddRectFilled( // User paddle (left side)
                new Vector2(windowPos.X + margin, windowPos.Y + userPaddlePosition - paddleHeight), 
                new Vector2(windowPos.X + margin + paddleWidth, windowPos.Y + userPaddlePosition), 
                ImGui.GetColorU32(new Vector4(1, 1, 1, 1))
            );

            drawList.AddRectFilled( // Bot paddle (right side)
                new Vector2(windowPos.X + windowWidth - margin - paddleWidth, windowPos.Y + botPaddlePosition - paddleHeight), 
                new Vector2(windowPos.X + windowWidth - margin, windowPos.Y + botPaddlePosition), 
                ImGui.GetColorU32(new Vector4(1, 1, 1, 1))
            );

            drawList.AddCircleFilled( // Ball
                new Vector2(windowPos.X + ballPosition.X, windowPos.Y + ballPosition.Y), 
                ballRadius, 
                ImGui.GetColorU32(new Vector4(1, 1, 1, 1))
            );

            drawList.AddLine( // Center line
                new Vector2(windowPos.X + windowWidth / 2, windowPos.Y), 
                new Vector2(windowPos.X + windowWidth / 2, windowPos.Y + windowHeight), 
                ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), 
                2
            );

            drawList.AddText( // User score
                new Vector2(windowPos.X + windowWidth / 4, windowPos.Y + margin + 25), 
                ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), 
                userScore.ToString()
            );

            drawList.AddText( // Bot score
                new Vector2(windowPos.X + 3 * windowWidth / 4, windowPos.Y + margin + 25), 
                ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), 
                botScore.ToString()
            );
        }
        
        public override void OnDisable() {
            base.OnDisable();
            OverlayHandler.Current.UnregisterWindow(_windowDefinition);
        }

        public override void Shutdown() {
            base.Shutdown();
        }
    }
}