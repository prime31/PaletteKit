using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;


/// <summary>
/// imports *most* ASE files. Websites that work great are:
/// - http://www.colorexplorer.com/mypalettes.aspx
/// - https://kuler.adobe.com
/// </summary>
namespace Prime31.PaletteKit
{
	public static class PSSwatchImporter
	{
		private enum ColorSpace
		{
			Rgb = 0,
			Hsb = 1,
			Cmyk = 2,
			Lab = 7,
			Grayscale = 8
		}

		private enum FileVersion
		{
			Version1 = 1,
			Version2
		}


		/// <summary>
		/// this doesn't take color space into account so it is pretty useless!
		/// </summary>
		static Color cmykToColor( float c, float m, float y, float k )
		{
			var black = 1f - k;
			float r = ( 1f - c ) * black;
			float g = ( 1f - m ) * black;
			float b = ( 1f - y ) * black;

			return new Color( r, g, b, 1f );
		}


		public static List<Color> readPhotoShopSwatchFile( string path )
		{
			var data = File.ReadAllBytes( path );
			var extension = System.IO.Path.GetExtension( path ).ToLower();

			if( extension == ".aco" )
				return readAcoPhotoShopSwatchFile( data );
			else
				return readAsePhotoShopSwatchFile( data );
		}


		static List<Color> readAcoPhotoShopSwatchFile( byte[] data )
		{
			var colorPalette = new List<Color>();

			//var version = getInt16( data, 0 );
			var totalColors = getInt16( data, 2 );

			var offset = 4;
			for( var i = 0; i < totalColors; i++ )
			{
				var colorModel = getInt16( data, offset );
				offset += sizeof( UInt16 );
				var value1 = getInt16( data, offset );
				offset += sizeof( UInt16 );
				var value2 = getInt16( data, offset );
				offset += sizeof( UInt16 );
				var value3 = getInt16( data, offset );
				offset += sizeof( UInt16 );
				var value4 = getInt16( data, offset );
				offset += sizeof( UInt16 );

				//Debug.Log( "colorModel: " + colorModel + ", value1: " + value1 + ", value2: " + value2 + ", value3: " + value3 + ", value4: " + value4 );

				switch( colorModel )
				{
					case 0: // rgb
						colorPalette.Add( new Color( (float)value1 / 65535f, value2 / 65535f, value3 / 65535f ) );
						break;

					case 1: // hsb
						float hue = (float)value1 / 65535f;
						float saturation = (float)value2 / 65535f;
						float brightness = (float)value3 / 65535f;


						colorPalette.Add( UnityEditor.EditorGUIUtility.HSVToRGB( hue, saturation, brightness ) );
						break;

					case 2: // cmyk
						float cyan = (float)value1 / 65535f;
						float magenta = (float)value2 / 65535f;
						float yellow = (float)value3 / 65535f;
						float black = (float)value4 / 65535f;

						Debug.Log( "c: " + cyan + ", m: " + magenta + ", y: " + yellow + ", b: " + black );

						colorPalette.Add( cmykToColor( cyan, magenta, yellow, black ) );
						break;

					case 7: // lab
						colorPalette.Add( Color.white );
						break;

					case 8: // gray
						var gray = 1f - (float)value1 / 10000f;
						colorPalette.Add( new Color( gray, gray, gray ) );
						break;

					default:
						colorPalette.Add( Color.white );
						break;
				}
			}

			try
			{
				var newVersion = getInt16( data, offset );
				offset += sizeof( UInt16 );

				if( newVersion == 2 )
				{
					// we dont really care about this for now since its the same just with names
				}
			}
			catch( Exception )
			{}

			return colorPalette;
		}


		static List<Color> readAsePhotoShopSwatchFile( byte[] data )
		{
			var colorPalette = new List<Color>();

			var versionHigh = getInt16( data, 4 );
			var versionLow = getInt16( data, 6 );
			var totalColors = getInt32( data, 8 );

			Debug.Log( "Adobe Swatch Exchange Format " + versionHigh + "." + versionLow + " with " + totalColors + " total colors." );


			int offset = 12;

			for( int b = 0; b < totalColors; b++ )
			{
				var blockType = getInt16( data, offset );
				offset += sizeof( UInt16 );

				var blockLength = getInt32( data, offset );
				offset += sizeof( UInt32 );

				switch( blockType )
				{
					case 0xC001: // Group Start Block (ignored)
						break;

					case 0xC002: // Group End Block (ignored)
						break;

					case 0x0001: // color
						colorPalette.Add( readColor( data, offset, b ) );
						break;

					default:
						Debug.LogError( "Warning: Block " + b + " is of an unknown type 0x" + blockType.ToString("X") + " (file corrupt?)" );
						break;
				}

				offset += (int)blockLength;
			}

			return colorPalette;
		}


		static Color readColor( byte[] data, int offset, int block )
		{
			var lengthOfName = getInt16( data, offset );
			offset += sizeof( UInt16 );

			lengthOfName *= 2; // turn into a count of bytes, not 16-bit characters

			//var colorName = Encoding.BigEndianUnicode.GetString( data, offset, lengthOfName - 2 ).Trim();
			offset += lengthOfName;

			var colorModel = Encoding.ASCII.GetString( data, offset, 4 ).Trim();
			offset += 4;


			switch( colorModel )
			{
				case "RGB":
					var value1 = getFloat32( data, offset );
					offset += sizeof( Single );
					var value2 = getFloat32( data, offset );
					offset += sizeof( Single );
					var value3 = getFloat32( data, offset );
					offset += sizeof( Single );

					return new Color( value1, value2, value3 );

				case "Gray":
					var gray = getFloat32( data, offset );
					offset += sizeof( Single );

					return new Color( gray, gray, gray );

				case "CMYK":
					var c = getFloat32( data, offset );
					offset += sizeof( Single );
					var m = getFloat32( data, offset );
					offset += sizeof( Single );
					var y = getFloat32( data, offset );
					offset += sizeof( Single );
					var k = getFloat32( data, offset );
					offset += sizeof( Single );

					return cmykToColor( c, m, y, k );

				case "HSB":
					var h = getFloat32( data, offset );
					offset += sizeof( Single );
					var s = getFloat32( data, offset );
					offset += sizeof( Single );
					var b = getFloat32( data, offset );
					offset += sizeof( Single );

					float hue = h / 182.04f; // 0-359
					float saturation = s / 655.35f; // 0-100
					float brightness = b / 655.35f; // 0-100

					return UnityEditor.EditorGUIUtility.HSVToRGB( hue, saturation, brightness );
				default:
					return Color.white;
			}
		}


		static UInt16 getInt16( byte[] data, int offset )
		{
			if( BitConverter.IsLittleEndian )
				Array.Reverse( data, offset, sizeof( UInt16 ) );
			return BitConverter.ToUInt16( data, offset );
		}


		static UInt32 getInt32( byte[] data, int offset )
		{
			if( BitConverter.IsLittleEndian )
				Array.Reverse( data, offset, sizeof( UInt32 ) );
			return BitConverter.ToUInt32( data, offset );
		}


		static Single getFloat32( byte[] data, int offset )
		{
			if( BitConverter.IsLittleEndian )
				Array.Reverse( data, offset, sizeof( Single ) );
			return BitConverter.ToSingle( data, offset );
		}

	}
}