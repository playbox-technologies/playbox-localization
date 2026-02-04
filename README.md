# Playbox Localization

**Playbox Localization** is a lightweight localization system for Unity projects within the Playbox ecosystem.  
It is designed for fast integration, convenient text management, and scalable use in mobile games.

The package solves common localization problems:
- a single source of truth for translations;
- easy access to localized strings from code and UI;
- support for multiple languages;
- predictable and safe behavior when keys or translations are missing.

---

## Features

- 🌍 Multi-language support  
- 🧩 Easy Unity integration  
- 🧠 String access from C# code  
- 🖥 UI integration (Text / TMP)  
- 🔄 Runtime language switching  
- 🧪 Safe fallbacks for missing keys  
- 📦 Unity Package Manager (UPM) support  

---

## Requirements

- **Unity**: `2021.3 LTS` or newer  
- **.NET**: `Standard 2.1`  
- **UI**:
  - `UnityEngine.UI.Text`
  - `TextMeshPro (TMP_Text)`

---

## Installation

### Via UPM (recommended)

Add the package to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.playbox.localization": "https://github.com/playbox-technologies/playbox-localization.git"
  }
}
