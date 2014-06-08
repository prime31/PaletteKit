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
		internal List<ColorPalette> _colorPalettes;
		private int _currentPaletteIndex = -1;


		public void OnEnable()
		{
			if( _colorPalettes == null )
				_colorPalettes = new List<ColorPalette>();
		}


		public void OnGUI()
		{
			var removeAtIndex = -1;

			for( var i = 0; i < _colorPalettes.Count; i++ )
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label( _colorPalettes[i].paletteName, GUILayout.Width( 60 ) );

					if( GUILayout.Button( "Delete" ) )
						removeAtIndex = i;

					if( GUILayout.Button( "Edit" ) )
						_currentPaletteIndex = i;

					if( GUILayout.Button( "Load" ) )
						PaletteChooserWindow.setColors( _colorPalettes[i]._colors );
				}
				GUILayout.EndHorizontal();
			}


			if( removeAtIndex >= 0 )
			{
				Undo.RecordObject( this, "Removing ColorPalette" );
				var currentlySelectedPalette = _currentPaletteIndex >= 0 && _currentPaletteIndex != removeAtIndex ? _colorPalettes[_currentPaletteIndex] : null;
				UnityEngine.Object.DestroyImmediate( _colorPalettes[removeAtIndex], true );
				AssetDatabase.SaveAssets();
				_colorPalettes.RemoveAt( removeAtIndex );
				removeAtIndex = -1;

				if( currentlySelectedPalette != null )
					_currentPaletteIndex = _colorPalettes.IndexOf( currentlySelectedPalette );
			}


			if( _currentPaletteIndex >= 0 )
			{
				GUILayout.Space( 30 );

				// ensure we stay in bounds in case of undo
				if( _colorPalettes.Count < _currentPaletteIndex + 1 )
					_currentPaletteIndex = -1;
				else
					_colorPalettes[_currentPaletteIndex].OnGUI();
			}

			GUILayout.Space( 30 );



			if( GUILayout.Button( "Add New Palette" ) )
			{
				Undo.RecordObject( this, "Adding ColorPalette" );
				var p = CreateInstance<ColorPalette>();
				p.paletteName = "New Palette";
				_colorPalettes.Add( p );
				_currentPaletteIndex = _colorPalettes.Count - 1;
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
						p._colors.Clear();
						p.paletteName = System.IO.Path.GetFileNameWithoutExtension( path );
						_colorPalettes.Add( p );

						// add in our colors
						for( var i = 0; i < colors.Count; i++ )
							p._colors.Add( colors[i] );

						p.recalculateHexCodes();
						_currentPaletteIndex = _colorPalettes.Count - 1;
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
