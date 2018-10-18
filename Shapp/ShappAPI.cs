using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Shapp
{
    public class ShappAPI
    {
        public int Elo()
        {
            ScriptEngine py = Python.CreateEngine();
            ScriptScope s = py.CreateScope();
            var ironPythonRuntime = Python.CreateRuntime();
            var script = py.CreateScriptSourceFromString(Properties.Resources.test);
            script.Execute(s);
            var myadd = s.GetVariable("myadd");
            var myFoo = myadd(2, 5);
            //var n = (double)myFoo.add(100, 200);
            return myFoo;
        }
    }
}
