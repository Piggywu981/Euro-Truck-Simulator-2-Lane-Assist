namespace ETS2LA.Tutorials;

public enum TutorialActionType
{
    ShowMessage,
    ShowMessageWaitForNext,
    PointAtScreen,
    PointAtCoordinate,
    PlaySound,
    WaitForInput,
    WaitForEvent
}

public class TutorialAction
{
    public TutorialActionType ActionType { get; set; }
    public string? Message { get; set; }
    public string? SoundFile { get; set; }
    public (float x, float y)? ScreenPosition { get; set; }
    public (float x, float y)? Coordinate { get; set; }
    public string? ControlEventId { get; set; }
    public string? WaitEventId { get; set; }

    public TutorialAction(string? message, string? soundFile, (float x, float y)? screenPosition, (float x, float y)? coordinate, string? controlEventId, string? waitEventId)
    {
        Message = message;
        SoundFile = soundFile;
        ScreenPosition = screenPosition;
        Coordinate = coordinate;
        ControlEventId = controlEventId;
        WaitEventId = waitEventId;
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