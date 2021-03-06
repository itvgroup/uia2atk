// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
//
// Authors:
//      Brad Taylor <brad@getcoded.net>
//

using Atk;

using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using AEIds = System.Windows.Automation.AutomationElementIdentifiers;

using Moonlight.AtkBridge.PatternImplementors;

namespace Moonlight.AtkBridge
{
	public class Adapter : Atk.Object, Atk.ComponentImplementor
	{
#region Public Properties
		public AutomationPeer Peer {
			get;
			protected set;
		}

		public double Alpha {
			get { return 1.0d; }
		}

		public new Layer Layer {
			get { return Atk.Layer.Widget; }
		}

		public new int MdiZorder {
			get { return 0; }
		}
#endregion

#region Public Methods
		public Adapter (AutomationPeer peer)
		{
			this.Peer = peer;
		}

		protected Adapter (IntPtr ptr)
			: base (ptr)
		{
		}

		public uint AddFocusHandler (FocusHandler handler)
		{
			if (focusHandlers.ContainsValue (handler))
				return 0;

			lastFocusHandlerId++;
			focusHandlers [lastFocusHandlerId] = handler;
			return lastFocusHandlerId;
		}

		public bool Contains (int x, int y, CoordType coordType)
		{
			if (Peer == null)
				return false;

			// Despite MSDN documentation, this is actually in
			// window coordinates
			Rect r = Peer.GetBoundingRectangle ();

			if (coordType == CoordType.Screen)
				ScreenToWindow (ref x, ref y);

			return r.Contains (new System.Windows.Point (x, y));
		}

		public void GetExtents (out int x, out int y,
		                        out int width, out int height,
		                        CoordType coordType)
		{
			x = y = Int32.MinValue;
			width = height = 0;

			if (Peer == null)
				return;

			// Despite MSDN documentation, this is actually in
			// window coordinates
			Rect r = Peer.GetBoundingRectangle ();

			if (!Peer.IsOffscreen ()) {
				x = (int) r.X;
				y = (int) r.Y;

				if (coordType == CoordType.Screen)
					WindowToScreen (ref x, ref y);
			}

			width = 100;
			height = 100;
			width = (int) r.Width;
			height = (int) r.Height;
		}

		public void GetPosition (out int x, out int y,
		                         CoordType coordType)
		{
			x = y = Int32.MinValue;

			if (Peer == null || Peer.IsOffscreen ())
				return;

			// Despite MSDN documentation, this is actually in
			// window coordinates
			Rect r = Peer.GetBoundingRectangle ();

			x = (int) r.X;
			y = (int) r.Y;

			if (coordType == CoordType.Screen)
				WindowToScreen (ref x, ref y);
		}

		public void GetSize (out int width, out int height)
		{
			width = height = 0;

			if (Peer == null)
				return;

			// Despite MSDN documentation, this is actually in
			// window coordinates
			Rect r = Peer.GetBoundingRectangle ();

			width = (int) r.Width;
			height = (int) r.Height;
		}

		public bool GrabFocus ()
		{
			if (Peer == null)
				return false;

			Peer.SetFocus ();

			return Peer.HasKeyboardFocus ();
		}

		public Atk.Object RefAccessibleAtPoint (int x, int y,
		                                        CoordType coordType)
		{
			CacheChildren ();

			lock (ChildrenLock) {
				if (children != null) {
					foreach (AutomationPeer peer in children) {
						Adapter adapter = DynamicAdapterFactory
							.Instance.GetAdapter (peer);
						if (adapter == null)
							continue;

						if (adapter.Contains (x, y, coordType))
							return adapter;
					}
				}
			}

			return Contains (x, y, coordType) ? this : null;
		}

		public void RemoveFocusHandler (uint handlerId)
		{
			if (focusHandlers.ContainsKey (handlerId))
				focusHandlers.Remove (handlerId);
		}

		public bool SetExtents (int x, int y, int width, int height,
		                        CoordType coordType)
		{
			return false;
		}

		public bool SetPosition (int x, int y, CoordType coordType)
		{
			return false;
		}

