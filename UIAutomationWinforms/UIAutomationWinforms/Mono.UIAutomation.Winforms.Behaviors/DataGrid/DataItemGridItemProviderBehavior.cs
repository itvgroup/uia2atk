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
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using SWF = System.Windows.Forms;
using Mono.UIAutomation.Winforms;
using Mono.UIAutomation.Winforms.Events;
using Mono.UIAutomation.Winforms.Events.DataGrid;

namespace Mono.UIAutomation.Winforms.Behaviors.DataGrid
{
	internal class DataItemGridItemProviderBehavior
		: ProviderBehavior, IGridItemProvider
	{
		#region Constructors
		
		public DataItemGridItemProviderBehavior (ListItemProvider itemProvider)
			: base (itemProvider)
		{
			this.itemProvider = (DataGridProvider.DataGridDataItemProvider) itemProvider;
		}
		
		#endregion
		
		#region ProviderBehavior Specialization
		
		public override AutomationPattern ProviderPattern {
			get { return GridItemPatternIdentifiers.Pattern; }
		}
		
		public override void Connect ()
		{
			// NOTE: RowSpan Property NEVER changes.
			// NOTE: ColumnSpan Property NEVER changes.
			// NOTE: ContainingGrid Property NEVER changes.
			// NOTE: ColumnProperty Property NEVER changes.
			Provider.SetEvent (ProviderEventType.GridItemPatternRowSpanProperty,
			                   new DataItemGridItemPatternRowEvent ((ListItemProvider) Provider));
		}
		
		public override void Disconnect ()
		{
			Provider.SetEvent (ProviderEventType.GridItemPatternRowProperty,
			                   null);
			Provider.SetEvent (ProviderEventType.GridItemPatternColumnProperty,
			                   null);
			Provider.SetEvent (ProviderEventType.GridItemPatternRowSpanProperty,
			                   null);
			Provider.SetEvent (ProviderEventType.GridItemPatternColumnProperty,
			                   null);
			Provider.SetEvent (ProviderEventType.GridItemPatternContainingGridProperty,
			                   null);
		}

		public override object GetPropertyValue (int propertyId)
		{
			if (propertyId == GridItemPatternIdentifiers.RowProperty.Id)
				return Row;
			else if (propertyId == GridItemPatternIdentifiers.ColumnProperty.Id)
				return Column;
			else if (propertyId == GridItemPatternIdentifiers.RowSpanProperty.Id)
				return RowSpan;
			else if (propertyId == GridItemPatternIdentifiers.ColumnSpanProperty.Id)
				return ColumnSpan;
			else if (propertyId == GridItemPatternIdentifiers.ContainingGridProperty.Id)
				return ContainingGrid;
			else
				return base.GetPropertyValue (propertyId);
		}
		
		#endregion
		
		#region IGridItemProvider Specialization
		
		public int Row {
			get { return itemProvider.Index; }
		}
		
		public int Column {
			get { return 0; }
		}
		
		public int RowSpan {
			get { return 1; }
		}
		
		public int ColumnSpan {
			get { return 1; }
		}
		
		public IRawElementProviderSimple ContainingGrid {
			get { return Provider.FragmentRoot; }
		}
		
		#endregion

		#region Protected Properties

		protected DataGridProvider.DataGridDataItemProvider ItemProvider {
			get { return itemProvider; }
		}

		#endregion
		
		#region Private Fields

		private DataGridProvider.DataGridDataItemProvider itemProvider;
		
		#endregion
		
	}
}
