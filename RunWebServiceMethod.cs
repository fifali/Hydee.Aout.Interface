using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hydee.Aout.Interface
{
    public class RunWebServiceMethod
    {
        public object RunMethod(CompilerResults cr, string strClassName, string strMethodName, object[] args)
        {
            string @namespace = "Hydee.Aout.Interface";

            System.Reflection.Assembly assembly = cr.CompiledAssembly;
            Type t = assembly.GetType(@namespace + "." + strClassName, true, true);
            object obj = Activator.CreateInstance(t);
            System.Reflection.MethodInfo mi = t.GetMethod(strMethodName);

            return mi.Invoke(obj, args);
        }

        public Type GetType(CompilerResults cr, string strClassName)
        {
            string @namespace = "Hydee.Aout.Interface";

            System.Reflection.Assembly assembly = cr.CompiledAssembly;
            Type t = assembly.GetType(@namespace + "." + strClassName, true, true);

            return t;
        }
    }
}
