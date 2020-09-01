using System;
using System.Collections.Generic;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEngine;

namespace Extend.Common.Editor {
	public class ExcelFileList : ScriptableObject {
		[Serializable]
		public class SheetColumn {
			[NonSerialized]
			public List<string> WholeColumnNames = new List<string>();

			public string SheetName;
			public string[] PreviewColumnNames;
		}

		[Serializable]
		public class ExcelFile {
			public string Filename;
			public List<SheetColumn> SheetColumns;
			public GUIContent[] SheetNameContent { get; private set; }

			private IWorkbook m_workBook;

			public IWorkbook Xlsx {
				get {
					if( m_workBook != null )
						return m_workBook;
					if( !File.Exists(Filename) )
						return null;

					using( var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read) ) {
						m_workBook = new XSSFWorkbook(stream);
						SheetNameContent = new GUIContent[m_workBook.NumberOfSheets];
						for( var i = 0; i < m_workBook.NumberOfSheets; i++ ) {
							var sheet = m_workBook.GetSheetAt(i);
							if( sheet.SheetName.StartsWith("ignore") ) {
								continue;
							}

							SheetNameContent[i] = new GUIContent(sheet.SheetName);
							for( var sheetIndex = 0; sheetIndex < SheetColumns.Count; ) {
								var sheetColumn = SheetColumns[sheetIndex];
								if( m_workBook.GetSheet(sheetColumn.SheetName) == null ) {
									SheetColumns.RemoveAt(sheetIndex);
								}
								else {
									sheetIndex++;
								}
							}

							var find = SheetColumns.Find(sheetCol => sheetCol.SheetName == sheet.SheetName);
							if( find == null ) {
								SheetColumns.Add(new SheetColumn() {
									SheetName = sheet.SheetName
								});
							}

							foreach( var sheetColumn in SheetColumns ) {
								var row = sheet.GetRow(0);
								foreach( var val in row ) {
									var name = val.ToString();
									if( name.StartsWith("ignore", StringComparison.InvariantCulture) ) {
										continue;
									}

									sheetColumn.WholeColumnNames.Add(name);
								}
							}
						}

						return m_workBook;
					}
				}
				set => m_workBook = value;
			}

			public void Save() {
				var filename = Path.GetTempFileName();
				using( var stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write) ) {
					m_workBook.Write(stream);
				}

				File.Delete(Filename);
				File.Move(filename, Filename);
			}

			public void Reload() {
				Xlsx = null;
			}
		}

		public ExcelFile[] XlsxFiles;
	}
}