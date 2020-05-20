using System;
using System.Collections;
using System.Collections.Generic;
using XLua;

namespace Extend.Common.Lua {
	public interface ILuaTable : IDisposable {
		void Get<TKey, TValue>(TKey key, out TValue value);
		bool ContainsKey<TKey>(TKey key);
		void Set<TKey, TValue>(TKey key, TValue value);
		T GetInPath<T>(string path);
		void SetInPath<T>(string path, T val);
		void ForEach<TKey, TValue>(Action<TKey, TValue> action);
		int Length { get; }
		IEnumerable GetKeys();
		IEnumerable<T> GetKeys<T>();
		TValue Get<TKey, TValue>(TKey key);
		TValue Get<TValue>(string key);
		void SetMetaTable(LuaTable metaTable);
		T Cast<T>();
	}
}