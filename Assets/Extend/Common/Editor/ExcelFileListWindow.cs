using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Extend.Common.Editor {
	public class ExcelFileListWindow : EditorWindow {
		[MenuItem("Tools/Excel Asset Tool")]
		private static void ShowWindow() {
			var window = GetWindow<ExcelFileListWindow>();
			window.titleContent = new GUIContent("Excel Asset Tool");
			window.Show();
		}

		private ReorderableList m_reList;
		private SerializedObject m_excelFileObject;
		private int m_selectIndex = -1;
		private const string ASSET_PATH = "Assets/Editor/ExcelFileList.asset";
		private int selectedHeight;
		private readonly int _controlHint = typeof(ExcelFileList).GetHashCode();

		private class MenuContext {
			public SerializedProperty PreviewSelectionsProp;
			public string Name;
		}

		public void Indent(ref Rect rect) {
			rect.xMin = rect.xMax + 5;
		}

		private void DrawPreviewSelectionPopup(Rect rect, SerializedProperty excelFileProp, IEnumerable<string> names) {
			var previewColumnNamesProp = excelFileProp.FindPropertyRelative("PreviewColumnNames");
			var previewElements = new string[previewColumnNamesProp.arraySize];
			for( var i = 0; i < previewColumnNamesProp.arraySize; i++ ) {
				var ele = previewColumnNamesProp.GetArrayElementAtIndex(i);
				previewElements[i] = ele.stringValue;
			}

			var triggerDropDown = false;
			var controlID = GUIUtility.GetControlID(_controlHint, FocusType.Keyboard, position);
			switch( Event.current.GetTypeForControl(controlID) ) {
				case EventType.MouseDown:
					if( GUI.enabled && rect.Contains(Event.current.mousePosition) ) {
						GUIUtility.keyboardControl = controlID;
						triggerDropDown = true;
						Event.current.Use();
					}

					break;
				case EventType.KeyDown:
					if( GUI.enabled && GUIUtility.keyboardControl == controlID ) {
						if( Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Space ) {
							triggerDropDown = true;
							Event.current.Use();
						}
					}

					break;
				case EventType.Repaint:
					var content = new GUIContent(string.Join("|", previewElements));
					EditorStyles.popup.Draw(rect, content, controlID);
					break;
			}

			if( triggerDropDown ) {
				var menu = new GenericMenu();
				List<string> selectedNames = new List<string>();
				for( var i = 0; i < previewColumnNamesProp.arraySize; i++ ) {
					var ele = previewColumnNamesProp.GetArrayElementAtIndex(i);
					selectedNames.Add(ele.stringValue);
				}

				foreach( var n in names ) {
					var content = new GUIContent(n);
					menu.AddItem(content, selectedNames.Contains(n), OnSelectMenuItem, new MenuContext() {
						Name = n,
						PreviewSelectionsProp = previewColumnNamesProp
					});
					menu.DropDown(rect);
				}
			}
		}

		private static void OnSelectMenuItem(object userData) {
			var context = userData as MenuContext;
			var size = context.PreviewSelectionsProp.arraySize;
			for( var i = 0; i < size; i++ ) {
				var ele = context.PreviewSelectionsProp.GetArrayElementAtIndex(i);
				if( ele.stringValue == context.Name ) {
					context.PreviewSelectionsProp.DeleteArrayElementAtIndex(i);
					return;
				}
			}

			context.PreviewSelectionsProp.InsertArrayElementAtIndex(size);
			var insertEle = context.PreviewSelectionsProp.GetArrayElementAtIndex(size);
			insertEle.stringValue = context.Name;

			context.PreviewSelectionsProp.serializedObject.ApplyModifiedProperties();
		}

		private void OnEnable() {
			var excelFileListData = AssetDatabase.LoadAssetAtPath<ExcelFileList>(ASSET_PATH);
			if( !excelFileListData ) {
				excelFileListData = CreateInstance<ExcelFileList>();
				AssetDatabase.CreateAsset(excelFileListData, ASSET_PATH);
			}

			m_excelFileObject = new SerializedObject(excelFileListData);
			var arrayProp = m_excelFileObject.FindProperty("XlsxFiles");
			m_reList = new ReorderableList(m_excelFileObject, arrayProp);
			m_reList.elementHeightCallback += index => {
				if( index == m_selectIndex ) {
					return ( EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing ) * selectedHeight;
				}

				return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			};
			m_reList.drawElementCallback += (rect, index, active, focused) => {
				var lineStart = rect.xMin;
				rect.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				rect.xMax -= 85;
				var element = arrayProp.GetArrayElementAtIndex(index);
				var excelFileProp = element.FindPropertyRelative("Filename");
				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField(rect, excelFileProp);

				rect.xMin = rect.xMax + 5;
				rect.xMax = rect.xMin + 80;
				if( GUI.Button(rect, "Show Detail") ) {
					m_selectIndex = index;
				}

				selectedHeight = 1;
				if( m_selectIndex == index ) {
					rect.xMin = lineStart;
					var xlsxFile = excelFileListData.XlsxFiles[index];
					var workBook = xlsxFile.Xlsx;
					for( var i = 0; i < workBook.NumberOfSheets; i++ ) {
						rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
						var sheet = workBook.GetSheetAt(i);
						if( sheet == null )
							break;

						var size = GUI.skin.label.CalcSize(xlsxFile.SheetNameContent[i]);
						rect.width = size.x;
						EditorGUI.LabelField(rect, sheet.SheetName);
						Indent(ref rect);

						rect.width = 120;
						var sheetColumnsProp = element.FindPropertyRelative("SheetColumns");
						DrawPreviewSelectionPopup(rect, sheetColumnsProp.GetArrayElementAtIndex(i), xlsxFile.SheetColumns[i].WholeColumnNames);

						Indent(ref rect);
						rect.width = 120;
						if( GUI.Button(rect, "Open Asset Edit") ) {
							var win = CreateWindow<ExcelAssetEditWindow>();
							win.Init(sheet, xlsxFile.SheetColumns[i].PreviewColumnNames, xlsxFile);
							win.Show();
						}
						selectedHeight++;
					}
				}

				if( EditorGUI.EndChangeCheck() ) {
					m_excelFileObject.ApplyModifiedProperties();
				}
			};
		}

		private void OnGUI() {
			var rect = new Rect(5, 5, position.width - 10, position.height - 10);
			m_reList.DoList(rect);
		}

		private void OnDestroy() {
			var excelFileListData = AssetDatabase.LoadAssetAtPath<ExcelFileList>(ASSET_PATH);
			foreach( var file in excelFileListData.XlsxFiles ) {
				file.Reload();
			}
		}
	}
}