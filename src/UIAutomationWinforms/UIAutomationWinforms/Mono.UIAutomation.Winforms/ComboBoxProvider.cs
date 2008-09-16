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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com) 
// 
// Authors: 
//	Mario Carrion <mcarrion@novell.com>
// 
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Forms;
using System.Windows;
using Mono.UIAutomation.Winforms.Behaviors;
using Mono.UIAutomation.Winforms.Navigation;

namespace Mono.UIAutomation.Winforms
{

	internal class ComboBoxProvider : ListProvider
	{

		#region Constructor
		
		public ComboBoxProvider (ComboBox combobox) : base (combobox)
		{
			comboboxControl = combobox;
			comboboxControl.DropDownStyleChanged += new EventHandler (OnDropDownStyleChanged);
		}
		
		#endregion
		
		#region SimpleControlProvider: Specializations

		public override object GetPropertyValue (int propertyId)
		{
			if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id)
				return ControlType.ComboBox.Id;
			else if (propertyId == AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id)
				return true;
			else if (propertyId == AutomationElementIdentifiers.LocalizedControlTypeProperty.Id)
				return "combo box";
			else
				return base.GetPropertyValue (propertyId);
		}
		
		public override void Terminate ()
		{
			base.Terminate ();
			
			comboboxControl.DropDownStyleChanged -= new EventHandler (OnDropDownStyleChanged);
		}	

		#endregion		

		#region FragmentRootControlProvider: Specializations
		
		public override IRawElementProviderFragment ElementProviderFromPoint (double x, double y)
		{
			throw new NotImplementedException ();
		}

		#endregion
		
		#region ListProvider: Specializations
		
		public override bool SupportsMultipleSelection { 
			get { return false; } 
		}
		
		public override int SelectedItemsCount { 
			get { return ListControl.SelectedIndex == -1 ? 0 : 1; }
		}
		
		public override int ItemsCount {
			get { return comboboxControl.Items.Count; }
		}
		
		public override ListItemProvider[] GetSelectedItemsProviders ()
		{
			if (comboboxControl.SelectedIndex == -1)
				return null;
			else
				return new ListItemProvider [] { (ListItemProvider) GetFocus () };
		}

		public override string GetItemName (ListItemProvider item)
		{
			if (ContainsItem (item) == true)
				return comboboxControl.Items [item.Index].ToString ();
			else
				return string.Empty;
		}
		
		public override System.Drawing.Rectangle GetItemBoundingRectangle (ListItemProvider item)
		{
			//FIXME: We need to improve this
			int loop = 1; //We start in 1 to add the Edit height or the control height depending on the style
			int index = item.Index;
			int totalHeight = 0;
			System.Drawing.Rectangle rectangle = System.Drawing.Rectangle.Empty;
			for (; loop < index; loop++)
				rectangle.Y += comboboxControl.GetItemHeight (loop);
			
			rectangle.Height = comboboxControl.GetItemHeight (index);
			rectangle.Width = comboboxControl.Bounds.Width;
			rectangle.X = comboboxControl.Bounds.X;		
			rectangle.Y += comboboxControl.Bounds.Y;

			if (comboboxControl.FindForm () == comboboxControl.Parent)
				rectangle = comboboxControl.TopLevelControl.RectangleToScreen (rectangle);
			else
				rectangle = comboboxControl.Parent.RectangleToScreen (rectangle);

			return rectangle;
		}
		
		public override void SelectItem (ListItemProvider item)
		{
			if (ContainsItem (item) == true)
				ListControl.SelectedIndex = item.Index;
		}

		public override void UnselectItem (ListItemProvider item)
		{
		}
		
		public override bool IsItemSelected (ListItemProvider item)
		{
			return ContainsItem (item) == false 
				? false : item.Index == ListControl.SelectedIndex;
		}
		
		protected override Type GetTypeOfObjectCollection ()
		{
			//Doesn't have any list-like children: only Edit, List and Button.
			return null;
		}
		
		protected override object GetInstanceOfObjectCollection ()
		{
			//Doesn't have any list-like children: only Edit, List and Button.
			return null;
		}

		public override void InitializeChildControlStructure ()
		{
			listboxProvider = new ComboBoxProvider.ComboBoxListBoxProvider (comboboxControl,
			                                                                this);			
			OnNavigationChildAdded (false, listboxProvider);
			UpdateBehaviors (false);
		}
		
