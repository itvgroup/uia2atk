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
//	Brad Taylor <brad@getcoded.net>
// 

using System;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using SWF = System.Windows.Forms;
using Mono.UIAutomation.Winforms;
using Mono.UIAutomation.Winforms.Events;
using Mono.UIAutomation.Winforms.Events.DomainUpDown;

namespace Mono.UIAutomation.Winforms.Behaviors.DomainUpDown
{
	internal class ListItemValueProviderBehavior
		: ProviderBehavior, IValueProvider
	{
#region Public Methods
		public ListItemValueProviderBehavior (ListItemProvider itemProvider)
			: base (itemProvider)
		{
		}
#endregion
		
#region ProviderBehavior Specialization
		public override AutomationPattern ProviderPattern {
			get { return ValuePatternIdentifiers.Pattern; }
		}
		
		public override void Connect ()
		{
			// XXX: Not raising the ReadOnly event, there doesn't seem to be a way to change it.
			Provider.SetEvent (ProviderEventType.ValuePatternValueProperty,
			                   new ListItemValuePatternValueEvent ((ListItemProvider) Provider));
		}
		
		public override void Disconnect ()
		{
			Provider.SetEvent (ProviderEventType.ValuePatternIsReadOnlyProperty,
			                   null);
			Provider.SetEvent (ProviderEventType.ValuePatternValueProperty,
			                   null);
		}

		public override object GetPropertyValue (int propertyId)
		{
			if (propertyId == ValuePatternIdentifiers.IsReadOnlyProperty.Id)
				return IsReadOnly;
			else if (propertyId == ValuePatternIdentifiers.ValueProperty.Id)
				return Value;
			else
				return base.GetPropertyValue (propertyId);
		}
#endregion

#region IValueProvider implementation 
		public void SetValue (string value)
		{
			if (IsReadOnly == true)
				throw new ElementNotEnabledException ();

			PerformSetValue (value);
		}
		
		public bool IsReadOnly {
			get { return false; }
		}
		
		public string Value {
			get {
				var val =  Convert.ToString (((ListItemProvider)Provider).ObjectItem);
				return val ?? string.Empty;
			}
		}
#endregion

#region Private Methods
		private void PerformSetValue (string value)
		{
			// TODO: threading
			
			// TODO: 
			//((ListItemProvider)Provider).ObjectItem = value;
		}
#endregion
	}

	delegate void ListItemSetValueDelegate (string value);
}
