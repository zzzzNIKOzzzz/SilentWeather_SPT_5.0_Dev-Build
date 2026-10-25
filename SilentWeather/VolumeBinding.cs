using System;
using System.Reflection;
using BepInEx.Configuration;

namespace SilentWeather
{
    internal sealed class VolumeBinding
    {
        public string MethodName;
        public int ParamCount;
        public ConfigEntry<float> Volume;

        public string BlenderTypeName;
        public string[] CacheMemberNames;

        internal Type BlenderType;
        internal MemberInfo CacheMember;
        internal Type CacheMemberType;
        internal MethodInfo RefreshMethod;   
        internal bool ResolveAttempted;
        internal bool Warned;
        internal float LastSeenValue;
    }
}