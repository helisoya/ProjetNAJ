# ProjetNAJ

Repository for the game codenamed NAJ.
It is a Point & Click / Visual Novel Hybrid about solving cases and mysteries.
Uses the Custom made ANF Framework for Unity.

ANF Code in Assets/ANF/Scripts
NAJ Code in Assets/NAJ/Scripts

## ANF

ANF is a modular framework composed of 4 separate parts :
- Global Data are persistent data (between scenes) that are global to all save files (e.g. Screen Settings)
- Player Data are persistent data that are local to a single save files (e.g. Player Name)
- World Components are components that handles 3D elements in scenes (e.g. Backgrounds, Characters, etc)
- GUI Components are components that handles UI elements in scenes (e.g. Pause menu, dialog window, etc)

You can add, remove, or create new component / data container easily. A new component / data container needs to fill in how saving / loading works for it, as well as how it works in general.

To control components and data containers during gameplay, you can use a custom scripting language named ANSL.
You can also add / remove new "Functions" (e.g. SetPlayerName, If, Switch, SetBackground) by creating new "ANSLFunction" classes and registering them in the systems.
"ANSLFunction" have a default compiling scheme that can be overwritten if the function is more complex and span multiple lines (e.g. If).
.ansl files are compiled to a .txt file readable by Unity. This file will then be interpreted during gameplay.

I talk about ANF in more details in this devlog : https://helisoya.itch.io/traveling-to-woolokii/devlog/1683462/devlog-4-anf-the-power-of-scripting
