using System;
using ETS2LA.Shared;
using ETS2LA.Backend.Events;
using ETS2LA.Settings;
using ETS2LA.Logging;
using ETS2LA.Tutorials;

namespace ExampleProvider
{

    [Serializable]
    class MySettings
    {
        public int ExampleValue = 42;
    }

    public class MyProvider : Plugin
    {
        public override PluginInformation Info => new PluginInformation
        {
            Name = "Example Provider",
            Description = "An example data provider plugin.",
            AuthorName = "Tumppi066",
        };

        public override float TickRate => 1.0f;
        private MySettings? _settings;
        private SettingsHandler? _settingsHandler;
        private string _settingsFilename = "example_settings.json";

        public override void OnEnable()
        {
            base.OnEnable();

            _settingsHandler = new SettingsHandler();
            _settings = _settingsHandler.Load<MySettings>(_settingsFilename);
            _settingsHandler.RegisterListener<MySettings>(_settingsFilename, OnSettingsChanged);

            var testTutorial = new Tutorial("Test Tutorial", "This is a test tutorial.", "ExampleProvider", new List<TutorialSection>
            {
                new TutorialSection
                {
                    Title = "Introduction",
                    Actions = new List<TutorialAction>
                    {
                        new TutorialAction
                        {
                            ActionType = TutorialActionType.ShowMessageWaitNext,
                            Message = "Welcome to ETS2LA!",
                            ScreenPositionCallback = LocationFunction
                        },
                        new TutorialAction
                        {
                            ActionType = TutorialActionType.ShowMessage,
                            Message = "This is the second message."
                        },
                        new TutorialAction
                        {
                            ActionType = TutorialActionType.ShowMessageWaitNext,
                            Message = "This is the third message."
                        }
                    }
                }
            });

            TutorialHandler.Current.RegisterTutorial(testTutorial);
            TutorialHandler.Current.StartTutorial("Test Tutorial");
        }

        private (int, int) LocationFunction()
        {
            float i = DateTime.Now.Millisecond / 1000f % 2;
            int j = (int)(i * 300);
            return (j, 200);
        }

        private void OnSettingsChanged(MySettings data)
        {
            Logger.Info($"ExampleProvider detected settings change: ExampleValue = {data.ExampleValue}");
            _settings = data;
        }

        public override void Tick()
        {
            Events.Current.Publish<float>("ExampleProvider.Time", System.DateTime.Now.Microsecond);
        }
    }
}