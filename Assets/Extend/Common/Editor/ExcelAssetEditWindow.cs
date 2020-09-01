using System;
using System.Collections.Generic;
using ListView;
using NPOI.SS.UserModel;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extend.Common.Editor {
	public class ExcelRowItem : TreeViewItem {
		public IRow Row { get; }

		public ExcelRowItem(IRow row) : base(row.GetHashCode()) {
			Row = row;
		}

		public override string displayName { get => Row.ToString(); set {} }
	}
	
	public class ExcelSheetDelegate : IListViewDelegate<ExcelRowItem> {
		private readonly ISheet m_sheet;
		private readonly string[] m_showColumnNames;
		private readonly int[] m_columnIndex;

		public ExcelSheetDelegate(ISheet sheet, string[] showColumnNames) {
			m_sheet = sheet;
			m_showColumnNames = showColumnNames;
			m_columnIndex = new int[showColumnNames.Length];

			var nameRow = sheet.GetRow(0);
			for( var i = 0; i < showColumnNames.Length; i++ ) {
				for( var j = 0; j < nameRow.Cells.Count; j++ ) {
					if( nameRow.Cells[j].StringCellValue == showColumnNames[i] ) {
						m_columnIndex[i] = j;
						break;
					}
				}
			}
		}

		public MultiColumnHeader Header {
			get {
				var columns = new MultiColumnHeaderState.Column[m_showColumnNames.Length];
				for( var i = 0; i < m_showColumnNames.Length; i++ ) {
					var content = new GUIContent(m_showColumnNames[i]);
					columns[i] = new MultiColumnHeaderState.Column() {
						headerContent = content,
						width = GUI.skin.label.CalcSize(content).x + 30
					};
				}
				return new MultiColumnHeader(new MultiColumnHeaderState(columns));
			}
		} 

		public List<TreeViewItem> GetData() {
			var items = new List<TreeViewItem>();
			for( var i = 3; i <= m_sheet.LastRowNum; i++ ) {
				var row = m_sheet.GetRow(i);
				if(row == null)
					break;
				items.Add(new ExcelRowItem(row));
			}

			return items;
		}

		public List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending) {
			var items = GetData();
			items.Sort((a, b) => (isAscending ? -1 : 1) * string.Compare(a.displayName, b.displayName, StringComparison.Ordinal));
			return items;
		}

		public void Draw(Rect rect, int columnIndex, ExcelRowItem row, bool selected) {
			var typeRow = m_sheet.GetRow(1);
			var colIndex = m_columnIndex[columnIndex];
			var type = typeRow.GetCell(colIndex).StringCellValue;
			var cell = row.Row.GetCell(colIndex);
			if( type == "asset" ) {
				var asset = cell == null ? null : AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(cell.StringCellValue));
				var newAsset = EditorGUI.ObjectField(rect, asset, typeof(Object), false);
				if( newAsset != asset ) {
					if( cell == null ) {
						cell = row.Row.CreateCell(colIndex);
					}
					cell.SetCellValue(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newAsset)));
				}
			}
			else {
				EditorGUI.LabelField(rect, cell == null ? "" : cell.ToString());
			}
		}

		public void OnItemClick(int id) {
		}

		public void OnContextClick() {
		}
	}

	public class ExcelAssetEditWindow : EditorWindow {
		private ListView<ExcelRowItem> m_listview;
		private ExcelFileList.ExcelFile m_file;
		public void Init(ISheet sheet, string[] displayColumnNames, ExcelFileList.ExcelFile excelFile) {
			var _delegate = new ExcelSheetDelegate(sheet, displayColumnNames);
			m_listview = new ListView<ExcelRowItem>(_delegate);
			m_listview.Refresh();
			m_file = excelFile;
		}

		private void OnGUI() {
			var controlRect = EditorGUILayout.GetControlRect(
				GUILayout.ExpandHeight(true),
				GUILayout.ExpandWidth(true));
			m_listview?.OnGUI(controlRect);
		}

		private void OnDisable() {
			m_file.Save();
		}
	}
}