		public bool SetSize (int width, int height)
		{
			return false;
		}
#endregion

#region Protected Properties
		protected virtual BasePatternImplementor [] PatternImplementors {
			get { return new BasePatternImplementor [0]; }
		}
#endregion

#region Protected Methods
		protected override string OnGetName ()
		{
			return (Peer != null) ? Peer.GetName ()
			                      : String.Empty;
		}

		protected override Role OnGetRole ()
		{
			if (Peer == null)
				return Role.Unknown;

			if (PatternImplementors.Length > 0
			    && PatternImplementors [0].OverriddenRole != Role.Unknown) {
				if (PatternImplementors.Length > 1)
					Log.Warn ("Adapter has more than one PatternImplementor, we are going to use the first OverridenRole value");
				return PatternImplementors [0].OverriddenRole;
			}

			if (Peer.IsPassword ())
				return Role.PasswordText;

			AutomationControlType type = Peer.GetAutomationControlType ();
			switch (type) {
			case AutomationControlType.Button:
				return Role.PushButton;
			case AutomationControlType.Calendar:
				return Role.Calendar;
			case AutomationControlType.CheckBox:
				return Role.CheckBox;
			case AutomationControlType.ComboBox:
				return Role.ComboBox;
			case AutomationControlType.Edit:
				return Role.Text;
			case AutomationControlType.Hyperlink:
				return Role.PushButton;
			case AutomationControlType.Image:
				return Role.Image;
			case AutomationControlType.ListItem:
				return Role.ListItem;
			case AutomationControlType.List:
				return Role.List;
			case AutomationControlType.Menu:
				return Role.Menu;
			case AutomationControlType.MenuBar:
				return Role.MenuBar;
			case AutomationControlType.MenuItem:
				return Role.MenuItem;
			case AutomationControlType.ProgressBar:
				return Role.ProgressBar;
			case AutomationControlType.RadioButton:
				return Role.RadioButton;
			case AutomationControlType.ScrollBar:
				return Role.ScrollBar;
			case AutomationControlType.Slider:
				return Role.Slider;
			case AutomationControlType.Spinner:
				return Role.SpinButton;
			case AutomationControlType.StatusBar:
				return Role.Statusbar;
			case AutomationControlType.Tab:
				return Role.PageTabList;
			case AutomationControlType.TabItem:
				return Role.PageTab;
			case AutomationControlType.Text:
				return Role.Label;
			case AutomationControlType.ToolBar:
				return Role.ToolBar;
			case AutomationControlType.ToolTip:
				return Role.ToolTip;
			case AutomationControlType.Tree:
				return Role.Table;
			case AutomationControlType.TreeItem:
				return Role.TableCell;
			case AutomationControlType.Custom:
				return Role.Unknown;
			case AutomationControlType.Group:
				return Role.LayeredPane;
			case AutomationControlType.Thumb:
				return Role.PushButton;
			case AutomationControlType.DataGrid:
				return Role.Table;
			case AutomationControlType.DataItem:
				return Role.TableCell;
			case AutomationControlType.Document:
				return Role.Panel;
			case AutomationControlType.SplitButton:
				return Role.PushButton;
			case AutomationControlType.Window:
				return Role.Filler;
			case AutomationControlType.Pane:
				return Role.Panel;
			case AutomationControlType.Header:
				return Role.TableRowHeader;
			case AutomationControlType.HeaderItem:
				return Role.TableCell;
			case AutomationControlType.Table:
				return Role.Table;
			case AutomationControlType.TitleBar:
				return Role.MenuBar;
			case AutomationControlType.Separator:
				return Role.Separator;
			default:
				return Role.Unknown;
			}
		}

