using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Prime31.PaletteKit
{
	[Serializable]
	public class Palettes : ScriptableObject
	{
		public List<ColorPalette> colorPalettes;
		private int _currentPaletteIndex = -1;


		public void OnEnable()
		{
			//hideFlags = HideFlags.HideAndDontSave;
			if( colorPalettes == null )
				colorPalettes = new List<ColorPalette>();
		}


		public void OnGUI()
		{
			var removeAtIndex = -1;

			for( var i = 0; i < colorPalettes.Count; i++ )
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label( colorPalettes[i].paletteName, GUILayout.Width( 60 ) );

					if( GUILayout.Button( "Delete" ) )
						removeAtIndex = i;

					if( GUILayout.Button( "Edit" ) )
						_currentPaletteIndex = i;

					if( GUILayout.Button( "Load" ) )
						PaletteChooserWindow.setColors( colorPalettes[i].colors );
				}
				GUILayout.EndHorizontal();
			}


			if( removeAtIndex >= 0 )
			{
				Undo.RecordObject( this, "Removing ColorPalette" );
				var currentlySelectedPalette = _currentPaletteIndex >= 0 && _currentPaletteIndex != removeAtIndex ? colorPalettes[_currentPaletteIndex] : null;
				UnityEngine.Object.DestroyImmediate( colorPalettes[removeAtIndex], true );
				AssetDatabase.SaveAssets();
				colorPalettes.RemoveAt( removeAtIndex );
				removeAtIndex = -1;

				if( currentlySelectedPalette != null )
					_currentPaletteIndex = colorPalettes.IndexOf( currentlySelectedPalette );
			}


			if( _currentPaletteIndex >= 0 )
			{
				GUILayout.Space( 30 );

				// ensure we stay in bounds in case of undo
				if( colorPalettes.Count < _currentPaletteIndex + 1 )
					_currentPaletteIndex = -1;
				else
					colorPalettes[_currentPaletteIndex].OnGUI();
			}

			GUILayout.Space( 30 );



			if( GUILayout.Button( "Add New Palette" ) )
			{
				Undo.RecordObject( this, "Adding ColorPalette" );
				var p = CreateInstance<ColorPalette>();
				p.paletteName = "New Palette";
				colorPalettes.Add( p );
				_currentPaletteIndex = colorPalettes.Count - 1;
				AssetDatabase.AddObjectToAsset( p, this );
			}

			if( GUILayout.Button( "New Palette from Swatch (ase or aco)" ) )
			{
				var path = EditorUtility.OpenFilePanel( "Choose PS Swatch File to Import (ase or aco file types)", Environment.GetFolderPath( Environment.SpecialFolder.Desktop ), "" );
				if( path != null && path != string.Empty )
				{
					var extension = System.IO.Path.GetExtension( path ).ToLower();
					if( extension != ".aco" && extension != ".ase" )
					{
						EditorUtility.DisplayDialog( "Palette Import Error", "Only .aco and .ase files are supported", "OK" );
						return;
					}

					var colors = PSSwatchImporter.readPhotoShopSwatchFile( path );
					if( colors.Count > 0 )
					{
						Undo.RecordObject( this, "Adding ColorPalette" );
						var p = CreateInstance<ColorPalette>();
						p.colors.Clear();
						p.paletteName = System.IO.Path.GetFileNameWithoutExtension( path );
						colorPalettes.Add( p );

						// add in our colors
						for( var i = 0; i < colors.Count; i++ )
							p.colors.Add( colors[i] );

						p.recalculateHexCodes();
						_currentPaletteIndex = colorPalettes.Count - 1;
						AssetDatabase.AddObjectToAsset( p, this );
					}
				}

			}

			if( GUILayout.Button( "Save Changes" ) )
			{
				AssetDatabase.SaveAssets();
			}

			GUILayout.Space( 10 );
		}
	}
}
