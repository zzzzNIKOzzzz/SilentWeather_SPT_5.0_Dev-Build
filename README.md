![SPT](https://img.shields.io/badge/SPT-5.0.0-blue?style=flat-square)
![EFT](https://img.shields.io/badge/EFT-1.1.5.1.47510-lightgrey?style=flat-square)
![BepInEx](https://img.shields.io/badge/BepInEx-6.0.0--be.788-orange?style=flat-square)
![IL2CPP](https://img.shields.io/badge/Runtime-IL2CPP-purple?style=flat-square)
![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?style=flat-square)
![License](https://img.shields.io/badge/License-Custom%20%2F%20SPT--only-red?style=flat-square)
![Status](https://img.shields.io/badge/Status-Working-brightgreen?style=flat-square)

# SilentWeather

Плагин для **SPT 5.0** (EFT 1.1.5.x), который даёт независимое управление громкостью дождя и ветра в рейде. Два слайдера в F12, изменения слышны сразу — без перезахода и без ожидания смены погоды.

## Возможности

- 🌧 **Громкость дождя** — от ванильной до полной тишины.
- 💨 **Громкость ветра** — то же самое, отдельный слайдер.
- ⚡ **Применяется мгновенно** — двигаете ползунок, звук меняется в течение ~0.25 с, включая помещение/улицу.
- 💾 **Настройки сохраняются** между запусками.
- 🛡 **Не ломает ничего, кроме громкости** — постфикс умножает результат, а не подменяет логику игры.

## Установка

1. Скопируйте `SilentWeather.dll` в `BepInEx/plugins/`.
2. Запустите игру, откройте F12 — секция **SilentWeather**.

Настройки в `BepInEx/config/com.zzzNIKOzzz.SilentWeather.cfg`.

## Конфигурация

| Параметр | По умолчанию | Диапазон | Что делает |
|---|---|---|---|
| **Громкость → Шум дождя** | `0.5` | `0.0` – `1.0` | Множитель громкости дождя. `1.0` — ванильная, `0.0` — полная тишина. |
| **Громкость → Шум ветра** | `1.0` | `0.0` – `1.0` | Множитель громкости ветра. `1.0` — ванильная, `0.0` — полная тишина. |

По умолчанию дождь приглушён вдвое (0.5), ветер — на ванили (1.0). Оба значения можно менять прямо в рейде.

## Совместимость

- **SPT:** 5.0.0
- **EFT:** 1.1.5.1.47510
- **BepInEx:** 6.0.0-be (IL2CPP)
- **Harmony:** HarmonyX (`0Harmony.dll`)

## Как это работает (для любопытных)

Громкость дождя и ветра в EFT считается в `ClientAudioSettings.SeasonAmbientSettings.GetRainMult` и `GetWindMult` соответственно. Плагин ставит на них Harmony-постфикс, который умножает возвращаемое значение на слайдер.
Но игра кеширует результат и пересчитывает его только в момент кроссфейда — при смене погоды или переходе через дверь. Это могло бы значить «ползунок двигается, а звук стоит». Поэтому плагин дополнительно инвалидирует кеш у активных блендеров (`PrecipitationAmbientBlender._prevRainIntensity`, `WindAmbientBlender._prevWindSqrMagnitude`), и игровой `ManualUpdate` в следующий тик сам запускает кроссфейд на новую громкость — родной, плавный, без хаков.

## Лицензия

MIT — делайте что хотите, только не продавайте.

## Благодарности

- **SPT team** — за сам проект.
- **BepInEx / HarmonyX / Il2CppInterop** — за инфраструктуру, на которой всё держится.
- Сообществу SPT-модеров — за то, что документация и примеры вообще существуют.

## Автор
zzzNIKOzzz

## Оригинальная идея:
[Quiet Weather](https://sp-mod.com/mod/2968/quiet-weather) мод от [wade](https://sp-mod.com/user/122654/wade)  (версия для SPT 4.1.6 / Mono)

## Лицензия:
MIT — делай что хочешь, просто не забудь упомянуть автора.
<img width="2560" height="1440" alt="2026-10-25_21-40-08" src="https://github.com/user-attachments/assets/fcb1202d-c166-4b37-b5d4-ba8b8af74e5f" />
