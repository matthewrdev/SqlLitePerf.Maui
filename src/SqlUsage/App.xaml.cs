using SqlUsage.Helpers;

namespace SqlUsage;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new MainPage();
	}

    public static App Instance => Current as App;

    public ChinookDatabase Database { get; private set; }

    protected override void OnStart()
    {
        base.OnStart();

		SetupDatabase();
    }

    private void SetupDatabase()
    {
        const string dbResourceName = "SqlUsage.Chinook_Sqlite.sqlite";

        const string dbFileName = "chinook.db";
        string databaseFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, dbFileName);

        if (!File.Exists(databaseFilePath))
        {
            ResourcesHelper.ExtractResourceToFile(this.GetType().Assembly, dbResourceName, databaseFilePath);
        }

        this.Database = new ChinookDatabase(databaseFilePath);
    }
}

