#pragma checksum "D:\Visual Studio 2008\Projects\CheckBoxSample\CheckBoxSample\Page.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "6CBDD9C55530F2501A5A44E1A27D4427"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace CheckBoxSample {
    
    
    public partial class Page : System.Windows.Controls.UserControl {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.CheckBox checkbox1;
        
        internal System.Windows.Controls.TextBlock text1;
        
        internal System.Windows.Controls.CheckBox checkbox2;
        
        internal System.Windows.Controls.TextBlock text2;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/CheckBoxSample;component/Page.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.checkbox1 = ((System.Windows.Controls.CheckBox)(this.FindName("checkbox1")));
            this.text1 = ((System.Windows.Controls.TextBlock)(this.FindName("text1")));
            this.checkbox2 = ((System.Windows.Controls.CheckBox)(this.FindName("checkbox2")));
            this.text2 = ((System.Windows.Controls.TextBlock)(this.FindName("text2")));
        }
    }
}