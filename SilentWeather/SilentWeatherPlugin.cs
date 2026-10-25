using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace SilentWeather
{
    [BepInPlugin(PluginGuid, "SilentWeather", PluginVersion)]
    public class SilentWeatherPlugin : BasePlugin
    {
        public const string PluginGuid = "com.zzzNIKOzzz.SilentWeather";
        public const string PluginVersion = "1.0.0";

        internal static ConfigEntry<float> RainVolume;
        internal static ConfigEntry<float> WindVolume;
        internal static new ManualLogSource Log;
        internal static SilentWeatherPlugin Instance;

        public override void Load()
        {
            Instance = this;
            Log = base.Log;

            RainVolume = Config.Bind(
        "Громкость", "Шум дождя", 0.5f,
        new ConfigDescription(
            "Множитель для цикла имитации дождя в помещении и на улице. 1.0 — стандартный вариант, 0.0 — без звука.",
            new AcceptableValueRange<float>(0f, 1f)));

            WindVolume = Config.Bind(
                "Громкость", "Шум ветра", 1.0f,
                new ConfigDescription(
                    "Множитель для цикла имитации ветра в помещении и на улице. 1.0 — стандартный вариант, 0.0 — без звука.",
                    new AcceptableValueRange<float>(0f, 1f)));

            Config.SaveOnConfigSet = false;
            Config.SettingChanged += OnSettingChanged;

            WeatherVolumePatches.ApplyAll(new Harmony(PluginGuid));

            ClassInjector.RegisterTypeInIl2Cpp<UpdateTicker>();
            AddComponent<UpdateTicker>();

            Log.LogInfo("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
            Log.LogInfo($"SilentWeather {PluginVersion} загружен. Наведём здесь тишины");
            Log.LogInfo("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            CheckAndQueue(WeatherVolumePatches.RainBinding);
            CheckAndQueue(WeatherVolumePatches.WindBinding);
        }

        private static void CheckAndQueue(VolumeBinding b)
        {
            if (b == null) return;
            var v = b.Volume.Value;
            if (Math.Abs(v - b.LastSeenValue) < 1e-5f) return;

            b.LastSeenValue = v;
            UpdateTicker.RequestChange(b);
        }

        internal static void SaveConfig()
        {
            try
            {
                Instance.Config.Save();
            }
            catch (Exception e)
            {
                Log.LogWarning("Невозможно сохранить конфигурацию: " + e.Message);
            }
        }
    }
}