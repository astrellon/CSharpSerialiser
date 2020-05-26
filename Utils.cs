using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace CSharpSerialiser
{
    public static class Utils
    {
        #region Methods
        public static IEnumerable<Type> GetLoadedTypes(Type input)
        {
            // Ugly hack to deal with types from unknown modules.
            try
            {
                return Assembly.GetAssembly(input).GetTypes();
            }
            catch (ReflectionTypeLoadException exp)
            {
                return exp.Types.Where(t => t != null);
            }
        }

        public static IEnumerable<Type> GetLoadedTypes(Module module)
        {
            // Ugly hack to deal with types from unknown modules.
            try
            {
                return module.GetTypes();
            }
            catch (ReflectionTypeLoadException exp)
            {
                return exp.Types.Where(t => t != null);
            }
        }

        public static IEnumerable<Type> GetLoadedTypes(Assembly assembly)
        {
            // Ugly hack to deal with types from unknown modules.
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exp)
            {
                return exp.Types.Where(t => t != null);
            }
        }
        #endregion
    }
}