		protected override Atk.StateSet OnRefStateSet ()
		{
			Atk.StateSet states = base.OnRefStateSet ();
			// Clearing inherited states to remove assumptions in specific
			// interface implementations.
			states.ClearStates ();

			DynamicAdapterFactory.Instance.MarkExternalReference (states);

			if (disposed) {
				states.AddState (Atk.StateType.Defunct);
				return states;
			}

			if (Peer == null)
				return states;

			if (!Peer.IsOffscreen ()) {
				states.AddState (Atk.StateType.Showing);
				states.AddState (Atk.StateType.Visible);
			} else {
				states.RemoveState (Atk.StateType.Showing);
				states.RemoveState (Atk.StateType.Visible);
			}

			if (Peer.IsEnabled ()) {
				states.AddState (Atk.StateType.Sensitive);
				states.AddState (Atk.StateType.Enabled);
			} else {
				states.RemoveState (Atk.StateType.Sensitive);
				states.RemoveState (Atk.StateType.Enabled);
			}

			if (Peer.IsKeyboardFocusable ())
				states.AddState (Atk.StateType.Focusable);
			else
				states.RemoveState (Atk.StateType.Focusable);

			if (Peer.HasKeyboardFocus ())
				states.AddState (Atk.StateType.Focused);
			else
				states.RemoveState (Atk.StateType.Focused);

			var orientation = Peer.GetOrientation ();
			if (orientation == AutomationOrientation.Horizontal)
				states.AddState (Atk.StateType.Horizontal);
			else
				states.RemoveState (Atk.StateType.Horizontal);

			if (orientation == AutomationOrientation.Vertical)
				states.AddState (Atk.StateType.Vertical);
			else
				states.RemoveState (Atk.StateType.Vertical);

			foreach (BasePatternImplementor impl in PatternImplementors)
				impl.OnRefStateSet (ref states);

			return states;
		}

		protected override int OnGetNChildren ()
		{
			CacheChildren ();

			lock (ChildrenLock) {
				return (children != null) ? children.Count : 0;
			}
		}

		protected override Atk.Object OnRefChild (int i)
		{
			CacheChildren ();

			lock (ChildrenLock) {
				if (children == null || i < 0 || i >= children.Count)
					return null;

				AutomationPeer child = children[i];
				if (child == null)
					return null;

				return DynamicAdapterFactory
					.Instance.GetAdapter (child);
			}
		}

		protected override Atk.RelationSet OnRefRelationSet ()
		{
			var relationSet = base.OnRefRelationSet ();
			DynamicAdapterFactory.Instance.MarkExternalReference (relationSet);
			return relationSet;
		}

		protected override Atk.Object OnGetParent ()
		{
			if (Peer == null)
				return null;

			AutomationPeer parent = Peer.GetParent ();

			// XXX: This is a huge hack.
			// ScrollViewer is implemented so that its children
			// appear as the children of its parent.  This gives us
			// a bit of a headache as the ScrollViewer is never
			// available in the Atk hierarchy, and an adapter is
			// never created for it.  Thus we pretend it doesn't
			// exist.
			if (parent is ScrollViewerAutomationPeer
			    && Peer is ItemAutomationPeer)
				parent = parent.GetParent ();

			if (parent == null)
				return DynamicAdapterFactory.Instance.RootVisualAdapter;

			return DynamicAdapterFactory.Instance.GetAdapter (parent);
		}

		protected override int OnGetIndexInParent ()
		{
			Adapter parent = Parent as Adapter;
			if (parent == null)
				return -1;

			return parent.GetIndexOfChild (this);
		}

		protected virtual void CacheChildren ()
		{
			lock (ChildrenLock) {
				if (Peer == null || children != null)
					return;

				children = GetPatternImplementorChildren ();
			}
		}

		private List<AutomationPeer> GetPatternImplementorChildren ()
		{
			List<AutomationPeer> patternChildren = new List<AutomationPeer> ();
			bool childrenOverriden = false;

			foreach (BasePatternImplementor impl in PatternImplementors) {
				if (impl.OverridesGetChildren) {
					childrenOverriden = true;
					List<AutomationPeer> implChildren = impl.GetChildren ();
					if (implChildren != null)
						patternChildren.AddRange (implChildren);
				}
			}

			return childrenOverriden ? patternChildren : Peer.GetChildren ();
		}

		protected void NotifyFocused (bool focused)
		{
			NotifyStateChange (Atk.StateType.Focused, focused);

			if (focused)
				Atk.Focus.TrackerNotify (this);
		}

