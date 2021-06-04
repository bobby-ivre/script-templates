// --------------------------------------------------------------------------------------------------------------------
// Creation Date:	06/02/21
// Author:				bgreaney
// Description:		Sets the project up to use custom script tempaltes, including namespace
//								placement, summary, author and date.
// --------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LughNut.EditorTools
{
	/// <summary>
	/// Sets the project up to use custom script tempaltes, including namespace placement, summary, author and date.
	/// </summary>
	public class CopyScriptTemplates : UnityEditor.AssetModificationProcessor
	{

		public const string ScriptTemplatesSuffix = "/Resources/ScriptTemplates/";
		public const string ScriptTemplatesBackup = "/Resources/ScriptTemplates_backup/";


		[MenuItem("Assets/Setup Script Templates", priority = 10000)]
		static void SetScriptTemplates()
        {
			if (EditorUtility.DisplayDialog("Update Script Templates",
				"Using this utility will delete your existing script templates " +
				"and use the built-in templates. Are you sure?",
				"Ok", "Cancel"))
            {
				CopyTemplateFiles();

				EditorUtility.DisplayDialog("Done",
					"Script Templates reloaded. Please restart the unity editor to use the new templates." +
					" Old templates have been placed into ScriptTemplates_backup.",
					"Ok.");
            }
        }


		static void CopyTemplateFiles()
		{
			string scriptTemplatesPath = EditorApplication.applicationContentsPath
				+ ScriptTemplatesSuffix;
			//string[] existingTemplates = Directory.GetFiles(scriptTemplatesPath);

			if (Directory.Exists(EditorApplication.applicationContentsPath
				+ ScriptTemplatesBackup) == false)
			{
				Directory.CreateDirectory(EditorApplication.applicationContentsPath
					+ ScriptTemplatesBackup);
			}
			string[] oldFiles = Directory.GetFiles(scriptTemplatesPath);
			for (int i = 0; i < oldFiles.Length; i++)
			{
				string newPath = oldFiles[i];
				newPath = newPath.Replace("/ScriptTemplates/", "/ScriptTemplates_backup/");
				File.Copy(oldFiles[i], newPath, true);
			}

			Directory.Delete(scriptTemplatesPath, true);
			Directory.CreateDirectory(scriptTemplatesPath);

			TextAsset[] ta = Resources.LoadAll<TextAsset>("ScriptTemplates/");
			for (int i = 0; i < ta.Length; i++)
			{
				using (StreamWriter stream = File.CreateText(scriptTemplatesPath + ta[i].name + ".txt"))
				{
					stream.Write(ta[i].text);
				}
			}
			Debug.Log("Copied Script Templates");
		}

		public static void OnWillCreateAsset(string path)
		{
			path = path.Replace(".meta", "");
			int index = path.LastIndexOf(".");
			if (index < 0)
				return;


			string file = path.Substring(index);
			if (file != ".cs" && file != ".js" && file != ".boo")
				return;


			index = Application.dataPath.LastIndexOf("Assets");
			path = Application.dataPath.Substring(0, index) + path;
			if (!System.IO.File.Exists(path))
				return;

			string fileContent = System.IO.File.ReadAllText(path);
			fileContent = fileContent.Replace("#CREATIONDATE#", System.DateTime.Today.ToString("dd/MM/yy") + "");
			fileContent = fileContent.Replace("#DEVELOPER#", System.Environment.UserName);
			if (EditorPrefs.HasKey("defaultNamespace"))
				fileContent = fileContent.Replace("REPLACE", EditorPrefs.GetString("defaultNamespace"));
			File.WriteAllText(path, fileContent);
			AssetDatabase.Refresh();
		}
	}

	public class DefaultNamespaceSetter : EditorWindow
	{

		static string defaultNamespace = "REPLACE";
		static string newNamespace;
		[MenuItem("Assets/Set Default Namespace", priority = 10001)]
		static void Init()
        {
			if (EditorPrefs.HasKey("defaultNamespace"))
				defaultNamespace = EditorPrefs.GetString("defaultNamespace");
			newNamespace = defaultNamespace;
			DefaultNamespaceSetter window = ScriptableObject.CreateInstance<DefaultNamespaceSetter>();
			window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
			window.ShowPopup();
		}
		void OnGUI()
		{
			EditorGUILayout.LabelField("Set Default Namespace", EditorStyles.wordWrappedLabel);
			GUILayout.Space(70);
			newNamespace = GUILayout.TextField(newNamespace);
			if (GUILayout.Button("Agree!"))
			{
				defaultNamespace = newNamespace;
				EditorPrefs.SetString("defaultNamespace", defaultNamespace);
				this.Close();
			}
		}
	}
}
#endif
