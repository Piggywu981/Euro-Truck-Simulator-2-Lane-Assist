using Hexa.NET.ImGui;
using ETS2LA.Overlay;
using ETS2LA.Audio;

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

public class TutorialAction
{
    public TutorialActionType ActionType { get; set; }
    public string? Message { get; set; }
    public string? SoundFile { get; set; }
    public (float x, float y)? ScreenPosition { get; set; }
    public Delegate? ScreenPositionCallback { get; set; }
    public (float x, float y)? Size { get; set; }
    public Delegate? SizeCallback { get; set; }
    public (float x, float y)? Coordinate { get; set; }
    public Delegate? CoordinateCallback { get; set; }
    public string? ControlEventId { get; set; }
    public string? WaitEventId { get; set; }
    public Delegate? ImGuiCallback { get; set; }
    public WindowDefinition ImGuiWindowDefinition { get; set; }

    public TutorialAction(TutorialActionType type, string? message, string? soundFile, (float x, float y)? screenPosition, (float x, float y)? coordinate, string? controlEventId, string? waitEventId, Delegate? imguiCallback)
    {
        ActionType = type;
        Message = message;
        SoundFile = soundFile;
        ScreenPosition = screenPosition;
        Coordinate = coordinate;
        ControlEventId = controlEventId;
        WaitEventId = waitEventId;
        ImGuiCallback = imguiCallback;
    }
}

public class TutorialSection
{
    public string Title { get; private set; }
    public List<TutorialAction> Actions { get; private set; }

    public TutorialSection(string title, List<TutorialAction> actions)
    {
        Title = title;
        Actions = actions;
    }
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