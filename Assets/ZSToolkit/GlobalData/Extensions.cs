using System;
using System.Text;

namespace ZSToolkit.GlobalData.Extensions
{
    public static class GlobalDataExtensions
    {
        public static string GenericToString(this Type type)
        {
            StringBuilder retType = new StringBuilder();

            if (type.IsGenericType)
            {
                string[] parentType = type.FullName.Split('`');
                // We will build the type here.
                Type[] arguments = type.GetGenericArguments();

                StringBuilder argList = new StringBuilder();
                foreach (Type t in arguments)
                {
                    // Let's make sure we get the argument list.
                    string arg = GenericToString(t);
                    if (argList.Length > 0)
                    {
                        argList.AppendFormat(", {0}", arg);
                    }
                    else
                    {
                        argList.Append(arg);
                    }
                }

                if (argList.Length > 0)
                {
                    retType.AppendFormat("{0}<{1}>", parentType[0], argList.ToString());
                }
            }
            else
            {
                return type.ToString();
            }

            return retType.ToString().Replace("System.", "");
        }
    }
}