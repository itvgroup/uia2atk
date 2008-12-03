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
//      Sandy Armstrong <sanfordarmstrong@gmail.com>
// 

using System;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

namespace UiaAtkBridge
{
	public class TextImageLabel : TextLabel, Atk.ImageImplementor
	{
		public TextImageLabel (IRawElementProviderSimple provider) : base (provider)
		{
			imageProvider = provider;
		}
		
		#region ImageImplementor implementation 

		string imageDescription = null;

		bool? hasImage = null;
		Mono.UIAutomation.Bridge.IEmbeddedImage embeddedImage = null;
		protected object imageProvider = null;
		
		private bool HasImage {
			get {
				if (hasImage == null) {
					//type only available in our Provider implementation
					embeddedImage = imageProvider as Mono.UIAutomation.Bridge.IEmbeddedImage;
					
					if (embeddedImage == null) {
						Console.WriteLine ("WARNING: your provider implementation doesn't have unofficial IEmbeddedImage support");
						hasImage = false;
					} else
						hasImage = !embeddedImage.Bounds.IsEmpty;
				}
				
				return hasImage.Value;
			}
		}
		
		public string ImageDescription
		{
			get { return imageDescription; }
		}
		
		public void GetImageSize (out int width, out int height)
		{
			width = -1;
			height = -1;
			if (HasImage) {
				width = (int)embeddedImage.Bounds.Width;
				height = (int)embeddedImage.Bounds.Height;
			}
		}
		
		public void GetImagePosition (out int x, out int y, Atk.CoordType coordType)
		{
			x = int.MinValue;
			y = int.MinValue;
			if (HasImage) {
				x = (int)embeddedImage.Bounds.X;
				y = (int)embeddedImage.Bounds.Y;
			}
		}
		
		public bool SetImageDescription (string description)
		{
			if (HasImage) {
				imageDescription = description;
				return true;
			}
			return false;
		}
		
		public string ImageLocale 
		{
			get { return imageDescription; /*TODO*/ }
		}
		
		#endregion
	}
}
