using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ListView {
	public interface IListViewDelegate<in T> where T : TreeViewItem {
		MultiColumnHeader Header { get; }
		List<TreeViewItem> GetData();
		List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending);
		void Draw(Rect rect, int columnIndex, T data, bool selected);
		void OnItemClick(int id);
		void OnContextClick();
	}

	public class ListView<T> : TreeView where T : TreeViewItem {
		private const string sortedColumnIndexStateKey = "ListView_sortedColumnIndex";
		public IListViewDelegate<T> viewDelegate;


		public ListView(IListViewDelegate<T> listViewViewDelegate) : this(new TreeViewState(), listViewViewDelegate.Header) {
			viewDelegate = listViewViewDelegate;
		}

		protected ListView(TreeViewState state, MultiColumnHeader header) : base(state, header) {
			rowHeight = 20;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			header.sortingChanged += SortingChanged;

			header.ResizeToFit();
			Reload();

			header.sortedColumnIndex = SessionState.GetInt(sortedColumnIndexStateKey, 1);
		}

		protected override TreeViewItem BuildRoot() {
			var root = new TreeViewItem {depth = -1};
			root.children = new List<TreeViewItem>();
			return root;
		}

		public void Refresh() {
			if( viewDelegate == null ) {
				return;
			}

			rootItem.children = viewDelegate.GetData();
			BuildRows(rootItem);
			Repaint();
		}

		private void SortingChanged(MultiColumnHeader header) {
			SessionState.SetInt(sortedColumnIndexStateKey, multiColumnHeader.sortedColumnIndex);

			if( viewDelegate == null ) {
				rootItem.children = new List<TreeViewItem>();
				BuildRows(rootItem);
				return;
			}

			var index = multiColumnHeader.sortedColumnIndex;
			var ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);

			rootItem.children = viewDelegate.GetSortedData(index, ascending);
			BuildRows(rootItem);
		}

		protected override bool CanMultiSelect(TreeViewItem item) {
			return false;
		}

		protected override void ContextClicked() {
			viewDelegate?.OnContextClick();
			base.ContextClicked();
		}

		protected override void SingleClickedItem(int id) {
			viewDelegate?.OnItemClick(id);
			base.SingleClickedItem(id);
		}

		protected override void RowGUI(RowGUIArgs args) {
			var item = args.item as T;

			for( var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++ ) {
				var rect = args.GetCellRect(visibleColumnIndex);
				var columnIndex = args.GetColumn(visibleColumnIndex);

				viewDelegate.Draw(rect, columnIndex, item, args.selected);
			}
		}
	}
}