		public override void FinalizeChildControlStructure ()
		{
			if (buttonProvider != null) {
				OnNavigationChildRemoved (false, buttonProvider);
				buttonProvider.Terminate ();
				buttonProvider = null;
			}
			if (listboxProvider != null) {
				OnNavigationChildRemoved (false, listboxProvider);
				listboxProvider.Terminate ();
				listboxProvider = null;
			}
			if (textboxProvider != null) {
				OnNavigationChildRemoved (false, textboxProvider);
				textboxProvider.Terminate ();
				textboxProvider = null;
			}
		}
		
		protected override ListProvider GetItemsListProvider ()
		{
			return listboxProvider;
		}
		
		public override void ScrollItemIntoView (ListItemProvider item)
		{
			if (ContainsItem (item) == true)
				throw new NotImplementedException ();
		}

		#endregion

		#region Private Methods
		
		private void OnDropDownStyleChanged (object sender, EventArgs args)
		{
			UpdateBehaviors (true);
		}
		
		private void UpdateBehaviors (bool generateEvent) 
		{
			if (comboboxControl.DropDownStyle == ComboBoxStyle.Simple) {
				SetBehavior (ExpandCollapsePatternIdentifiers.Pattern, 
				             null);
				SetBehavior (ValuePatternIdentifiers.Pattern,
				             new ComboBoxValueProviderBehavior (this));
				
				TerminateButtonProvider (generateEvent);
				InitializeEditProvider (generateEvent);
			} else if (comboboxControl.DropDownStyle == ComboBoxStyle.DropDown) {
				SetBehavior (ExpandCollapsePatternIdentifiers.Pattern,
				             new ComboBoxExpandCollapseProviderBehavior (this));
				SetBehavior (ValuePatternIdentifiers.Pattern,
				             new ComboBoxValueProviderBehavior (this));
				
				InitializeButtonProvider (generateEvent);
				InitializeEditProvider (generateEvent);
			} else if (comboboxControl.DropDownStyle == ComboBoxStyle.DropDownList) {
				SetBehavior (ExpandCollapsePatternIdentifiers.Pattern,
				             new ComboBoxExpandCollapseProviderBehavior (this));
				SetBehavior (ValuePatternIdentifiers.Pattern, 
				             null);
				
				InitializeButtonProvider (generateEvent);
				TerminateEditProvider (generateEvent);
			} 
		}
		
		private void InitializeEditProvider (bool generateEvent)
		{
			if (textboxProvider == null) {
				TextBox textbox = Helper.GetPrivateProperty<ComboBox, TextBox> (typeof (ComboBox), 
				                                                                comboboxControl, 
				                                                                "UIATextBox");
				textboxProvider = (TextBoxProvider) ProviderFactory.GetProvider (textbox);
				OnNavigationChildAdded (generateEvent, textboxProvider);
			}
		}
		
		private void TerminateEditProvider (bool generateEvent)
		{
			if (textboxProvider != null) {
				OnNavigationChildRemoved (generateEvent, textboxProvider);
				textboxProvider.Terminate ();
				textboxProvider = null;
			}
		}
		
		private void InitializeButtonProvider (bool generateEvent)
		{
			if (buttonProvider == null) {
				buttonProvider = new ComboBoxProvider.ComboBoxButtonProvider (comboboxControl,
				                                                               this);
				OnNavigationChildAdded (generateEvent, buttonProvider);
			}
		}
		
		private void TerminateButtonProvider (bool generateEvent)
		{
			if (buttonProvider != null) {
				OnNavigationChildRemoved (generateEvent, buttonProvider);
				buttonProvider.Terminate ();
				buttonProvider = null;
			}
		}
		
		#endregion
			
		#region Private Fields
		
		private ComboBox comboboxControl;
		private ComboBoxProvider.ComboBoxButtonProvider buttonProvider;
		private ComboBoxProvider.ComboBoxListBoxProvider listboxProvider;
		private TextBoxProvider textboxProvider;
		
		#endregion
		
		#region Internal Class: ListBox provider
		
		//TODO: This class missing ScrollBar Navigation.
		internal class ComboBoxListBoxProvider : ListProvider
		{
			
			public ComboBoxListBoxProvider (ComboBox control, 
			                                ComboBoxProvider provider)
				: base (control)
			{
				comboboxControl = control;
				comboboxProvider = provider;
			}
			
