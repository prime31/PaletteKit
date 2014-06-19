using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Prime31.PaletteKit
{
	public class PaletteChooserWindow : EditorWindow
	{
		// default location and file name for saved palettes
		public const string _pathToAsset = "Assets/PaletteKit/Editor/ColorPalettes.asset";
		[SerializeField]
		private Palettes _palettes;
		[SerializeField]
		private Vector2 _scrollPosition;
		[SerializeField]
		private static PaletteSceneViewWindow _paletteWindow;


		[MenuItem( "Tools/ProBuilder/Vertex Colors/Color Palette Window" )]
		static void menuColorPaletteWindow()
		{
			GetWindow<PaletteChooserWindow>( "Palette Kit" );
		}


		void OnEnable()
		{
			if( _palettes == null )
			{
				// first, try to find one already stored on disk
				_palettes = (Palettes)AssetDatabase.LoadAssetAtPath( _pathToAsset, typeof( Palettes ) );

				if( _palettes == null )
				{
					_palettes = CreateInstance<Palettes>();
					AssetDatabase.CreateAsset( _palettes, _pathToAsset );
				}
			}

			// create our scene view palette window
			if( _paletteWindow == null )
			{
				_paletteWindow = ScriptableObject.CreateInstance<PaletteSceneViewWindow>();

				// if we have some palettes load up the first one
				if( _palettes.colorPalettes.Count > 0 )
					_paletteWindow.setColors( _palettes.colorPalettes[0].colors );
			}

			SceneView.onSceneGUIDelegate += onSceneView;
		}


		void OnDisable()
		{
			SceneView.onSceneGUIDelegate -= onSceneView;
		}


		void onSceneView( SceneView sv )
		{
			// PROBUILDER REFERENCE
			// we only need to show this if we are in face editing mode
			//if( pb_Editor.instanceIfExists.editLevel == ProBuilder2.EditorEnum.EditLevel.Geometry
			//	&& pb_Editor.instanceIfExists.GetSelectionMode() == ProBuilder2.EditorEnum.SelectMode.Face )
				_paletteWindow.OnSceneGUI();
		}


		void OnGUI()
		{
			GUILayout.Label( "Color Palettes", EditorStyles.boldLabel );

			_scrollPosition = GUILayout.BeginScrollView( _scrollPosition );
			_palettes.OnGUI();
			GUILayout.EndScrollView();
		}


		public static void setColors( List<Color> colors )
		{
			_paletteWindow.setColors( colors );
		}

	}
}