using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Prime31.PaletteKit
{
	[Serializable]
	public class ColorPalette : ScriptableObject
	{
		[SerializeField]
		public string paletteName;

		[SerializeField]
		public List<Color> _colors;
		private List<string> _hexCodes = new List<string>();


		string colorToHex( Color32 color )
		{
			return color.r.ToString( "X2" ) + color.g.ToString( "X2" ) + color.b.ToString( "X2" );
		}


		Color hexToColor( string hex )
		{
			byte r = byte.Parse( hex.Substring( 0, 2 ), System.Globalization.NumberStyles.HexNumber );
			byte g = byte.Parse( hex.Substring( 2, 2 ), System.Globalization.NumberStyles.HexNumber );
			byte b = byte.Parse( hex.Substring( 4, 2 ), System.Globalization.NumberStyles.HexNumber );

			return new Color32( r, g, b, 255 );
		}


		public void recalculateHexCodes()
		{
			_hexCodes.Clear();
			for( var i = 0; i < _colors.Count; i++ )
				_hexCodes.Add( colorToHex( _colors[i] ) );
		}


		public void OnEnable()
		{
			//hideFlags = HideFlags.HideAndDontSave;
			if( paletteName == null || paletteName == string.Empty )
			{
				paletteName = "New Palette";

				_colors = new List<Color>( 5 );
				for( var i = 0; i < 5; i++ )
					_colors.Add( new Color( UnityEngine.Random.Range( 0f, 1f ), UnityEngine.Random.Range( 0f, 1f ), UnityEngine.Random.Range( 0f, 1f ) ) );
			}

			recalculateHexCodes();
		}


		public void OnGUI()
		{
			paletteName = GUILayout.TextField( paletteName );
			var requiresColorPaletteUpdate = false;
			var removeColorAtIndex = -1;

			for( var i = 0; i < _colors.Count; i++ )
			{
				GUILayout.BeginHorizontal();
				{
					EditorGUI.BeginChangeCheck();
					_hexCodes[i] = GUILayout.TextField( _hexCodes[i], GUILayout.Width( 60 ) );

					if( EditorGUI.EndChangeCheck() )
					{
						if( _hexCodes[i].Length == 7 && _hexCodes[i][0] == '#' )
							_hexCodes[i] = _hexCodes[i].Substring( 1 );

						if( _hexCodes[i].Length == 6 )
						{
							_colors[i] = hexToColor( _hexCodes[i] );
							requiresColorPaletteUpdate = true;
						}
					}


					EditorGUI.BeginChangeCheck();
					_colors[i] = EditorGUILayout.ColorField( _colors[i], GUILayout.ExpandWidth( true ) );

					if( GUILayout.Button( "x" ) )
					{
						removeColorAtIndex = i;
						requiresColorPaletteUpdate = true;
					}

					if( EditorGUI.EndChangeCheck() )
					{
						_hexCodes[i] = colorToHex( _colors[i] );
						requiresColorPaletteUpdate = true;
					}
				}
				GUILayout.EndHorizontal();
			}

			if( GUILayout.Button( "Add Color to Palette" ) )
			{
				_colors.Add( Color.white );
				_hexCodes.Add( "FFFFFF" );
				requiresColorPaletteUpdate = true;
			}

			if( removeColorAtIndex >= 0 )
			{
				_colors.RemoveAt( removeColorAtIndex );
				_hexCodes.RemoveAt( removeColorAtIndex );
			}

			if( requiresColorPaletteUpdate )
				PaletteChooserWindow.setColors( _colors );

			if( GUI.changed )
				Undo.RecordObject( this, "ColorPalette Modified" );
		}
	}
}