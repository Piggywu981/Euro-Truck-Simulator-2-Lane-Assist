using Hexa.NET.ImGui;
using ETS2LA.Overlay;
using ETS2LA.Audio;
using Avalonia.Data;
using ETS2LA.Overlay.AR;

namespace ETS2LA.Tutorials;

public enum TutorialActionType
{
    // Instant actions, these don't clear automatically.
    // Instead they wait for the next wait action to be
    // executed. PlaySound is an exception, as it only
    // plays a sound once.
    ShowMessage,
    ShowImguiWindow,
    PointAtScreen,
    PointAtCoordinate,
    PlaySound,

    // These are so called "wait" actions.
    // They will clear previous instant actions
    // once completed.
    ShowMessageWaitNext,
    WaitForInput,
    WaitForEvent
}

public struct TutorialAction
{
    public TutorialActionType ActionType;
    public string? Message;
    public string? SoundFile;
    public (float x, float y)? ScreenPosition;
    public Func<(int, int)>? ScreenPositionCallback;
    public (float x, float y)? Size;
    public Func<(int, int)>? SizeCallback;
    public (float x, float y)? Coordinate;
    public Func<ARCoordinate>? CoordinateCallback;
    public string? ControlEventId;
    public string? WaitEventId;
    public Delegate? ImGuiCallback;
    public WindowDefinition ImGuiWindowDefinition;
}

public struct TutorialSection
{
    public string Title;
    public List<TutorialAction> Actions;
}

public class Tutorial
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Source { get; private set; }
    public List<TutorialSection> Sections { get; private set; }

    public Tutorial(string title, string description, string source, List<TutorialSection> sections)
    {
        Title = title;
        Description = description;
        Source = source;
        Sections = sections;
    }
}

public class TutorialExecutor
{
    public bool shutdown = false;

    private Tutorial tutorial;
    private int sectionIndex;
    private int actionIndex;

    private bool clearAfter = false;
    private bool actionLocked = false;

    private List<Action> imguiCallbacks;
    private List<WindowDefinition> imguiWindowDefinitions;

    public TutorialExecutor(Tutorial tutorial)
    {
        this.tutorial = tutorial;
        sectionIndex = 0;
        actionIndex = 0;
        imguiCallbacks = new List<Action>();
        imguiWindowDefinitions = new List<WindowDefinition>();

        Thread tutorialThread = new Thread(ExecutionThread);
        tutorialThread.Start();
    }

    private void RegisterImGuiWindow(WindowDefinition def, Action callback)
    {
        imguiWindowDefinitions.Add(def);
        imguiCallbacks.Add(callback);
        OverlayHandler.Current.RegisterWindow(imguiWindowDefinitions.Last(), imguiCallbacks.Last());
    }

    public void ExecuteAction()
    {
        var action = tutorial.Sections[sectionIndex].Actions[actionIndex];
        switch (action.ActionType)
        {
            case TutorialActionType.ShowMessage:
                RegisterImGuiWindow(new WindowDefinition
                {
                    Title = $"ShowMessage {sectionIndex} - {actionIndex}",
                    Flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize,
                    X = action.ScreenPosition.HasValue ? (int)action.ScreenPosition.Value.x : -1,
                    Y = action.ScreenPosition.HasValue ? (int)action.ScreenPosition.Value.y : -1,
                    LocationFunction = action.ScreenPositionCallback != null ? action.ScreenPositionCallback : null
                }, () =>
                {
                    ImGui.Text(action.Message);
                });
                break;
            case TutorialActionType.ShowMessageWaitNext:
                RegisterImGuiWindow(new WindowDefinition
                {
                    Title = $"ShowMessageWaitNext {sectionIndex} - {actionIndex}",
                    Flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize,
                    X = action.ScreenPosition.HasValue ? (int)action.ScreenPosition.Value.x : -1,
                    Y = action.ScreenPosition.HasValue ? (int)action.ScreenPosition.Value.y : -1,
                    LocationFunction = action.ScreenPositionCallback != null ? action.ScreenPositionCallback : null
                }, () =>
                {
                    ImGui.Text(action.Message);
                    ImGui.Button("Next");
                    if (ImGui.IsItemClicked())
                    {
                        actionLocked = false;
                    }
                });
                clearAfter = true;
                actionLocked = true;
                break;
            case TutorialActionType.ShowImguiWindow:
                // Show ImGui window
                break;
            case TutorialActionType.PointAtScreen:
                // Point at a specific screen position
                break;
            case TutorialActionType.PointAtCoordinate:
                // Point at a specific coordinate
                break;
            case TutorialActionType.PlaySound:
                // Play a sound
                break;
            case TutorialActionType.WaitForInput:
                // Wait for user input
                break;
            case TutorialActionType.WaitForEvent:
                // Wait for a specific event
                break;
        }
    }

    public void ExecuteSection()
    {
        while (actionIndex < tutorial.Sections[sectionIndex].Actions.Count && !shutdown)
        {
            ExecuteAction();
            while (actionLocked && !shutdown)
            {
                Thread.Sleep(100);
            }

            if (clearAfter)
            {
                int windowCount = imguiCallbacks.Count;
                for (int i = 0; i < windowCount; i++)
                {
                    OverlayHandler.Current.UnregisterWindow(imguiWindowDefinitions[i]);
                }
                imguiWindowDefinitions.Clear();
                imguiCallbacks.Clear();
                clearAfter = false;
            }

            actionIndex++;
        }
    }

    public void ExecutionThread()
    {
        while (sectionIndex < tutorial.Sections.Count && !shutdown)
        {
            ExecuteSection();
            sectionIndex++;
        }
    }
}