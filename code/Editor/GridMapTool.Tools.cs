using Sandbox;

namespace Editor;

public partial class GridMapTool
{
	private SceneTraceResult projectedPoint;
	public int FloorHeight = 128;

	public SceneTraceResult CursorRay (Ray cursorRay )
	{
		var tr = Scene.Trace.Ray( cursorRay, 5000 )
		.UseRenderMeshes( true )
		.UsePhysicsWorld( false )
		.WithTag( "gridtile" )
		.Run();

		return tr;
	}
	public void HandlePlacement( SceneTraceResult tr, Ray cursorRay )
	{
		if ( SelectedJsonObject is null ) return;
		using var scope = SceneEditorSession.Scope();
		
		projectedPoint = ProjectRayOntoGroundPlane( cursorRay.Position, cursorRay.Forward, floors );
		
		if ( projectedPoint.Hit )
		{
			// Snap the projected point to the grid and adjust for floor height
			var snappedPosition = projectedPoint.EndPosition;

			var go = new GameObject( true, "GridTile" );
			PrefabUtility.MakeGameObjectsUnique( SelectedJsonObject );
			go.Deserialize( SelectedJsonObject );
			go.Parent = CurrentGameObjectCollection;
			go.Transform.Position = snappedPosition;
			go.Transform.Rotation = Rotation.FromPitch( -90 ) * rotation;
			go.Tags.Remove( "group" );
			go.Tags.Add( "gridtile" );

			go.EditLog( "Grid Placed", go );
		}
	}

	public void HandleRemove( Ray cursorRay )
	{		
		if ( CursorRay(cursorRay).Hit )
		{
			Log.Info( $"Remove {CursorRay( cursorRay ).GameObject.Name}" );
			CursorRay( cursorRay ).GameObject.Destroy();
		}
	}
	public void HandleGetMove( Ray cursorRay )
	{
		if ( CursorRay( cursorRay ).Hit )
		{
			Log.Info( $"Start Moving {CursorRay( cursorRay ).GameObject.Name}" );
			SelectedObject = CursorRay( cursorRay ).GameObject;
			lastRot = SelectedObject.Transform.Rotation;
			beenRotated = false;
		}
	}
	Rotation lastRot;
	bool beenRotated;
	public void HandleMove( Ray cursorRay )
	{
		projectedPoint = ProjectRayOntoGroundPlane( cursorRay.Position, cursorRay.Forward, floors );

		if ( projectedPoint.Hit )
		{
			// Snap the projected point to the grid and adjust for floor height
			var snappedPosition = projectedPoint.EndPosition;

			SelectedObject.Transform.Position = snappedPosition;

			// Only update rotation if 'shouldRotate' is true
			if ( beenRotated )
			{
				SelectedObject.Transform.Rotation = Rotation.FromPitch( -90 ) * rotation;
			}
			else
			{
				// Keep the last rotation
				SelectedObject.Transform.Rotation = lastRot;
			}
		}
	}

	public void HandleCopyPlace( SceneTraceResult trace, Ray cursorRay )
	{
		if ( CursorRay( cursorRay ).Hit )
		{
			using var scope = SceneEditorSession.Scope();

			var options = new GameObject.SerializeOptions();
			var selection = CopyObject;
			var json = selection.Serialize( options );
			
			SceneUtility.MakeGameObjectsUnique( json );
			var go = SceneEditorSession.Active.Scene.CreateObject();

			go.Deserialize( json );
			go.MakeNameUnique();
			go.Parent = CurrentGameObjectCollection;
			go.Transform.Position = GetGizmoPosition( trace, cursorRay );
			go.Transform.Rotation = GizmoGameObject.Transform.Rotation;

			go.Tags.Add( "gridtile" );
		}
	}

	public void HandleCopy( Ray cursorRay )
	{
		if ( CursorRay( cursorRay ).Hit )
		{
			if( CursorRay( cursorRay ).GameObject.IsPrefabInstance)
			{
				var prefab = CursorRay( cursorRay ).GameObject.Root;
				CopyObject = prefab;
			}
			else
			{
				CopyObject = CursorRay( cursorRay ).GameObject;
			}

			Log.Info( $"Copy {CopyObject}" );
			beenRotated = false;
		}
	}
	
