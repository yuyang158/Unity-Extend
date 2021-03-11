using System;
using System.Collections.Generic;
using UnityEditor;

namespace Extend.Common.Editor {
	[InitializeOnLoad]
	public static class EditorMainThreadDispatcher {
		private static readonly Queue<Action> m_mainThreadAction = new Queue<Action>();
		
		static EditorMainThreadDispatcher() {
			EditorApplication.update += () => {
				lock( m_mainThreadAction ) {
					if( m_mainThreadAction.Count > 0 ) {
						var action = m_mainThreadAction.Dequeue();
						action.Invoke();
					}
				}
			};
		}

		public static void PushAction(Action action) {
			lock( m_mainThreadAction ) {
				m_mainThreadAction.Enqueue(action);
			}
		}
	}
}