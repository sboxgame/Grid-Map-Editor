using Sandbox.UI;

namespace Editor;

public partial class GridMapTool
{
	private Label floorLabel;
	private Label currentaxisLabel;

	private LineEdit heightInput;
	private WidgetWindow gridwindowWidget;
	private FloatSlider slider;

	[Sandbox.Range( 0, 100 )]
	public float thumbnailScale { get; set; }

	public GroundAxis Axis { get; set; } = GroundAxis.Z;
	public enum GroundAxis
	{

		X,
		Y,
		Z
	}
	
	ListStyle CurrentListStyle = ListStyle.Grid;
	private SegmentedControl paintmode;
	private SerializedProperty resourceso;

	public enum ListStyle
	{
		Grid,
		List
	}
	void ToolWindow(SerializedObject so)
	{
		{
			gridwindowWidget = new WidgetWindow( SceneOverlay );
			gridwindowWidget.MaximumWidth = 300;
			gridwindowWidget.WindowTitle = "Grid Map Tool";

			var row = Layout.Column();
			/*
			paintmode = row.Add( new SegmentedControl() );
			paintmode.AddOption( "Place", "brush" );
			paintmode.AddOption( "Remove", "delete" );
			paintmode.AddOption( "Move", "open_with" );
			paintmode.AddOption( "Copy", "content_copy" );

			paintmode.OnSelectedChanged += ( s ) =>
			{
				CurrentPaintMode = Enum.Parse<PaintMode>( s );
			};
			*/
			var collectionrow = Layout.Row();

			var collectionLabel = new Label( "Collection :" );
			var collectionCombo = collectionDropDown;

			var collectionButton = new Button( "", "add" );
			collectionButton.ButtonType = "clear";
			collectionButton.Clicked = () =>
			{
				var popup = new NewCollectionObjectWindow();
				popup.Show();
				popup.gridmapToolDockWidget = this;
			};
			collectionButton.MaximumWidth = 32;

			collectionrow.AddSpacingCell( 5 );
			collectionrow.Add( collectionLabel );
			collectionrow.AddSpacingCell( 1 );
			collectionrow.Add( collectionCombo );
			collectionrow.Add( collectionButton );

			var iconrow = Layout.Row();

			var search = new LineEdit();
			search.PlaceholderText = "Search...";
			search.TextEdited += OnSearchTextChanged;
			iconrow.Add( search );
			
			slider = iconrow.Add(new FloatSlider( gridwindowWidget ));
			slider.Minimum = 48;
			slider.Maximum = 128;
			slider.MinimumWidth = 150;
			slider.Value = 64;

			iconrow.AddSpacingCell( 2 );
			var buttonGrid = iconrow.Add( new Button( "", "grid_view" ) );
			buttonGrid.OnPaintOverride += () =>
			{
				PaintButton( "grid_view", ListStyle.Grid );
				return true;
			};

			buttonGrid.MaximumSize = 24;
			var buttonList = iconrow.Add( new Button() );
			buttonList.MaximumSize = 24;
			buttonList.OnPaintOverride += () =>
			{
				PaintButton( "reorder", ListStyle.List );
				return true;
			};

			buttonGrid.Clicked += () => { CurrentListStyle = ListStyle.Grid; buttonList.Update(); };
			buttonList.Clicked += () => { CurrentListStyle = ListStyle.List; buttonGrid.Update(); };

			tilelistView.ItemAlign = Align.Center;
			tilelistView.ItemSelected = ( item ) =>
			{
				if ( item is TileList data )
				{
					SelectedJsonObject = data.jsonObject;
					UpdatePaintObjectGizmo();
					//Log.Info( "Selected Model: " + SelectedJsonObject );
				}
			};
			
			tilelistView.OnPaintOverride += () => PaintListBackground( tilelistView );
			tilelistView.ItemPaint = PaintBrushItem;

			row.Add( collectionrow );
			row.Add( iconrow );
			
			row.AddSeparator();
			row.Add( tilelistView );

			gridwindowWidget.Layout = row;
			
			AddOverlay( gridwindowWidget, TextFlag.RightBottom, 0 );
		}
	}
	void UpdateWidgetValues()
	{
		gridwindowWidget.FixedHeight = SceneOverlay.Height;

		if ( CurrentListStyle == ListStyle.Grid )
		{
			tilelistView.ItemSize = slider.Value;
		}
		else if ( CurrentListStyle == ListStyle.List )
		{
			tilelistView.ItemSize = new Vector2( 275, slider.Value );
		}
	}
	void MainWindow(SerializedObject so)
	{
		{
			var window = new WidgetWindow( SceneOverlay, "Grid Map Controls" );
			window.MaximumWidth = 400;
			window.MinimumWidth = 400;

			var row = Layout.Column();		
			paintmode = row.Add( new SegmentedControl() );
			paintmode.AddOption( "Place", "brush" );
			paintmode.AddOption( "Remove", "delete" );
			paintmode.AddOption( "Move", "open_with" );
			paintmode.AddOption( "Copy", "content_copy" );
			paintmode.OnSelectedChanged += ( s ) =>
			{
				CurrentPaintMode = Enum.Parse<PaintMode>( s );
			};
			
			var cs = new ControlSheet();

			cs.AddRow( so.GetProperty( "PrefabResourse" ) );
			//cs.AddRow( so.GetProperty( "CurrentRotationSnap" ) );

			var rotationButtons = Layout.Column();
			var rotButtonX = rotationButtons.Add(Layout.Row());
			rotButtonX.Spacing = 8;
			rotButtonX.Add( new Label( "Rotation X: " ) );
			rotButtonX.AddStretchCell( 1 );
			rotButtonX.Add( new Button( "", "arrow_back" ) { Clicked = DoRotation( true, GroundAxis.X ), ButtonType = "clear" } );
			rotButtonX.Add( new Button( "", "arrow_forward" ) { Clicked = DoRotation( false, GroundAxis.X ), ButtonType = "clear" } );

			var rotButtonY = rotationButtons.Add( Layout.Row() );
			rotButtonY.Spacing = 8;
			rotButtonY.Add( new Label( "Rotation Y: " ) );
			rotButtonY.AddStretchCell( 1 );
			rotButtonY.Add( new Button( "", "arrow_back" ) { Clicked = DoRotation( true, GroundAxis.Y ), ButtonType = "clear" } );
			rotButtonY.Add( new Button( "", "arrow_forward" ) { Clicked = DoRotation( false, GroundAxis.Y ), ButtonType = "clear" } );

			var rotButtonZ = rotationButtons.Add( Layout.Row() );
			rotButtonZ.Spacing = 8;
			rotButtonZ.Add( new Label( "Rotation Z: " ) );
			rotButtonZ.AddStretchCell( 1 );
			rotButtonZ.Add( new Button( "", "arrow_back" ) { Clicked = DoRotation( true, GroundAxis.Z ), ButtonType = "clear" } );
			rotButtonZ.Add( new Button( "", "arrow_forward" ) { Clicked = DoRotation( false, GroundAxis.Z ), ButtonType = "clear" } );

			var FloorHeightValue = Layout.Row();
			FloorHeightValue.Add( new Label( "Floor Height:" ) );
			FloorHeightValue.AddStretchCell( 1 );
			heightInput = new LineEdit();
			FloorHeightValue.Add( heightInput );
			heightInput.MinimumHeight = Theme.RowHeight;
			heightInput.Text = "128";
			heightInput.TextChanged += (x) => FloorHeight = x.ToInt();

			var Floorbuttons = Layout.Row();
			Floorbuttons.Spacing = 8;
			Floorbuttons.Add( new Label( "Floor:" ) );
			Floorbuttons.AddStretchCell( 1 );
			floorLabel = new Label( floorCount.ToString() );
			Floorbuttons.Add( floorLabel );
			Floorbuttons.AddStretchCell( 1 );
			Floorbuttons.Add( new Button( "", "arrow_upward" ) { Clicked = () => { DoFloors( FloorHeight )(); floorLabel.Text = floorCount.ToString(); }, ButtonType = "clear" } );
			Floorbuttons.Add( new Button( "", "arrow_downward" ) { Clicked = () => { DoFloors( -FloorHeight )(); floorLabel.Text = floorCount.ToString(); }, ButtonType = "clear" } );

			var pop = Layout.Row();

			var popbutton = pop.Add( new Button( "Options", "more_horiz" ) { Clicked = () => { OpenDropdown( window ); } } );
			popbutton.ButtonType = "clear";
			popbutton.OnPaintOverride += () => 
			{
				if(popbutton.IsUnderMouse || optionsOpened )
				{
					Paint.SetPen( Theme.White.WithAlpha( .75f ) );
					Paint.SetFont( "Poppins", 8, 450 );
					Paint.DrawText( popbutton.LocalRect, "Options" );
					Paint.ClearBrush();
					Paint.ClearPen();
					Paint.SetBrush( Theme.ControlBackground.WithAlpha( .4f ) );
				}
				
				Paint.SetPen( Theme.White.WithAlpha( .5f ) );
				Paint.SetFont( "Poppins", 8, 450 );
				Paint.DrawText( popbutton.LocalRect, "Options");
				Paint.ClearBrush();
				Paint.ClearPen();
				Paint.SetBrush( Theme.ControlBackground.WithAlpha( .2f ) );
				Paint.DrawRect( popbutton.LocalRect.Shrink( 2 ) );

				return true;
			};
			popbutton.MaximumWidth = 100;

			row.Add( cs );
			row.Add( pop );

			//cs.Add( new Button( "Clear", "clear" ) { Clicked = ClearAll } );
			window.Layout = row;
			window.OnPaintOverride += () => PaintControlBackground( window );
			AddOverlay( window, TextFlag.CenterTop, 0 );
		}
	}

