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

using System.Collections.Generic;

using System.Windows;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

using NUnit.Framework;
using Mono.UIAutomation.Bridge;

namespace MonoTests.Mono.UIAutomation.Winforms
{
	public static class TestHelper
	{
		public static MockBridge SetUpEnvironment ()
		{
			Type interopProviderType = typeof (AutomationInteropProvider);

			// Set the bridge assembly name to a value that will
			// fail when the static constructor attempts to load it.
			Environment.SetEnvironmentVariable ("MONO_UIA_BRIDGE",
			                                    "garbage");

			/*
			// HACK: Clear the string referencing the UiaAtkBridge
			//       assembly so that when AutomationInteropProvider's
			//       static constructor attempts to load a bridge,
			//       it will fail.
			Type bridgeManagerType =
				interopProviderType.Assembly.GetType ("System.Windows.Automation.Provider.BridgeManager");
			FieldInfo assemblyField =
				bridgeManagerType.GetField ("UiaAtkBridgeAssembly",
				                            BindingFlags.NonPublic | BindingFlags.Static);
			assemblyField.SetValue (null, string.Empty);
			*/
			
			// Inject a mock automation bridge into the
			// AutomationInteropProvider, so that we don't try
			// to load the UiaAtkBridge.
			MockBridge bridge = new MockBridge ();
			FieldInfo bridgeField =
				interopProviderType.GetField ("bridge", BindingFlags.NonPublic
				                                        | BindingFlags.Static);
			bridgeField.SetValue (null, bridge);
			
			bridge.ClientsAreListening = true;

			return bridge;
		}

		public static void TearDownEnvironment ()
		{
			Type interopProviderType = typeof (AutomationInteropProvider);
			FieldInfo bridgeField =
				interopProviderType.GetField ("bridge", BindingFlags.NonPublic | BindingFlags.Static);
			bridgeField.SetValue (null, null);

			
			Environment.SetEnvironmentVariable ("MONO_UIA_BRIDGE",
			                                   string.Empty);
		}

		public static void TestAutomationProperty (IRawElementProviderSimple provider,
		                                           AutomationProperty property,
		                                           object expectedValue)
		{
			Assert.AreEqual (expectedValue,
			                 provider.GetPropertyValue (property.Id),
			                 property.ProgrammaticName);
		}

		private static List <AutomationPattern> allPatterns = null;
		
		private static List <AutomationPattern> GetAllPossiblePatterns () {
			if (allPatterns == null) {
				allPatterns = new List<AutomationPattern> ();

				allPatterns.Add (DockPatternIdentifiers.Pattern);
				allPatterns.Add (ExpandCollapsePatternIdentifiers.Pattern);
				allPatterns.Add (GridItemPatternIdentifiers.Pattern);
				allPatterns.Add (InvokePatternIdentifiers.Pattern);
				allPatterns.Add (MultipleViewPatternIdentifiers.Pattern);
				allPatterns.Add (RangeValuePatternIdentifiers.Pattern);
				allPatterns.Add (ScrollPatternIdentifiers.Pattern);
				allPatterns.Add (ScrollItemPatternIdentifiers.Pattern);
				allPatterns.Add (SelectionPatternIdentifiers.Pattern);
				allPatterns.Add (SelectionItemPatternIdentifiers.Pattern);
				allPatterns.Add (TablePatternIdentifiers.Pattern);
				allPatterns.Add (TableItemPatternIdentifiers.Pattern);
				allPatterns.Add (TogglePatternIdentifiers.Pattern);
				allPatterns.Add (TransformPatternIdentifiers.Pattern);
				allPatterns.Add (ValuePatternIdentifiers.Pattern);
				allPatterns.Add (WindowPatternIdentifiers.Pattern);
			}

			return allPatterns;
		}
		
		internal static void TestPatterns (string msg, IRawElementProviderSimple provider, params AutomationPattern [] expected)
		{
			List <AutomationPattern> expectedPatterns = new List <AutomationPattern> (expected);
			List <AutomationPattern> missingPatterns = new List <AutomationPattern> ();
			List <AutomationPattern> superfluousPatterns = new List <AutomationPattern> ();
			
			foreach (AutomationPattern pattern in GetAllPossiblePatterns ()) {
				object behaviour = provider.GetPatternProvider (pattern.Id);
				if (expectedPatterns.Contains (pattern) &&
				    behaviour == null)
					missingPatterns.Add (pattern);
				else if ((!expectedPatterns.Contains (pattern)) && 
					     behaviour != null)
					superfluousPatterns.Add (pattern);
			}

			string missingPatternsMsg = string.Empty;
			string superfluousPatternsMsg = string.Empty;

			if (missingPatterns.Count != 0) {
				missingPatternsMsg = "Missing patterns: ";
				foreach (AutomationPattern pattern in missingPatterns)
					missingPatternsMsg += pattern.ProgrammaticName + ",";
			}
			if (superfluousPatterns.Count != 0) {
				superfluousPatternsMsg = "Superfluous patterns: ";
				foreach (AutomationPattern pattern in superfluousPatterns)
					superfluousPatternsMsg += pattern.ProgrammaticName + ",";
			}
			Assert.IsTrue ((missingPatterns.Count == 0) && (superfluousPatterns.Count == 0),
			  msg + "; " + missingPatternsMsg + " .. " + superfluousPatternsMsg);
		}
	}
}
