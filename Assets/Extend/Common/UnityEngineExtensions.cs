using System;
using UnityEngine;
 
public static class UnityEngineExtensions
{
	/// <summary>
	/// Returns the component of Type type. If one doesn't already exist on the GameObject it will be added.
	/// </summary>
	/// <typeparam name="T">The type of Component to return.</typeparam>
	/// <param name="gameObject">The GameObject this Component is attached to.</param>
	/// <returns>Component</returns>
	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
	}
	
	public static T GetOrAddComponent<T>(this Component component) where T : Component
	{
		return component.GetComponent<T>() ?? component.gameObject.AddComponent<T>();
	}
}