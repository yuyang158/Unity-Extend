using System.Collections.Generic;
using System.IO;
using System.Linq;
using XLua;

namespace Extend.Common {
	[LuaCallCSharp]
	public class IniRead {
		private class Section {
			public string Name { get; }

			public Dictionary<string, string> KeyValue { get; } = new Dictionary<string, string>();

			public Section(string n) {
				Name = n;
			}
		}

		private readonly List<Section> iniSections = new List<Section>();
		[BlackList]
		public static IniRead Parse(TextReader reader) {
			var ini_reader = new IniRead();
			string line;
			while( ( line = reader.ReadLine() ) != null ) {
				line = TrimComment(line);
				if( line.Length == 0 ) continue;
				if( line.StartsWith("[") && line.Contains("]") ) {
					var index = line.IndexOf(']');
					var name = line.Substring(1, index - 1).Trim();
					var foundSection = ini_reader.iniSections.Find(x => x.Name == name);
					if( foundSection == null )
						ini_reader.iniSections.Add(new Section(name));
					else
						continue;
				}

				if( line.Contains("=") ) {
					var index = line.IndexOf('=');
					var key = line.Substring(0, index).Trim();
					var value = line.Substring(index + 1).Trim();
					ini_reader.iniSections.Last().KeyValue.Add(key, value);
				}
			}

			return ini_reader;
		}

		private static string TrimComment(string s) {
			if( s.Contains(";") ) {
				var index = s.IndexOf(';');
				s = s.Substring(0, index).Trim();
			}

			return s;
		}

		private bool FindOne(string section, string key, out string val) {
			var s = iniSections.Find(x => x.Name == section);
			if( s == null ) {
				val = string.Empty;
				return false;
			}
			return s.KeyValue.TryGetValue(key, out val);
		}

		public int GetInt(string section, string key) {
			return FindOne(section, key, out var val) ? int.Parse(val) : default;
		}

		public bool GetBool(string section, string key) {
			return FindOne(section, key, out var val) && (val == "true" || val == "false");
		}

		public string GetString(string section, string key) {
			FindOne(section, key, out var val);
			return val;
		}

		public double GetDouble(string section, string key) {
			return FindOne(section, key, out var val) ? double.Parse(val) : default;
		}

		private IniRead() {
		}
		
		public static IniRead SystemSetting { get; set; }
	}
}