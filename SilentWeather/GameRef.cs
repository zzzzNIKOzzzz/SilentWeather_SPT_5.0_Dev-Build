using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilentWeather
{
    internal static class GameRef
    {
        private static Assembly _assembly;
        private static Type[] _types;

        public static Assembly GameAssembly
        {
            get { return _assembly ?? (_assembly = FindGameAssembly()); }
        }

        public static Type[] Types
        {
            get { return _types ?? (_types = LoadTypes()); }
        }

        private static Assembly FindGameAssembly()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
        }

        private static Type[] LoadTypes()
        {
            if (GameAssembly == null)
            {
                return new Type[0];
            }

            try
            {
                return GameAssembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).ToArray();
            }
        }

        public static Type FindType(string fullName)
        {
            var simpleName = fullName.Substring(fullName.LastIndexOf('.') + 1);
            return Types.FirstOrDefault(t => t.FullName == fullName)
                   ?? Types.FirstOrDefault(t => t.Name == simpleName);
        }

        public static List<MethodInfo> FindMethods(string name, int paramCount)
        {
            return AllMethods()
                .Where(m => m.Name == name
                            && m.ReturnType == typeof(float)
                            && m.GetParameters().Length == paramCount)
                .ToList();
        }

        public static IEnumerable<MethodInfo> AllMethods()
        {
            return Types.SelectMany(SafeMethods);
        }

        private static IEnumerable<MethodInfo> SafeMethods(Type type)
        {
            try
            {
                return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic
                                       | BindingFlags.Instance | BindingFlags.Static
                                       | BindingFlags.DeclaredOnly);
            }
            catch
            {
                return Enumerable.Empty<MethodInfo>();
            }
        }

        public static void LogMethodCandidates(string keyword)
        {
            try
            {
                var near = AllMethods()
                    .Where(m => m.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                                && m.ReturnType == typeof(float))
                    .Select(m => string.Format("{0}::{1}({2})",
                        m.DeclaringType != null ? m.DeclaringType.FullName : "?",
                        m.Name,
                        m.GetParameters().Length))
                    .Distinct()
                    .Take(20)
                    .ToList();

                SilentWeatherPlugin.Log.LogWarning(near.Count == 0
                    ? string.Format("No float-returning methods contain '{0}' at all.", keyword)
                    : string.Format("Float-returning methods containing '{0}': {1}",
                        keyword, string.Join(" | ", near.ToArray())));
            }
            catch (Exception e)
            {
                SilentWeatherPlugin.Log.LogWarning("Ошибка дампа кандидата: " + e.Message);
            }
        }

        public static void LogTypeCandidates(string keyword)
        {
            try
            {
                var near = Types
                    .Where(t => t.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(t => t.FullName)
                    .Take(20)
                    .ToList();

                SilentWeatherPlugin.Log.LogWarning(near.Count == 0
                    ? string.Format("No types contain '{0}' at all.", keyword)
                    : string.Format("Types containing '{0}': {1}", keyword, string.Join(" | ", near.ToArray())));
            }
            catch (Exception e)
            {
                SilentWeatherPlugin.Log.LogWarning("Ошибка при создании дампа: " + e.Message);
            }
        }
    }
}
