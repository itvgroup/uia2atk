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
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using System.Windows.Forms;

namespace Mono.UIAutomation.Winforms
{

	//A TextPatternRange can represent an 
	//- insertion point, 
	//- a subset, or 
	//- all of the text in a TextPattern container.
	public class TextRangeProvider : ITextRangeProvider
	{

#region Constructors

		public TextRangeProvider (ITextProvider provider, TextBoxBase textboxbase)
		{
			this.provider = provider;
			this.textboxbase = textboxbase;
			
			normalizer = new TextNormalizer (textboxbase);
		}
		
#endregion
		
#region Public Properties 

		public ITextProvider TextProvider {
			get { return provider; }
		}
		
		public int StartPoint {
			get { return normalizer.StartPoint; }
		}
		
		public int EndPoint {
			get { return normalizer.EndPoint; }
		}

#endregion

#region ITextRangeProvider Members

		public void AddToSelection ()
		{
			if (provider.SupportedTextSelection != SupportedTextSelection.Multiple)
				throw new InvalidOperationException ();
			
			//TODO: Verify if RichTextBox supports: SupportedTextSelection.Multiple
		}

		public ITextRangeProvider Clone ()
		{
			return (ITextRangeProvider) MemberwiseClone ();
		}

		//TODO: Evaluate rangeProvider == null
		public bool Compare (ITextRangeProvider range)
		{
			TextRangeProvider rangeProvider = range as TextRangeProvider;
			if (rangeProvider.TextProvider != provider)
				throw new ArgumentException ();
		
			return (rangeProvider.StartPoint != StartPoint) 
				&& (rangeProvider.EndPoint != EndPoint);
		}

		//TODO: Evaluate rangeProvider == null
		public int CompareEndpoints (TextPatternRangeEndpoint endpoint, 
		                             ITextRangeProvider targetRange,
		                             TextPatternRangeEndpoint targetEndpoint)
		{
			TextRangeProvider targetRangeProvider = targetRange as TextRangeProvider;
			if (targetRangeProvider.TextProvider != provider)
				throw new ArgumentException ();
			
			int point = endpoint == TextPatternRangeEndpoint.End ? EndPoint : StartPoint;
			int targePoint = targetEndpoint == TextPatternRangeEndpoint.End 
				? targetRangeProvider.EndPoint : targetRangeProvider.StartPoint;

			return point - targePoint;
		}

		//Character, Format, Word, Line, Paragraph, Page, Document
		public void ExpandToEnclosingUnit (TextUnit unit)
		{
			if (unit == TextUnit.Character) {
				//Does nothing.
			} else if (unit == TextUnit.Word) {
				normalizer.WordNormalize ();
			}
			//TODO: ADD MISSING
			throw new NotImplementedException();
		}

		public ITextRangeProvider FindAttribute (int attribute, object value, 
		                                         bool backward)
		{
			throw new NotImplementedException();
		}

		public ITextRangeProvider FindText (string text, bool backward, 
		                                    bool ignoreCase)
		{
			throw new NotImplementedException();
		}

		public object GetAttributeValue (int attribute)
		{
			throw new NotImplementedException();
		}

		public double[] GetBoundingRectangles ()
		{
			throw new NotImplementedException();
		}

		public IRawElementProviderSimple[] GetChildren ()
		{
			throw new NotImplementedException();
		}

		public IRawElementProviderSimple GetEnclosingElement ()
		{
			throw new NotImplementedException();
		}

		public string GetText (int maxLength)
		{
			if (maxLength < -1)
				throw new ArgumentOutOfRangeException ();
				
			if (maxLength == -1)
				maxLength = 0;
			
			int startPoint = 0;
			int endPoint = 0;
			if (StartPoint > EndPoint) {
				startPoint = EndPoint;
				endPoint = StartPoint;
			}

			int length = endPoint - startPoint;
			if (length > maxLength)
				length = maxLength;

			return textboxbase.Text.Substring (startPoint, length);
		}

		//Character, Format, Word, Line, Paragraph, Page, Document
		public int Move (TextUnit unit, int count)
		{
			TextNormalizerPoints points = normalizer.Move (unit, count);
			
			return points.Moved;
		}

		public void MoveEndpointByRange (TextPatternRangeEndpoint endpoint, 
		                                 ITextRangeProvider targetRange, 
		                                 TextPatternRangeEndpoint targetEndpoint)
		{
			throw new NotImplementedException();
		}

		public int MoveEndpointByUnit (TextPatternRangeEndpoint endpoint, 
		                               TextUnit unit, int count)
		{
			//TODO: OK?
			if (this == TextProvider.DocumentRange)
				return 0;
			
			if (unit == TextUnit.Character) {
				if (endpoint == TextPatternRangeEndpoint.Start)
					return normalizer.CharacterMoveStartPoint (count);
				else
					return normalizer.CharacterMoveEndPoint (count);
			} else if (unit == TextUnit.Word) {
				if (endpoint == TextPatternRangeEndpoint.Start)
					return normalizer.WordMoveStartPoint (count);
				else
					return normalizer.WordMoveEndPoint (count);
			}
			
			//TODO: ADD MISSING

			throw new NotImplementedException();
		}

		public void RemoveFromSelection ()
		{
			if (provider.SupportedTextSelection != SupportedTextSelection.Multiple)
				throw new InvalidOperationException ();
			
			//TODO: Verify if RichTextBox supports: SupportedTextSelection.Multiple
		}

		public void ScrollIntoView (bool alignToTop)
		{
			throw new NotImplementedException();
		}

		public void Select ()
		{
			textboxbase.SelectionStart = normalizer.StartPoint;
			textboxbase.SelectionLength = normalizer.EndPoint - normalizer.StartPoint;
		}
		
#endregion

#region Private Members

		private ITextProvider provider;
		private TextBoxBase textboxbase;
		private TextNormalizer normalizer;

#endregion

	}
}
