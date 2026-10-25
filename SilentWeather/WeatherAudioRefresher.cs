using System;
using System.Linq;
using System.Reflection;
using Il2CppInterop.Runtime;

namespace SilentWeather
{
    internal static class WeatherAudioRefresher
    {
        private const BindingFlags AnyInstance =
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public static void Refresh(VolumeBinding b)
        {
            if (b.BlenderTypeName == null) return;

            try
            {
                if (!Resolve(b)) return;
                if (b.RefreshMethod == null)
                {
                    b.RefreshMethod = typeof(WeatherAudioRefresher)
                        .GetMethod(nameof(RefreshTyped),
                            BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(b.BlenderType);
                }
                b.RefreshMethod.Invoke(null, new object[] { b });
            }
            catch (Exception e)
            {
                SilentWeatherPlugin.Log.LogWarning(
                    b.MethodName + " ошибка обновления: " + e.GetBaseException().Message);
            }
        }

        private static void RefreshTyped<TBlender>(VolumeBinding b)
            where TBlender : UnityEngine.Object
        {
            var blenders = UnityEngine.Object.FindObjectsByType<TBlender>(
                UnityEngine.FindObjectsInactive.Include,
                UnityEngine.FindObjectsSortMode.None);

            var invalid = MakeInvalidValue(b.CacheMemberType);

            foreach (var blender in blenders)
            {
                if (blender == null) continue;

                if (b.CacheMember is PropertyInfo prop)
                    prop.SetValue(blender, invalid);
                else
                    ((FieldInfo)b.CacheMember).SetValue(blender, invalid);
            }

            SilentWeatherPlugin.Log.LogDebug(string.Format(
            "{0} refresh: {1} blender(s) via {2}.{3}, mult={4}",
                b.MethodName, blenders.Length, b.BlenderType.Name, b.CacheMember.Name, b.Volume.Value));
        }

        private static bool Resolve(VolumeBinding b)
        {
            if (b.BlenderType != null && b.CacheMember != null) return true;
            if (b.ResolveAttempted) return false;
            b.ResolveAttempted = true;

            var type = GameRef.FindType(b.BlenderTypeName);
            if (type == null)
            {
                WarnOnce(b, b.BlenderTypeName + " not found");
                GameRef.LogTypeCandidates("AmbientBlender");
                return false;
            }

            var member = FindCacheMember(type, b);
            if (member == null)
            {
                WarnOnce(b, string.Format(
                    "no cache member on {0}. Fields: [{1}]; Properties: [{2}]",
                    type.Name,
                    string.Join(", ", type.GetFields(AnyInstance).Select(f => f.Name).Take(30).ToArray()),
                    string.Join(", ", type.GetProperties(AnyInstance).Select(p => p.Name).Take(30).ToArray())));
                return false;
            }

            b.BlenderType = type;
            b.CacheMember = member;
            b.CacheMemberType = member is PropertyInfo p ? p.PropertyType : ((FieldInfo)member).FieldType;
            return true;
        }

        private static MemberInfo FindCacheMember(Type type, VolumeBinding b)
        {
            foreach (var name in b.CacheMemberNames)
            {
                var prop = type.GetProperty(name, AnyInstance);
                if (prop != null) return prop;

                var fld = type.GetField(name, AnyInstance);
                if (fld != null) return fld;
            }
            return null;
        }

        private static object MakeInvalidValue(Type t)
        {
            if (t == null) return null;

            if (t.IsEnum)
            {
                var u = Enum.GetUnderlyingType(t);
                if (u == typeof(byte)) return Enum.ToObject(t, (byte)255);
                if (u == typeof(sbyte)) return Enum.ToObject(t, (sbyte)-1);
                if (u == typeof(short)) return Enum.ToObject(t, (short)-1);
                if (u == typeof(ushort)) return Enum.ToObject(t, (ushort)65535);
                if (u == typeof(uint)) return Enum.ToObject(t, uint.MaxValue);
                if (u == typeof(long)) return Enum.ToObject(t, long.MinValue);
                if (u == typeof(ulong)) return Enum.ToObject(t, ulong.MaxValue);
                return Enum.ToObject(t, -1);
            }

            if (t == typeof(float)) return -1f;
            if (t == typeof(double)) return -1d;
            if (t == typeof(int)) return -1;
            if (t == typeof(long)) return -1L;
            if (t == typeof(short)) return (short)-1;
            if (t == typeof(byte)) return (byte)255;
            if (t == typeof(bool)) return true;

            return null;
        }

        private static void WarnOnce(VolumeBinding b, string detail)
        {
            if (b.Warned) return;
            b.Warned = true;
            SilentWeatherPlugin.Log.LogWarning(
                detail + " - " + b.MethodName + " Изменения вступят в силу после следующей проверки погоды." +
                "переход между помещением и улицей происходит постепенно, а не мгновенно. На саму регулировку громкости это не влияет.");
        }
    }
}