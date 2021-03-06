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
//      Brad Taylor <brad@getcoded.net>
// 

using System;
using System.Collections.Generic;

using System.Windows.Automation;
using System.Windows.Automation.Provider;

namespace UiaAtkBridge
{
	public class SplitButton : ComponentParentAdapter
	{
#region Public Methods
		public SplitButton (IRawElementProviderSimple provider)
			: base (provider)
		{
			Role = Atk.Role.Filler;

			button = AutomationBridge.CreateAdapter<Button> (provider);
			if (button != null) {
				button.ManagesRemoval = true;
				base.AddOneChild (button);
			}

			ec_button = AutomationBridge.CreateAdapter<ExpandCollapseButton> (provider);
			if (ec_button != null) {
				ec_button.ManagesRemoval = true;
				base.AddOneChild (ec_button);
			}
		}

		public override void RaiseStructureChangedEvent (object provider, StructureChangedEventArgs e)
		{
		}
#endregion
		
#region Protected Methods
		// Intentionally don't RequestChildren, as this walks the UIA
		// hierarchy which is different than ours
		internal override void RequestChildren ()
		{
		}

		protected override void RemoveUnmanagedChildren ()
		{
			if (button != null)
				base.RemoveChild (button);
			if (ec_button != null)
				base.RemoveChild (ec_button);
		}
#endregion

#region Internal Methods
		internal override void AddOneChild (Atk.Object child)
		{
			if (ec_button != null)
				ec_button.AddOneChild (child);
		}
		
		internal override void RemoveChild (Atk.Object child)
		{
			if (ec_button != null)
				ec_button.RemoveChild (child);
		}
#endregion

#region Private Fields
		private Button button;
		private ExpandCollapseButton ec_button;
#endregion
	}
}
