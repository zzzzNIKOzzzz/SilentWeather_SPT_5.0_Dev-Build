using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace SilentWeather
{
    public class UpdateTicker : MonoBehaviour
    {
        private const float SettleSeconds = 0.25f;

        private static UpdateTicker _instance;

        private readonly List<VolumeBinding> _pending = new List<VolumeBinding>();

        private float _lastChangeAt;

        public UpdateTicker(IntPtr ptr) : base(ptr) { }

        public void Awake()
        {
            _instance = this;
        }

        [HideFromIl2Cpp]
        internal static void RequestChange(VolumeBinding b)
        {
            if (_instance == null) return;
            if (!_instance._pending.Contains(b))
                _instance._pending.Add(b);
            _instance._lastChangeAt = Time.unscaledTime;
        }

        public void Update()
        {
            if (_pending.Count == 0) return;
            if (Time.unscaledTime - _lastChangeAt < SettleSeconds) return;

            foreach (var b in _pending)
                WeatherAudioRefresher.Refresh(b);
            _pending.Clear();
            SilentWeatherPlugin.SaveConfig();
        }

        public void OnDestroy()
        {
            if (_pending.Count > 0)
                SilentWeatherPlugin.SaveConfig();
            _instance = null;
        }
    }
}