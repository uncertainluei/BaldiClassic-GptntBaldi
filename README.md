![Version](https://img.shields.io/badge/version-0.1-purple) ![GitHub License](https://img.shields.io/github/license/uncertainluei/BaldiClassic-GptntBaldi)
![BBiEaL version](https://img.shields.io/badge/bbieal-1.4.3-69C12E?color=green) ![BepInEx version](https://img.shields.io/badge/bepinex-5.4.23-69C12E?color=yellow&link=https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.3)

Ever wished an AI made changes to your [Baldi's Basics Classic](https://basically-games.itch.io/baldis-basics) gameplay? No? Due to the noteriety of AI models and *those who use it to make entire mods*, this mod is instead made by a human replicating the changes the AI would've tried to make.

# Changes

## Characters
- **Principal of the Thing**: Now gives you the maximum possible detention time regardless of repeated offences.
- **Playtime**: Now requires ten jumps to complete her game. Rope speed and delay are varied for each jump. Failing twice prematurely ends the game, but makes a sound Baldi can hear!
- **It's a Bully**: Now spawns sooner, and if possible, near the player. Losing an item to the Bully will also teleport you to a random location after five seconds.

# Build Instructions
This is for building the mod's .DLL and .PDB, which should be found at the `Source/bin/Debug*/net35/` directory.

\*`Release` if built with the *Release* configuration

### Terminal
Make sure you have the [.NET SDK](https://dotnet.microsoft.com/en-us/download) installed. Open your terminal on the cloned/downloaded repository's "Source" subdirectory, and execute:

`dotnet build`

This will build to the *Debug* configuration by default, append `-c Release` if you want to built it with the *Release* configuration.