			public override IRawElementProviderFragmentRoot FragmentRoot {
				get { return comboboxProvider; }
			}
	
			public override object GetPropertyValue (int propertyId)
			{
				//TODO: Include: HelpTextProperty, LabeledByProperty, NameProperty
				if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id)
					return ControlType.List.Id;
				else if (propertyId == AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id)
					return true;
				else if (propertyId == AutomationElementIdentifiers.LocalizedControlTypeProperty.Id)
					return "list";
				else if (propertyId == AutomationElementIdentifiers.IsScrollPatternAvailableProperty.Id)
					return IsBehaviorEnabled (ScrollPatternIdentifiers.Pattern);
				else if (propertyId == AutomationElementIdentifiers.IsTablePatternAvailableProperty.Id)
					return false;
				else
					return base.GetPropertyValue (propertyId);
			}
			
			public override ListItemProvider GetItemProvider (int index)
			{
				return comboboxProvider.GetItemProvider (index);
			}
					
			public override int IndexOfItem (ListItemProvider item)
			{
				return comboboxProvider.IndexOfItem (item);
			}
			
			public override ListItemProvider RemoveItemAt (int index)
			{
				return comboboxProvider.RemoveItemAt (index);
			}
			
			public override IRawElementProviderFragment ElementProviderFromPoint (double x, double y)
			{
				return comboboxProvider.ElementProviderFromPoint (x, y);
			}
			
			public override int SelectedItemsCount { 
				get { return comboboxProvider.SelectedItemsCount; }
			}
			
			public override bool SupportsMultipleSelection { 
				get { return false; }
			}
			
			public override int ItemsCount {
				get { return comboboxProvider.ItemsCount; }
			}
			
			public override ListItemProvider[] GetSelectedItemsProviders ()
			{
				return comboboxProvider.GetSelectedItemsProviders ();
			}
			
			public override string GetItemName (ListItemProvider item)
			{
				return comboboxProvider.GetItemName (item);
			}
			
			public override System.Drawing.Rectangle GetItemBoundingRectangle (ListItemProvider item)
			{
				return comboboxProvider.GetItemBoundingRectangle (item);
			}
			
			public override void SelectItem (ListItemProvider item)
			{
				comboboxProvider.SelectItem (item);
			}
	
			public override void UnselectItem (ListItemProvider item)
			{
				comboboxProvider.UnselectItem (item);
			}
			
			public override bool IsItemSelected (ListItemProvider item)
			{
				return comboboxProvider.IsItemSelected (item);
			}
			
			protected override Type GetTypeOfObjectCollection ()
			{
				return typeof (ComboBox.ObjectCollection);
			}
			
			protected override object GetInstanceOfObjectCollection ()
			{
				return comboboxControl.Items;
			}
	
			public override void InitializeChildControlStructure ()
			{
				base.InitializeChildControlStructure ();
				
				for (int index = 0; index < comboboxControl.Items.Count; index++) {
					ListItemProvider item = GetItemProvider (index);
					OnNavigationChildAdded (false, item);
				}
			}
			
			public override void FinalizeChildControlStructure ()
			{
				base.FinalizeChildControlStructure ();
				
				for (int index = 0; index < comboboxControl.Items.Count; index++) {
					ListItemProvider item = GetItemProvider (index);
					OnNavigationChildAdded (false, item);
				}
			}

			public override void ScrollItemIntoView (ListItemProvider item) 
			{ 
				throw new NotImplementedException ();
			}

			private ComboBox comboboxControl;
			private ComboBoxProvider comboboxProvider;

		}
		
		#endregion

		#region Internal Class: Button provider

		internal class ComboBoxButtonProvider : FragmentControlProvider
		{

			public ComboBoxButtonProvider (Control control,
			                               ComboBoxProvider provider)
				: base (control)
			{
				SetBehavior (InvokePatternIdentifiers.Pattern, 
				             new ComboBoxButtonInvokeBehavior (provider));
			}
	
			public override object GetPropertyValue (int propertyId)
			{
				//TODO: i18n?
				if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id)
					return ControlType.Button.Id;
				else if (propertyId == AutomationElementIdentifiers.NameProperty.Id)
					return "Drop Down Button";
				else
					return base.GetPropertyValue (propertyId);
			}
		}
		
		#endregion
		
	}
}
