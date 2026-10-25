using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SilentWeather
{
    internal static class WeatherVolumePatches
    {
        private static readonly Dictionary<MethodBase, VolumeBinding> _patched
            = new Dictionary<MethodBase, VolumeBinding>();

        internal static VolumeBinding RainBinding;
        internal static VolumeBinding WindBinding;

        public static void ApplyAll(Harmony harmony)
        {
            RainBinding = new VolumeBinding
            {
                MethodName = "GetRainMult",
                ParamCount = 2,
                Volume = SilentWeatherPlugin.RainVolume,
                BlenderTypeName = "Audio.AmbientSubsystem.PrecipitationAmbientBlender",
                CacheMemberNames = new[] { "_prevRainIntensity", "erainIntensity_0" },
                LastSeenValue = SilentWeatherPlugin.RainVolume.Value,
            };

            WindBinding = new VolumeBinding
            {
                MethodName = "GetWindMult",
                ParamCount = 1,
                Volume = SilentWeatherPlugin.WindVolume,
                BlenderTypeName = "Audio.AmbientSubsystem.WindAmbientBlender",
                CacheMemberNames = new[] { "_prevWindSqrMagnitude" },
                LastSeenValue = SilentWeatherPlugin.WindVolume.Value,
            };

            PatchByBinding(harmony, RainBinding);
            PatchByBinding(harmony, WindBinding);

        }

        private static void PatchByBinding(Harmony harmony, VolumeBinding b)
        {
            try
            {
                if (GameRef.GameAssembly == null)
                {
                    SilentWeatherPlugin.Log.LogError(
                        "Assembly-CSharp не загружен — невозможно применить патч. Вы точно запускаете это внутри EFT?");
                    return;
                }

                var matches = GameRef.FindMethods(b.MethodName, b.ParamCount);

                if (matches.Count == 0)
                {
                    SilentWeatherPlugin.Log.LogError(
                        $"Не удалось найти {b.MethodName} - этот ползунок не будет выполнять никаких действий." +
                        "Вероятно, в этой сборке SPT он был переименован; в списке ниже должно быть указано новое название.");
                    GameRef.LogMethodCandidates("Wind");
                    return;
                }

                if (matches.Count > 1)
                {
                    SilentWeatherPlugin.Log.LogError(
                        $"Найдено {matches.Count} кандидатов для {b.MethodName}; отказ от внесения исправлений в неоднозначную цель.");
                    return;
                }

                var postfix = new HarmonyMethod(typeof(WeatherVolumePatches).GetMethod(
                    nameof(UniversalPostfix), BindingFlags.Static | BindingFlags.NonPublic));

                var match = matches[0];
                harmony.Patch(match, postfix: postfix);
                _patched[match] = b;
                SilentWeatherPlugin.Log.LogInfo(
                    $"Patched {match.DeclaringType?.FullName}::{match.Name} " +
                    $"({(match.IsStatic ? "static" : "instance")}, {match.GetParameters().Length} args)");
            }
            catch (Exception e)
            {
                SilentWeatherPlugin.Log.LogError($"Ошибка патча {b.MethodName}: {e}");
            }
        }

        private static void UniversalPostfix(ref float __result, MethodBase __originalMethod)
        {
            VolumeBinding binding;
            if (_patched.TryGetValue(__originalMethod, out binding))
            {
                __result *= binding.Volume.Value;
            }
        }
    }
}