		protected void RemoveChild (AutomationPeer peer)
		{
			Adapter adapter = DynamicAdapterFactory.Instance.GetAdapter (peer, false);
			if (adapter == null) {
				children.Remove (peer);
				return;
			}

			int index = children.IndexOf (peer);
			if (index < 0)
				return;

			EmitChildrenChanged (ChildrenChangedDetail.Remove,
					     (uint) index, adapter);
			children.Remove (peer);

			DynamicAdapterFactory.Instance.UnregisterAdapter (peer);
		}

		protected void AddChild (AutomationPeer peer)
		{
			Adapter adapter = DynamicAdapterFactory.Instance.GetAdapter (peer);
			if (adapter == null)
				return;

			EmitChildrenChanged (ChildrenChangedDetail.Add,
			                     (uint) children.IndexOf (peer),
			                     adapter);
		}
#endregion

#region Protected Fields
		protected List<AutomationPeer> children = null;
		protected object ChildrenLock = new object ();
#endregion

#region Internal Events
		internal event EventHandler<AutomationPropertyChangedEventArgs> AutomationPropertyChanged;
		internal event EventHandler<AutomationEventEventArgs> AutomationEventRaised;
#endregion

#region Internal Methods
		// Depth-first search the current adapter's children
		internal void Foreach (Action<Adapter> func, bool create)
		{
			lock (ChildrenLock) {
				if (children != null) {
					foreach (AutomationPeer peer in children) {
						var child = DynamicAdapterFactory
							.Instance.GetAdapter (peer, create);
						if (child == null)
							continue;

						child.Foreach (func, create);
					}
				}
			}

			func (this);
		}

		internal int GetIndexOfChild (Adapter child)
		{
			CacheChildren ();

			if (children == null || child == null)
				return -1;

			// Intentionally use Children list instead of
			// Peer.GetChildren () as we're concerned with what is
			// currently displayed to the user
			lock (ChildrenLock) {
				return children.IndexOf (child.Peer);
			}
		}

		internal void HandleAutomationPropertyChanged (AutomationPropertyChangedEventArgs args)
		{
			if (args.Property == AEIds.HasKeyboardFocusProperty) {
				bool focused = (bool) args.NewValue;
				NotifyFocused (focused);

				foreach (FocusHandler handler in focusHandlers.Values)
					handler (this, focused);
			} else if (args.Property == AEIds.IsOffscreenProperty) {
				bool offscreen = (bool) args.NewValue;
				NotifyStateChange (Atk.StateType.Visible, !offscreen);
			} else if (args.Property == AEIds.IsEnabledProperty) {
				bool enabled = (bool) args.NewValue;
				NotifyStateChange (Atk.StateType.Enabled, enabled);
				NotifyStateChange (Atk.StateType.Sensitive, enabled);
			} else if (args.Property == AEIds.HelpTextProperty) {
				Description = (string) args.NewValue;
			} else if (args.Property == AEIds.BoundingRectangleProperty) {
				EmitBoundsChanged ((System.Windows.Rect) args.NewValue);
			} else if (args.Property == AEIds.NameProperty) {
				Notify ("accessible-name");
			} else if (args.Property == AEIds.ControlTypeProperty) {
				// Assume that the RootVisual will never change
				// its control type.
				Adapter parent = Parent as Adapter;
				if (parent != null)
					parent.HandleControlTypeChange (Peer);
			}

			if (AutomationPropertyChanged != null)
				AutomationPropertyChanged (args.Peer, args);
		}

		internal void HandleAutomationEventRaised (AutomationEventEventArgs args)
		{
			if (args.Event == AutomationEvents.StructureChanged)
				HandleStructureChanged ();

			if (AutomationEventRaised != null)
				AutomationEventRaised (args.Peer, args);
		}

