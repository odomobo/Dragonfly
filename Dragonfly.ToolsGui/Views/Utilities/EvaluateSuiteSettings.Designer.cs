﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Dragonfly.ToolsGui.Views.Utilities {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.8.0.0")]
    internal sealed partial class EvaluateSuiteSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static EvaluateSuiteSettings defaultInstance = ((EvaluateSuiteSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new EvaluateSuiteSettings())));
        
        public static EvaluateSuiteSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string InputFile {
            get {
                return ((string)(this["InputFile"]));
            }
            set {
                this["InputFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string OutputFile {
            get {
                return ((string)(this["OutputFile"]));
            }
            set {
                this["OutputFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1000000")]
        public int NodeCount {
            get {
                return ((int)(this["NodeCount"]));
            }
            set {
                this["NodeCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int ThreadCount {
            get {
                return ((int)(this["ThreadCount"]));
            }
            set {
                this["ThreadCount"] = value;
            }
        }
    }
}
