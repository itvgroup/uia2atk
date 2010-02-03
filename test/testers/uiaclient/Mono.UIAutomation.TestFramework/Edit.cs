// Edit.cs: Edit control class wrapper.
//
// This program is free software; you can redistribute it and/or modify it under
// the terms of the GNU General Public License version 2 as published by the
// Free Software Foundation.
//
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along with
// this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
//
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
//
// Authors:
//	Ray Wang  (rawang@novell.com)
//	Felicia Mu  (fxmu@novell.com)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;

namespace Mono.UIAutomation.TestFramework
{
	public class Edit : Element
	{
		public static readonly ControlType UIAType = ControlType.Edit;

		public Edit (AutomationElement elm)
			: base (elm)
		{
		}

		// The method of ValuePattern
		public void SetValue (string value)
		{
			SetValue (value, true);
		}

		public void SetValue (string value, bool log)
		{
			if (log)
				procedureLogger.Action (string.Format ("Set {0} to {1}.", value, this.NameAndType));

			ValuePattern vp = (ValuePattern) element.GetCurrentPattern (ValuePattern.Pattern);
			vp.SetValue (value);
		}

		// The properties of ValuePattern
		public string Value {
			get { return (string) element.GetCurrentPropertyValue (ValuePattern.ValueProperty); }
		}

		public bool IsReadOnly {
			get { return (bool) element.GetCurrentPropertyValue (ValuePattern.IsReadOnlyProperty); }
		}

		// The properties of TableItemPattern
		public int Column {
			get { return (int) element.GetCurrentPropertyValue (TableItemPattern.ColumnProperty); }
		}

		public int ColumnSpan {
			get { return (int) element.GetCurrentPropertyValue (TableItemPattern.ColumnSpanProperty); }
		}

		public int Row {
			get { return (int) element.GetCurrentPropertyValue (TableItemPattern.RowProperty); }
		}

		public int RowSpan {
			get { return (int) element.GetCurrentPropertyValue (TableItemPattern.RowSpanProperty); }
		}

		public AutomationElement ContainingGrid {
			get { return (AutomationElement) element.GetCurrentPropertyValue (TableItemPattern.ContainingGridProperty); }
		}

		public AutomationElement[] ColumnHeaderItems {
			get { return (AutomationElement[]) element.GetCurrentPropertyValue (TableItemPattern.ColumnHeaderItemsProperty); }
		}

		public AutomationElement[] RowHeaderItems {
			get { return (AutomationElement[]) element.GetCurrentPropertyValue (TableItemPattern.RowHeaderItemsProperty); }
		}
	}
}