		internal void HandleStructureChanged ()
		{
			lock (ChildrenLock) {
				var new_children = GetPatternImplementorChildren ();

				if (children != null) {
					var removed = (new_children != null) ? children.Except (new_children)
									     : children;

					// Remove children that aren't
					// in the new list
					while (removed != null && removed.Count () > 0)
						RemoveChild (removed.ElementAt (0));
				}

				if (new_children != null) {
					// In an ideal world, we
					// wouldn't actually add
					// children (they would be lazy
					// loaded), but we need to send
					// events, so blah.
					//

					var added = (children != null) ? new_children.Except (children)
								       : new_children;

					// Make sure we set Children
					// correctly before we start
					// sending events so that
					// listeners will see us in the
					// correct state
					children = new_children;

					// Add children that we haven't
					// seen before
					foreach (AutomationPeer peer in added)
						AddChild (peer);
				} else {
					children = new_children;
				}
			}
		}

		internal void HandleControlTypeChange (AutomationPeer peer)
		{
			var adapter = DynamicAdapterFactory.Instance.GetAdapter (peer, false);
			if (adapter == null || children == null) {
				// If we've never created the adapter, we don't
				// need to notify anyone that it's going away
				// or recreate it.
				return;
			}

			lock (ChildrenLock) {
				int index = children.IndexOf (peer);
				if (index < 0)
					return;

				// Unregister and dispose the adapter's
				// children so that they don't become orphans
				adapter.Foreach (child => {
					DynamicAdapterFactory.Instance.UnregisterAdapter (child.Peer);
				}, false);

				// Invalidate the cached Adapter
				EmitChildrenChanged (ChildrenChangedDetail.Remove,
						     (uint) index, adapter);
				adapter.NotifyStateChange (Atk.StateType.Defunct, true);
				DynamicAdapterFactory.Instance.UnregisterAdapter (peer);

				// Create a new adapter to reflect the current ControlType
				adapter = DynamicAdapterFactory.Instance.GetAdapter (peer);
				EmitChildrenChanged (ChildrenChangedDetail.Add,
						     (uint) index, adapter);
			}
		}

		internal void WindowToScreen (ref int x, ref int y)
		{
			ConvertCoords (ref x, ref y, true);
		}

		internal void ScreenToWindow (ref int x, ref int y)
		{
			ConvertCoords (ref x, ref y, false);
		}

		internal void NotifyStateChange (Atk.StateType state)
		{
			NotifyStateChange (state, RefStateSet ().ContainsState (state));
		}

		internal void EmitSignal (string signal)
		{
			GLib.Signal.Emit (this, signal);
		}

		internal void NotifyPropertyChanged (string property)
		{
			Notify (property);
		}

		// Meant to be used by BasePatternImplementor subclasses
		internal List<AutomationPeer> GetChildren ()
		{
			CacheChildren ();

			lock (ChildrenLock) {
				return children;
			}
		}
#endregion

#region Private Methods
		private void ConvertCoords (ref int x, ref int y, bool addParent)
		{
			Adapter rootVisual
				= DynamicAdapterFactory.Instance.RootVisualAdapter;
			if (rootVisual == null || rootVisual.Parent == null) {
				// TODO: Logging
				return;
			}

			// Since our parent is unmanaged, we can't just cast.
			Atk.Component parent = ComponentAdapter.GetObject (rootVisual.Parent);
			if (parent == null) {
				// TODO: Logging
				return;
			}

			int parentX, parentY;
			parent.GetPosition (out parentX, out parentY, CoordType.Screen);

			x += parentX * (addParent ? 1 : -1);
			y += parentY * (addParent ? 1 : -1);
		}

		private void EmitBoundsChanged (System.Windows.Rect rect)
		{
			Atk.Rectangle atkRect;
			atkRect.X = (int) rect.X;
			atkRect.Y = (int) rect.Y;
			atkRect.Width = (int) rect.Width;
			atkRect.Height = (int) rect.Height;
			GLib.Signal.Emit (this, "bounds_changed", atkRect);
		}
#endregion

#region Private Fields
		private uint lastFocusHandlerId = 0;
		private Dictionary<uint, FocusHandler> focusHandlers
			= new Dictionary<uint, FocusHandler> ();
#endregion
	}
}
