﻿#pragma checksum "C:\Users\71972\Desktop\HereYou\HereYou\View\LoginJWXT.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "BAE3A83728862588473B954B1772CBB7"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HereYou
{
    partial class LoginJWXT : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                {
                    this.login = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 2:
                {
                    this.webview = (global::Windows.UI.Xaml.Controls.WebView)(target);
                }
                break;
            case 3:
                {
                    this.stuNum = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 4:
                {
                    this.password = (global::Windows.UI.Xaml.Controls.PasswordBox)(target);
                }
                break;
            case 5:
                {
                    this.j_code = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                    #line 51 "..\..\..\View\LoginJWXT.xaml"
                    ((global::Windows.UI.Xaml.Controls.TextBox)this.j_code).KeyUp += this.j_code_KeyUp;
                    #line default
                }
                break;
            case 6:
                {
                    global::Windows.UI.Xaml.Controls.Button element6 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 58 "..\..\..\View\LoginJWXT.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element6).Click += this.freshCode;
                    #line default
                }
                break;
            case 7:
                {
                    this.remember = (global::Windows.UI.Xaml.Controls.CheckBox)(target);
                }
                break;
            case 8:
                {
                    this.signin = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 83 "..\..\..\View\LoginJWXT.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.signin).Click += this.postSignin;
                    #line default
                }
                break;
            case 9:
                {
                    this.j_codepic = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

