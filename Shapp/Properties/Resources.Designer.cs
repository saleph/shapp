﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Shapp.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Shapp.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to import classad
        ///import htcondor
        ///import os
        ///import random
        ///
        ///
        ///def perform_log():
        ///    schedd = htcondor.Schedd(htcondor.Collector().locate(htcondor.DaemonTypes.Schedd, &quot;masterubuntu&quot;))
        ///    # for history in schedd.history(classad.ExprTree(&quot;1&quot;), [&quot;ClusterId&quot;, &quot;ProcId&quot;], 10):
        ///    #     print &quot;{}.{}&quot;.format(history[&quot;ClusterId&quot;], history[&quot;ProcId&quot;])
        ///    #     print &quot;Attribs no: &quot;, len(history)
        ///    jobs = schedd.query(&quot;1&quot;, [&quot;ClusterId&quot;, &quot;JobStatus&quot;, &quot;LastRemoteHost&quot;])
        ///    for job in jobs:
        ///        print (&quot;Cl [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string main {
            get {
                return ResourceManager.GetString("main", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ExitBySignal = false;
        ///Owner = &quot;tgalecki&quot;;
        ///BufferBlockSize = 32768;
        ///ExitCode = 0;
        ///Err = &quot;/dev/null&quot;;
        ///BytesRecvd = 3.137600000000000E+04;
        ///CondorVersion = &quot;$CondorVersion: 8.7.7 Mar 12 2018 BuildID: UW_Python_Wheel_Build $&quot;;
        ///MyType = &quot;Job&quot;;
        ///PeriodicRelease = false;
        ///ImageSize = 100;
        ///WhenToTransferOutput = &quot;ON_EXIT&quot;;
        ///AutoClusterId = 3;
        ///ShouldTransferFiles = &quot;YES&quot;;
        ///LocalSysCpu = 0.0;
        ///LastJobStatus = 2;
        ///NumCkpts_RAW = 0;
        ///OrigMaxHosts = 1;
        ///BytesSent = 0.0;
        ///ClusterId = 5;
        ///JobFinishedHookDone = 152 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string parameters {
            get {
                return ResourceManager.GetString("parameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] submit {
            get {
                object obj = ResourceManager.GetObject("submit", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #!/usr/bin/env python
        ///import sys
        ///import os
        ///import htcondor
        ///
        ///def myadd(first, second):
        ///	return first+second
        ///
        ///print(myadd(2,4))
        ///.
        /// </summary>
        internal static string testscript {
            get {
                return ResourceManager.GetString("testscript", resourceCulture);
            }
        }
    }
}