	bool optionsOpened = false;
	private WidgetWindow popup;

	void OpenDropdown( Widget window )
	{
		if ( optionsOpened )
		{
			popup.Close();
			optionsOpened = false;
			return;
		}
		optionsOpened = true;
		popup = new WidgetWindow( window );

		popup.WindowTitle = "Options";

		popup.MinimumWidth = window.Width;
		popup.Width = 500;
		popup.Layout = Layout.Column();
		popup.Layout.Margin = 16;
		
		var ps = new PropertySheet( popup );
		ps.AddSectionHeader( "Floors" );
		{
			floorLabel = ps.AddRow( "Current Floor Level:", new Label( floorCount.ToString() ) );
		}
		{
			heightInput = ps.AddRow( "Floor Height:", new LineEdit("Floor Height") );
			heightInput.Bind( "Value" ).From( () => FloorHeight.ToString(), x => { if ( float.TryParse( x, out var f ) ) FloorHeight = f.FloorToInt(); } );
			heightInput.Text = "128";
		}
		{
			var x = ps.AddRow("Floor Level:", new TwoButton() );
			x.button1.Clicked = () => { DoFloors( FloorHeight )(); floorLabel.Text = floorCount.ToString(); };
			x.button1.Icon = "arrow_upward";
			x.button2.Clicked = () => { DoFloors( -FloorHeight )(); floorLabel.Text = floorCount.ToString(); };
			x.button2.Icon = "arrow_downward";
		}

		ps.AddSectionHeader( "Rotation" );
		{
			var x = ps.AddRow( "Rotation X:", new TwoButton() );
			x.button1.Clicked = () => { DoRotation( true, GroundAxis.X )(); };
			x.button1.Icon = "arrow_back";
			x.button2.Clicked = () => { DoRotation( false, GroundAxis.X )(); };
			x.button2.Icon = "arrow_forward";

			var y = ps.AddRow( "Rotation Y:", new TwoButton() );
			y.button1.Clicked = () => { DoRotation( true, GroundAxis.Y )(); };
			y.button1.Icon = "arrow_back";
			y.button2.Clicked = () => { DoRotation( false, GroundAxis.Y )(); };
			y.button2.Icon = "arrow_forward";

			var z = ps.AddRow( "Rotation Z:", new TwoButton() );
			z.button1.Clicked = () => { DoRotation( true, GroundAxis.Z )(); };
			z.button1.Icon = "arrow_back";
			z.button2.Clicked = () => { DoRotation( false, GroundAxis.Z )(); };
			z.button2.Icon = "arrow_forward";
		}
		ps.AddSectionHeader( "Ground Axis" );
		{
			var w = ps.AddRow( "X", new Checkbox( "Key: C" ) );
			w.Bind( "Value" ).From( () => Axis == GroundAxis.X, x => { if ( x ) Axis = GroundAxis.X; currentaxisLabel.Text = Axis.ToString(); } );
		}
		{
			var w = ps.AddRow( "Y", new Checkbox( "Key: X" ) );
			w.Bind( "Value" ).From( () => Axis == GroundAxis.Y, x => { if ( x ) Axis = GroundAxis.Y; currentaxisLabel.Text = Axis.ToString(); } );
		}
		{
			var w = ps.AddRow( "Z", new Checkbox( "Key: Z" ) );
			w.Bind( "Value" ).From( () => Axis == GroundAxis.Z, x => { if ( x ) Axis = GroundAxis.Z; currentaxisLabel.Text = Axis.ToString(); } );
		}

		popup.Layout.Add( ps );
		
		AddOverlay( popup, TextFlag.None, window.Position + new Vector2( 0, window.Size.y ) );

		popup.Show();
	}
	private static bool PaintControlBackground( Widget widget )
	{
		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground.WithAlpha( .0f ) );
		Paint.DrawRect( widget.LocalRect, 12 );

