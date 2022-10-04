namespace MissileDefence;

internal static class Program
{
    internal static MainForm? form;
    internal static string applicationSpecificFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MissileDefenceAI");
    internal static string aiFolder = Path.Combine(applicationSpecificFolder, "AIModels");
    
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {       
        // Combine the base folder with your specific folder....

        // CreateDirectory will check if every folder in path exists and, if not, create them.
        // If all folders exist then CreateDirectory will do nothing.
        Directory.CreateDirectory(applicationSpecificFolder);
        Directory.CreateDirectory(aiFolder);

        ApplicationConfiguration.Initialize();
        form = new MainForm();
        Application.Run(form);
    }
}