	bool _prevlessFloor = false;
	bool _prevmoreFloor = false;
	public void FloorHeightShortCut()
	{
		if ( Application.IsKeyDown( KeyCode.Q ) && !_prevlessFloor )
		{
			DoFloors( -FloorHeight )();
			so.Delete();
			so = null;
			Grid( new Vector2( 16384, 16384 ), Gizmo.Settings.GridSpacing, Gizmo.Settings.GridOpacity );

			floorLabel.Text = floorCount.ToString();
		}
		else if ( Application.IsKeyDown( KeyCode.E ) && !_prevmoreFloor )
		{
			DoFloors( FloorHeight )();
			so.Delete();
			so = null;
			Grid( new Vector2( 16384, 16384 ), Gizmo.Settings.GridSpacing, Gizmo.Settings.GridOpacity );
			
			floorLabel.Text = floorCount.ToString();
		}

		_prevlessFloor = Application.IsKeyDown( KeyCode.Q );
		_prevmoreFloor = Application.IsKeyDown( KeyCode.E );
	}
	
	//Nasty
	bool _prevlessRotationZ = false;
	bool _prevmoreRotationZ = false;
	bool _prevlessRotationX = false;
	bool _prevmoreRotationX = false;
	bool _prevlessRotationY = false;
	bool _prevmoreRotationY = false;

	public void HandleRotation()
	{

		if ( Application.IsKeyDown( KeyCode.Num1 ) && Gizmo.IsShiftPressed && !_prevlessRotationZ )
		{
			DoRotation( true, GroundAxis.Z )();
			SnapToClosest( GroundAxis.Z );
		}
		else if ( Application.IsKeyDown( KeyCode.Num1 ) && Gizmo.IsAltPressed && !_prevmoreRotationZ )
		{
			DoRotation( false, GroundAxis.Z )();
			SnapToClosest( GroundAxis.Z );
		}

		if ( Application.IsKeyDown( KeyCode.Num2 ) && Gizmo.IsShiftPressed && !_prevlessRotationX )
		{
			DoRotation( true, GroundAxis.X )();
			SnapToClosest( GroundAxis.X );
		}
		else if ( Application.IsKeyDown( KeyCode.Num2 ) && Gizmo.IsAltPressed && !_prevmoreRotationX )
		{
			DoRotation( false, GroundAxis.X )();
			SnapToClosest( GroundAxis.X );
		}

		if ( Application.IsKeyDown( KeyCode.Num3 ) && Gizmo.IsShiftPressed && !_prevlessRotationY )
		{
			DoRotation( true, GroundAxis.Y )();
			SnapToClosest( GroundAxis.Y );
		}
		else if ( Application.IsKeyDown( KeyCode.Num3 ) && Gizmo.IsAltPressed && !_prevmoreRotationY )
		{
			DoRotation( false, GroundAxis.Y )();
			SnapToClosest( GroundAxis.Y );
		}

		_prevlessRotationZ = Application.IsKeyDown( KeyCode.Num1 ) && Gizmo.IsShiftPressed;
		_prevmoreRotationZ = Application.IsKeyDown( KeyCode.Num1 ) && Gizmo.IsAltPressed;
		_prevlessRotationX = Application.IsKeyDown( KeyCode.Num2 ) && Gizmo.IsShiftPressed;
		_prevmoreRotationX = Application.IsKeyDown( KeyCode.Num2 ) && Gizmo.IsAltPressed;
		_prevlessRotationY = Application.IsKeyDown( KeyCode.Num3 ) && Gizmo.IsShiftPressed;
		_prevmoreRotationY = Application.IsKeyDown( KeyCode.Num3 ) && Gizmo.IsAltPressed;
	}

	bool _prevlessRotationSnap = false;
	bool _prevmoreRotationSnap = false;

	public void UpdateRotationSnapWithKeybind()
	{
		if ( Gizmo.IsCtrlPressed && Application.IsKeyDown( KeyCode.BraceLeft ) && !_prevlessRotationSnap )
		{
			CycleRotationSnap( -1 );
		}
		else if ( Gizmo.IsCtrlPressed && Application.IsKeyDown( KeyCode.BraceRight ) && !_prevmoreRotationSnap )
		{
			CycleRotationSnap( 1 );
			Log.Info( $"Rotation Snap: {rotationSnap}" );
		}

		_prevlessRotationSnap = Gizmo.IsCtrlPressed && Application.IsKeyDown( KeyCode.BraceLeft );
		_prevmoreRotationSnap = Gizmo.IsCtrlPressed && Application.IsKeyDown( KeyCode.BraceRight );
	}

	private void CycleRotationSnap( int direction )
	{
		// Get all values of the RotationSnap enum
		var values = Enum.GetValues( typeof( RotationSnap ) ).Cast<RotationSnap>().ToList();

		// Find the current index of the enum
		int currentIndex = values.IndexOf( CurrentRotationSnap );

		// Calculate the new index
		int newIndex = (currentIndex + direction + values.Count) % values.Count;

		// Update the CurrentRotationSnap to the new value
		CurrentRotationSnap = values[newIndex];
	}
}