		return true;
	}
	private static bool PaintListBackground( Widget widget )
	{
		
		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground.WithAlpha( 1f ) );
		Paint.DrawRect( widget.LocalRect );

		return false;
	}
	private void PaintButton(string icon, ListStyle style)
	{
		if( CurrentListStyle == style )
		{
			Paint.ClearPen();
			Paint.SetBrush( Theme.Green.WithAlpha( 0.10f ) );
			Paint.SetPen( Theme.Black.WithAlpha( 0.50f ) );
			Paint.DrawRect( new Rect( 0, 0, 24, 24 ), 3 );
		}

		Paint.ClearPen();
		Paint.SetBrush( Theme.Grey.WithAlpha( 0.10f ) );
		Paint.SetPen( Theme.Black.WithAlpha( 0.50f ) );
		Paint.DrawRect( new Rect( 0, 0, 24, 24 ), 3 );


		Paint.ClearPen();
		Paint.SetPen( Theme.Grey, 2.0f );
		Paint.SetFont( "Poppins", 6, 700 );
		Paint.DrawIcon( new Rect( 0, 0, 24, 24 ), icon, 16 );
	}
	private void PaintBrushItem( VirtualWidget widget )
	{
		var brush = (TileList)widget.Object;

		Paint.Antialiasing = true;
		Paint.TextAntialiasing = true;
	
		if ( brush.jsonObject == SelectedJsonObject )
		{
			Paint.ClearPen();
			Paint.SetBrush( Theme.Green.WithAlpha( 0.10f ) );
			Paint.SetPen( Theme.Green.WithAlpha( 0.50f ) );
			Paint.DrawRect( widget.Rect.Grow( 0 ), 3 );
		}
		
		Paint.ClearPen();
		Paint.SetBrush( Theme.White.WithAlpha( 0.01f ) );
		Paint.SetPen( Theme.White.WithAlpha( 0.05f ) );
		Paint.DrawRect( widget.Rect.Shrink( 2 ), 3 );

		if ( CurrentListStyle == ListStyle.List )
		{
			int iconSize = (int)Math.Min( widget.Rect.Height, widget.Rect.Width / 3 ); // Adjust for icon width ratio
			Vector2 iconPosition = new Vector2(
				widget.Rect.Left + 5, // 5 pixels padding from the left edge
				widget.Rect.Top + (widget.Rect.Height - iconSize) / 2 // Center icon vertically
			);

			var iconRect = new Rect( iconPosition.x, iconPosition.y, iconSize, iconSize );

			Paint.Draw( iconRect, brush.icon );
		}
		else
		{
			Paint.Draw( widget.Rect.Shrink(5), brush.icon );
		}


		var rect = widget.Rect;

		var textRect = rect.Shrink( 2 );

		if ( CurrentListStyle == ListStyle.Grid )
		{
			textRect.Top = textRect.Top + slider.Value * 0.65f;
			textRect.Top = textRect.Top + slider.Value / 10;

			Paint.ClearPen();
			Paint.SetBrush( Theme.Black.WithAlpha( 0.5f ) );
			Paint.DrawRect( textRect, 0.0f );
		}
		else
		{
			textRect.Right = textRect.Right + slider.Value;
		}
		
		Paint.Antialiasing = true;

		Paint.SetPen( Theme.Blue, 2.0f );
		Paint.ClearBrush();
		Paint.SetFont( "Poppins", slider.Value / 8f, 700 );
		Paint.DrawText( textRect, brush.name );
	}
}

