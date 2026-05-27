namespace ETS2LA.Tutorials;

public class TutorialHandler
{
    private static readonly Lazy<TutorialHandler> _instance = new(() => new TutorialHandler());
    public static TutorialHandler Current => _instance.Value;

    public List<Tutorial> Tutorials { get; private set; }
    public List<TutorialExecutor> Executors { get; private set; }

    private TutorialHandler()
    {
        Tutorials = new List<Tutorial>();
        Executors = new List<TutorialExecutor>();
    }

    public void RegisterTutorial(Tutorial tutorial)
    {
        Tutorials.Add(tutorial);
    }

    public void RemoveTutorial(Tutorial tutorial)
    {
        Tutorials.Remove(tutorial);
    }

    public void StartTutorial(string tutorialTitle)
    {
        var tutorial = Tutorials.FirstOrDefault(t => t.Title == tutorialTitle);
        if (tutorial != null)
        {
            var executor = new TutorialExecutor(tutorial);
            Executors.Add(executor);
        }
    }

    public void Shutdown()
    {
        foreach (var executor in Executors)
        {
            executor.shutdown = true;
        }
    }
}