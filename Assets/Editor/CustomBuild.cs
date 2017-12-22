using UnityEngine;
using UnityEditor;
using System.Collections;

// Needed because Unity doesn't support custom pre-build steps, only post-build.
//  For all builds we'll be making these assumptions:
//   Linux
//   Universal (x86 + x64)
//   Scenes: prototype
public class CustomBuild : MonoBehaviour {

	private static string[] levels = { "Assets/Scenes/prototype.unity" };
	private static BuildTarget target = BuildTarget.StandaloneLinuxUniversal;

	private static string GetSavePath( )
	{
		string filePath = EditorUtility.SaveFilePanel( "Choose location for build", "", "", "x86" );
		
		if( filePath.EndsWith( ".x86" ) ) {
			filePath.Remove( filePath.Length - 5 );
		}

		return filePath;
	}

	private static void BuildVersionScript( )
	{
		// create a file called PCVersion that just has a member variable that returns a string used to identify builds
		//  will just find and increment the file version every build
		string version = "1";
		string filePath = "Assets/Scripts/Auto Generated/PCVersion.cs";
		if( System.IO.File.Exists( filePath ) ) {
			// load in the file and try to find the verion number
			string inFileText = System.IO.File.ReadAllText( filePath );
			if( !string.IsNullOrEmpty( inFileText ) ) {
				// assume we're looking for a bit of text of the format '_version = "n"', where n is the version number
				string startMarker = "_version = \"";
				int index = inFileText.IndexOf( startMarker );
				if( index >= 0 ) {
					int startValIdx = index + startMarker.Length;
					int endIndex = inFileText.IndexOf( '\"', startValIdx );
					if( endIndex >= 0 ) {
						int value = 0;
						try {
							value = int.Parse( inFileText.Substring( startValIdx, ( endIndex - startValIdx ) ) );
						} catch( System.Exception ) {
							Debug.LogWarning( "Problem parsing version number, resetting to 1" );
						}
						++value;
						version = value.ToString( );
					}
				}
			}
		}

		string outFileText = "// This is auto generated code, from CustomBuild.cs in the function BuildVersionScript( )\n" +
			"//  Do not change manually unless necessary\n" +
			"public class PCVersion {\n" +
			"\tprivate static string _version = \"" + version + "\";\n" +
			"\tpublic static string Version {\n" +
			"\t\tget { return _version; }\n" +
			"\t}\n" +
			"}\n";

		System.IO.File.WriteAllText( filePath, outFileText );
		AssetDatabase.ImportAsset( filePath );
	}

	private static void PreBuildSteps( )
	{
		EditorUtility.DisplayProgressBar("Pre-Build", "Processing pre-build steps", 0.0f );

		// create build version script
		EditorUtility.DisplayProgressBar("Pre-Build", "Building version script", 0.0f );
		BuildVersionScript( );

		EditorUtility.ClearProgressBar( );
	}

	[MenuItem( "Custom Build/Build Standard" )]
	private static void StandardBuild( )
	{
		string filePath = GetSavePath( );
		if( string.IsNullOrEmpty( filePath ) ) {
			return;
		}

		PreBuildSteps( );
		BuildPipeline.BuildPlayer( levels, filePath, target, BuildOptions.None );
	}

	[MenuItem( "Custom Build/Build Profile" )]
	private static void ProfileBuild( )
	{
		string filePath = GetSavePath( );
		if( string.IsNullOrEmpty( filePath ) ) {
			return;
		}

		PreBuildSteps( );
		BuildPipeline.BuildPlayer( levels, filePath, target,
		                          BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler | BuildOptions.Development );
	}
}