public class PopUpOptions : WidgetWindow
{
	public PopUpOptions( Widget widget )
	{

	}
	protected override void OnPaint()
	{

	}

}

public class NewCollectionObjectWindow : BaseWindow
{

	Widget container;
	public GridMapTool gridmapToolDockWidget;

	public NewCollectionObjectWindow()
	{
		Size = new Vector2( 350, 150 );
		MinimumSize = Size;
		MaximumSize = Size;
		TranslucentBackground = true;
		NoSystemBackground = true;

		WindowTitle = "Add New Collection Object";
		SetWindowIcon( "rocket_launch" );

		Layout = Layout.Column();
		Layout.Margin = 4;
		Layout.Spacing = 4;

		container = new Widget( this );

		var properties = new PropertySheet( this );

		var nameLabel = new Label.Subtitle( "Add New Collection Object" );
		nameLabel.Margin = 16;

		var nameEdit = properties.AddLineEdit( "Collection Object Name", "" );
		var addbutton = new Button.Primary( "Add Collection", "add_circle" );
		addbutton.MaximumWidth = 100;
		addbutton.Clicked = () =>
		{
			gridmapToolDockWidget.CreateCollection( nameEdit.Text );
			/*
			var gameobject = new GameObject(true, nameEdit.Text );
			gameobject.Transform.Position = Vector3.Zero;
			gameobject.Tags.Add( "gridtile" );
			gameobject.Tags.Add( "collection" );
			*/
			gridmapToolDockWidget.collectionDropDown.AddItem( nameEdit.Text );
			Close();
		};
		Layout.Add( nameLabel );
		Layout.Add( properties );
		Layout.Add( addbutton );
		Layout.Add( container );

		Show();
	}
	public string SetButtonIcon = "settings";

	public Button buttonIcon;
}

public class TwoButton : Widget
{
	public Button button1;
	public Button button2;

	public string Button1Text { get; set; }
	public string Button2Text { get; set; }

	public TwoButton()
	{
		// Create a horizontal layout for the buttons
		var layout = Layout.Row();
		layout.Spacing = 4;
		// Create the fvar irst button
		button1 = layout.Add( new Button("", Button1Text, this ) );
		button1.ButtonType = "clear";

		layout.AddSpacingCell( 40 );

		// Create the svar econd button
		button2 = layout.Add( new Button("", Button2Text, this ) );
		button2.ButtonType = "clear";
		button2.Position = new Vector2( 50, 0 );
		layout.Add( button2 );

		MinimumHeight = 23;
	}
}
