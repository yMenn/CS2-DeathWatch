# CS2-DeathWatch Plugin (WIP)

Welcome to **DeathWatch**, a Counter-Strike plugin designed specifically with the **Jailbreak gamemode** in mind. This plugin tracks player deaths and allows staff members to respawn them, providing tools to rectify mass death events such as mass freekills or accidental deaths in a non intrusive way. Although it’s my **first plugin**, I’m excited to share it and continue improving it based on feedback!

## 🚧 Status: Work In Progress

This plugin is **not fully complete**, but I’m actively working on it. I will keep fixing reported issues as they come up. Since this is my first plugin, I’m open to **critiques** and **tips** to make it better!

## 🎯 Features

- **Death Tracking**: Track deaths that happen during gameplay. While not too detailed, it provides essential information to admins, including a **configurable delay** between deaths that determines whether they are counted as part of the same event.
- **Respawn Controls**: The core feature of this plugin is the ability for staff to instantly respawn the last few players who died, eliminating the need to ask "who died?" and manually respawn each player by name. Staff can quickly respawn groups of players based on recent death events with a single command.
- **Mass Death Detection**: Automatically detects mass death events in-game, with a configurable number of deaths required to trigger an alert. These alerts are sent **only to staff** so they can take appropriate actions.
- **Non-Intrusive Design**: Players won’t be affected unless respawned by staff. The plugin operates quietly in the background.
- **Circle Drawing Utility**: Provides visual indicators for player locations or events using in-game circles. You can configure whether or not to **draw player markers** on the map for visibility.
- **Configurable Plugin Tag**: Customize the plugin tag that appears in chat messages to match your server branding.

## ⚙️ Commands

Here’s a list of available commands for staff:

| Command|Description|
|-----------------------|-----------------------------------------------------|
| `!deathwatch`         | Displays available commands and help information.|
| `!listevents`         | Lists the recent death events recorded in the current round.|
| `!eventinfo <event_id>`| Shows the details of a specific death event by event ID.|
| `!sr <count>`         | Respawns the last `<count>` players who died.|
| `!srspot <count>`     | Respawns the last `<count>` players at the spot where they died.|
| `!srhere <count>`     | Respawns the last `<count>` players who died at your current location.|
| `!er [event_id]`      | Respawns players from a death event at their last spawn location. Defaults to the **latest death event** if no `event_id` is provided. |
| `!erspot [event_id]`  | Respawns players from a death event at the location where they died. Defaults to the **latest death event** if no `event_id` is provided. |
| `!erhere [event_id]`  | Respawns players from a death event at your current location. Defaults to the **latest death event** if no `event_id` is provided. |

## 🛣️ ROADMAP

Here are some planned features and improvements for the DeathWatch plugin:

- **WASD Menu for Managing the Plugin**: Implement a user-friendly WASD-based menu for staff to easily manage plugin settings and features without needing to type commands.
- **Improved Circle Drawing**: Attempt to draw the circle with the same radius as the player model to accurately show where the player was located and where their body stretched upon death.
- **More Detailed Death Logging**: Enhance the plugin's ability to log more specific details about player deaths, providing staff with better insights into in-game events.
- **Overall Optimizations**: Search for and implement performance improvements to ensure the plugin runs smoothly and efficiently.

## 🚀 How to Install

1. Download the latest release from the releases section.
2. Place the plugin folder in `csgo/addons/counterstrikesharp/plugins`
3. After first startup, change config in `csgo/addons/counterstrikesharp/configs/plugins/DeathWatch/DeathWatch.json`
4. Restart your server or reload plugin to apply the changes.

## 💬 Feedback and Contributions

Since this is my **first plugin**, I’m eager to hear your thoughts! If you spot any issues, have feature requests, or just want to give feedback, feel free to open an issue or pull request. I’m also open to **suggestions** and **tips** to make this plugin even better!

---

Thanks for checking out DeathWatch! Your feedback will help make it great.
