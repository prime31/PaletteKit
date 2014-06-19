using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


namespace Prime31.PaletteKit
{
	[System.Serializable]
	public class PaletteSceneViewWindow : ScriptableObject
	{
		[SerializeField]
		private int _itemsPerRow = 4;
		[SerializeField]
		private int _totalRows;
		[SerializeField]
		private float _scale = 15f;
		[SerializeField]
		private Texture2D _texture;
		[SerializeField]
		private Rect _windowRect = new Rect( 0, 0, 200, 200 );



		public void OnEnable()
		{
			hideFlags = HideFlags.HideAndDontSave;
			_windowRect = loadRectFromPrefs( _windowRect );
		}


		public void OnDisable()
		{
			// OnDisable gets called a lot for some reason. we only need to persist once
			if( _texture )
				saveRectToPrefs( _windowRect );

			Editor.DestroyImmediate( _texture );
			_texture = null;
		}


		public void OnSceneGUI()
		{
			if( Event.current.type == EventType.Repaint )
				return;

			if( _texture == null )
			{
				var colors = new List<Color> { Color.white, Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.yellow };
				_texture = textureWithColors( colors, _itemsPerRow );
			}


			Handles.BeginGUI();

			_windowRect = GUILayout.Window( 224323, _windowRect, id =>
			{
				var textureRect = GUILayoutUtility.GetRect( _texture.width * _scale, _texture.height * _scale );
				if( Event.current.type == EventType.MouseDown && textureRect.Contains( Event.current.mousePosition ) )
				{
					// figure out where in the texture we clicked. we have to flip the y value since textures go from button to top
					var clickPosInTexture = Event.current.mousePosition - textureRect.position;

					var clickedTexX = Mathf.FloorToInt( clickPosInTexture.x / _scale );
					var clickedTexY = _totalRows - 1 - Mathf.FloorToInt( clickPosInTexture.y / _scale );
					var clickedColor = _texture.GetPixel( clickedTexX, clickedTexY );

					// PROBUILDER REFERENCE
					var pbType = System.Type.GetType( "pb_VertexColorInterface, ProBuilderEditor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" );
					if( pbType != null )
					{
						var pbMethod = pbType.GetMethod( "SetFaceColors", new System.Type[] { typeof( Color32 ) } );
						if( pbMethod != null )
							pbMethod.Invoke( null, new object[] { (Color32)clickedColor } );
					}
				}

				GUI.DrawTexture( textureRect, _texture );
				GUI.DragWindow();
			}, "Colors", GUILayout.MaxWidth( _texture.width ), GUILayout.MaxHeight( _texture.height ) );

			Handles.EndGUI();
		}


		public void setColors( List<Color> colors )
		{
			if( _texture != null )
				Editor.DestroyImmediate( _texture );

			// get an itemsPerRow that is as close to square as possible
			_itemsPerRow = Mathf.CeilToInt( Mathf.Sqrt( colors.Count ) );
			_texture = textureWithColors( colors, _itemsPerRow );
		}


		Texture2D textureWithColors( List<Color> colors, int itemsPerRow )
		{
			// figure out our texture size based on the itemsPerRow and color count
			_totalRows = Mathf.CeilToInt( (float)colors.Count / (float)itemsPerRow );

			var tex = new Texture2D( itemsPerRow, _totalRows, TextureFormat.RGB24, false, true );
			tex.filterMode = FilterMode.Point;
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.hideFlags = HideFlags.HideAndDontSave;

			for( var i = 0; i < colors.Count; i++ )
			{
				var x = i % itemsPerRow;
				var y = _totalRows - 1 - Mathf.CeilToInt( i / itemsPerRow );
				tex.SetPixel( x, y, colors[i] );
			}

			tex.Apply();

			return tex;
		}


		// docking not implemented due to buggy Unity height and yMax/yMin values
	//	void tryToDockWindow()
	//	{
	//		var sceneRect = SceneView.currentDrawingSceneView.position;
	//		var closeEnough = 50f;
	//		var yMin = _windowRect.center.y - ( _texture.height * _scale / 2f );
	//		var yMax = _windowRect.center.y + ( _texture.height * _scale / 2f );
	//
	//		// cant do bottom due to messed up height and yMax/yMin values
	//		// left
	//		if( _windowRect.xMin < closeEnough )
	//		{
	//			_windowRect.xMin = 0;
	//			SceneView.currentDrawingSceneView.Repaint();
	//		}
	//		else if( sceneRect.size.x - _windowRect.xMax < closeEnough )
	//		{
	//			_windowRect.x = sceneRect.size.x - _windowRect.size.x;
	//			SceneView.currentDrawingSceneView.Repaint();
	//		}
	//	}


		void saveRectToPrefs( Rect rect )
		{
			EditorPrefs.SetFloat( "P31ColorPickerX", rect.x );
			EditorPrefs.SetFloat( "P31ColorPickerY", rect.y );
		}


		Rect loadRectFromPrefs( Rect rect )
		{
			if( EditorPrefs.HasKey( "P31ColorPickerX" ) )
			{
				rect.x = EditorPrefs.GetFloat( "P31ColorPickerX" );
				rect.y = EditorPrefs.GetFloat( "P31ColorPickerY" );
			}

			return rect;
		}
	}
}
