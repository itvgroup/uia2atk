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
//	Mario Carrion <mcarrion@novell.com>
// 

using Mono.UIAutomation.Bridge;
using Mono.UIAutomation.Services;
using System;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

namespace UiaAtkBridge
{

	internal class EditableTextImplementor
	{
		
		public EditableTextImplementor (Adapter adapter, Atk.TextImplementor textImplementor)
		{
			this.adapter = adapter;
			this.textImplementor = textImplementor;

			valueProvider 
				= adapter.Provider.GetPatternProvider (ValuePatternIdentifiers.Pattern.Id)
					as IValueProvider;
			textProvider
				= adapter.Provider.GetPatternProvider (TextPatternIdentifiers.Pattern.Id)
					as ITextProvider;
			textExpert = TextImplementorFactory.GetImplementor (adapter, adapter.Provider);

			valueProvider = adapter.Provider.GetPatternProvider (ValuePatternIdentifiers.Pattern.Id)
				as IValueProvider;
			if (valueProvider != null)
				editable = !valueProvider.IsReadOnly;

			oldText = textExpert.Text;
			
			// FIXME:
			// The following lines must be refactored, so our internal Bridge 
			// interfaces (IText and IClipboardSupport) will be called by using
			// GetPatternProvider instead of doing explicit casting.
			CaretProvider = textProvider as IText;
			if (CaretProvider == null)
				CaretProvider = valueProvider as IText;
			CaretOffset = (CaretProvider != null ? CaretProvider.CaretOffset : textExpert.Length);

			ClipboardProvider = textProvider as IClipboardSupport;
			if (ClipboardProvider == null)
				ClipboardProvider = valueProvider as IClipboardSupport;
		}

		#region Public Properties

		public IText CaretProvider {
			get;
			private set;
		}

		public int CaretOffset {
			get { return caretOffset; }
			set { caretOffset = value; }
		}

		public IClipboardSupport ClipboardProvider {
			get;
			private set;
		}

		public bool Editable {
			get { return editable; }
			private set {
				if (editable == value)
					return;

				editable = value;
				adapter.NotifyStateChange (Atk.StateType.Editable, editable);
			}
		}

		#endregion

		#region Atk.EditableTextImplementor implementation 
		
		public bool SetRunAttributes (GLib.SList attrib_set, int start_offset, int end_offset)
		{
			return false;
		}
		
		public void InsertText (string str, ref int position)
		{
			if (position < 0 || position > textExpert.Length)
				position = textExpert.Length;	// gail

			TextContents = textExpert.Text.Substring (0, position)
				+ str
				+ textExpert.Text.Substring (position);
		}
		
		public void CopyText (int startPos, int endPos)
		{
			if (ClipboardProvider == null)
				return;

			ClipboardProvider.Copy (startPos, endPos);
		}
		
		public void CutText (int startPos, int endPos)
		{
			if (ClipboardProvider == null)
				return;

			ClipboardProvider.Copy (startPos, endPos);
			DeleteText (startPos, endPos);
		}
		
		public void DeleteText (int startPos, int endPos)
		{
			if (startPos < 0)
				startPos = 0;
			if (endPos < 0 || endPos > textExpert.Length)
				endPos = textExpert.Length;
			if (startPos > endPos)
				startPos = endPos;

			if (TextContents == null)
				return;

			TextContents = TextContents.Remove (startPos, endPos - startPos);
		}
		
		public void PasteText (int position)
		{
			if (ClipboardProvider == null)
				return;

			ClipboardProvider.Paste (position);
		}
		
		public string TextContents {
			get {
				if (valueProvider == null)
					return null;

				return valueProvider.Value; 
			}
			set {
				if (valueProvider == null) {
					Log.Warn (string.Format ("Cannot set text on an '{0}' Provider {1} Does not implement IValueProvider.",
					                         adapter.GetType (),
					                         adapter.Provider.GetType ()));
					return;
				}

				if (!valueProvider.IsReadOnly) {
					try {
						valueProvider.SetValue (value);
					} catch (Exception e) {
						Log.Error ("EditableTextImplementor: Caught exception while trying to set value:\n{0}", e);
					}
				}
			}
		}
		
		#endregion

		#region Events Methods

		public bool RaiseAutomationPropertyChangedEvent (AutomationPropertyChangedEventArgs e)
		{
			if (e.Property.Id == AutomationElementIdentifiers.IsValuePatternAvailableProperty.Id) {
				bool isAvailable = (bool) e.NewValue;
				if (!isAvailable) {
					valueProvider = null;
					Editable = false;
					ClipboardProvider = null;
				} else {
					valueProvider = adapter.Provider.GetPatternProvider (ValuePatternIdentifiers.Pattern.Id)
						as IValueProvider;
					if (valueProvider != null) {
						Editable = !valueProvider.IsReadOnly;
	
						// The following lines must be replaced after refactoring
						if (ClipboardProvider == null)
							ClipboardProvider = valueProvider as IClipboardSupport;
					}
				}
				
				return true;
			} else if (e.Property.Id == ValuePatternIdentifiers.ValueProperty.Id) {
				// Don't fire spurious events if the text hasn't changed
				if (textExpert.HandleSimpleChange (ref oldText, 
				                                   ref caretOffset, 
				                                   CaretProvider == null))
					return true;

				Atk.TextAdapter textAdapter = new Atk.TextAdapter (textImplementor);
				string newText = textExpert.Text;

				// First delete all text, then insert the new text
				textAdapter.EmitTextChanged (Atk.TextChangedDetail.Delete, 
				                             0, 
				                             oldText.Length);

				textAdapter.EmitTextChanged (Atk.TextChangedDetail.Insert, 
				                             0,
				                             newText == null ? 0 : newText.Length);

				if (CaretProvider == null)
					caretOffset = textExpert.Length;

				oldText = newText;

				GLib.Signal.Emit (adapter, "visible-data-changed");

				return true;
			} else if (e.Property.Id == ValuePatternIdentifiers.IsReadOnlyProperty.Id) {
				bool? isReadOnlyVal = e.NewValue as bool?;
				if (isReadOnlyVal == null && valueProvider != null)
					isReadOnlyVal = valueProvider.IsReadOnly;
				Editable = isReadOnlyVal ?? false;

				return true;
			}
			
			return false;
		}
		
		public bool RaiseAutomationEvent (AutomationEvent eventId, AutomationEventArgs e)
		{
			if (eventId == TextPatternIdentifiers.CaretMovedEvent) {
				int newCaretOffset = CaretProvider.CaretOffset;
				if (newCaretOffset != caretOffset) {
					caretOffset = newCaretOffset;
					GLib.Signal.Emit (adapter, "text_caret_moved", caretOffset);
				}
				return true;
			} else if (eventId == TextPatternIdentifiers.TextSelectionChangedEvent) {
				GLib.Signal.Emit (adapter, "text_selection_changed");
				return true;
			} 
			
			return false;
		}

		#endregion

		#region Private Fields

		private Adapter adapter;
		private int caretOffset = -1;
		private bool editable;
		private string oldText;
		private ITextImplementor textExpert;		
		private Atk.TextImplementor textImplementor;
		private ITextProvider textProvider;
		private IValueProvider valueProvider;

		#endregion
	}
}