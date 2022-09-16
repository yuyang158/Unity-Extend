using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Extend.Editor
{
	struct AnimationArray
	{
		public string[] essential;
		public string[] others;
	}

	public static class AvatarEditorExtend
	{
		[MenuItem("Tools/Avatar/获取动画列表")]
		public static void GetAvatarAnimationList()
		{
			string dirPath = Path.Combine(Application.dataPath, "Res/Avatar/Config/AnimationClip.json");
			string essentialFilePath = Path.Combine(Application.dataPath, "Res/Avatar/Model-Doll01/Release/Animation/Required");
			string otherFilePath = Path.Combine(Application.dataPath, "Res/Avatar/Model-Doll01/Release/Animation/Others");

			AnimationArray animationArray = new AnimationArray();
			List<string> EssentialList = new List<string>();
			List<string> OtherlList = new List<string>();
			GetFilesName(essentialFilePath, "", EssentialList);
			GetFilesName(otherFilePath, "", OtherlList);
			animationArray.essential = EssentialList.ToArray();
			animationArray.others = OtherlList.ToArray();
			string json = JsonUtility.ToJson(animationArray);
			byte[] byteArr = System.Text.Encoding.Default.GetBytes(json);
			using (FileStream fs = new FileStream(dirPath, FileMode.Create))
			{
				fs.Write(byteArr, 0, byteArr.Length);
			}
			AssetDatabase.Refresh();
			Debug.Log("获取动画列表成功");
		}

		private static void GetFilesName(string filePath, string resPath, List<string> list)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
			var directroies = directoryInfo.GetDirectories();
			if (directroies.Length > 0)
			{
				foreach (var directroy in directroies)
				{
					string res = resPath +"/" + directroy.Name;
					GetFilesName(directroy.FullName, res, list);
				}
			}
			string[] files = Directory.GetFiles(filePath).Where(s => !s.EndsWith(".meta")).ToArray();
			string[] filesname = new string[files.Length];
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fi = new FileInfo(files[i]);
				filesname[i] = resPath + "/" + fi.Name;
			}

			list.AddRange(filesname);
		}
	}
}