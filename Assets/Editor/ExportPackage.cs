using UnityEditor;

namespace Mobcast.Coffee.Button
{
	public static class ExportPackage
	{
		const string kPackageName = "ButtonEx.unitypackage";
		static readonly string[] kAssetPathes = {
			"Assets/Mobcast/Coffee/ButtonEx",
		};

		[MenuItem ("Export Package/" + kPackageName)]
		[InitializeOnLoadMethod]
		static void Export ()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;
			
			AssetDatabase.ExportPackage (kAssetPathes, kPackageName, ExportPackageOptions.Recurse | ExportPackageOptions.Default);
			UnityEngine.Debug.Log ("Export successfully : " + kPackageName);
		}
	}
}