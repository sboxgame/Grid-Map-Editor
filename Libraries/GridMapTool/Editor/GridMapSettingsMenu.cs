using Editor;

public static class GridMapSettingsMenu
{
	[Menu( "Editor", "GridMapTool/Settings" )]
	public static void OpenMyMenu()
	{
		new GridMapSettings();
	}
}

public class GridMapSettings : BaseWindow
{
	public GridMapSettings()
	{
		WindowTitle = "GridMap Settings";
		SetWindowIcon( "settings" );
		Name = "GridMap Settings";
		Size = new Vector2( 400, 700 );

		GridHeight = ProjectCookie.Get( "GridHeight", 128 );
		GridMultiplier = ProjectCookie.Get( "GridMultiplier", 1.0f );

		CreateUI();
		Show();
	}

	Widget container;

	float GridHeight { get; set; } = 128;
	float GridMultiplier { get; set; } = 1.0f;

	public void CreateUI()
	{
		Layout = Layout.Column();
		Layout.Margin = 4;
		Layout.Spacing = 4;

		container = new Widget( this );

		var properties = new ControlSheet();
		var nameLabel = new Label.Subtitle( "Grid Map Settings" );
		nameLabel.Margin = 16;

		var so = this.GetSerialized();

		properties.AddRow( so.GetProperty( nameof( GridHeight ) ) );
		properties.AddRow( so.GetProperty( nameof( GridMultiplier ) ) );

		var saveButton = new Button.Primary( "Save Settings", "add_circle" );
		saveButton.Clicked = SaveSettings;

		Layout.Add( nameLabel );
		Layout.Add( properties );
		Layout.Add( saveButton );
		Layout.Add( container );

		Layout.AddStretchCell();
	}

	public void SaveSettings()
	{
		// Save settingsPackage
		ProjectCookie.Set( "GridHeight", GridHeight );
		ProjectCookie.Set( "GridMultiplier", GridMultiplier );
	}
}
