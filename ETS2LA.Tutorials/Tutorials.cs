namespace ETS2LA.Tutorials;

public class TutorialHandler
{
    private static readonly Lazy<TutorialHandler> _instance = new(() => new TutorialHandler());
    public static TutorialHandler Current => _instance.Value;
}