# Hollow Knight HKMP Mod

A mod that synchronizes mob and boss health from the host to all players in a co-op session, and gives soul to all players when any player attacks mobs.

## Features

- **Enemy HP sync** — hitting an enemy on one screen damages it on everyone's screen
- **Shared deaths** — killing an enemy kills it for everyone simultaneously  
- **Shared soul gain** — everyone earns soul when any player kills an enemy
- **Boss health sync** — boss health is synchronized from host to all players

## Installation

1. Ensure **Modding API** and **HKMP** are already installed
2. Download the compiled mod DLL
3. Create this folder if it doesn't exist:
   ```
   Hollow Knight\hollow_knight_Data\Managed\Mods\HollowKnightHKMPMod\
   ```
4. Drop the `HollowKnightHKMPMod.dll` into that folder
5. Launch the game

## Usage

- When you join a co-op session, all players will see the same enemy health
- When any player attacks an enemy, all players will gain soul
- Boss health is synchronized from the host to all players in real-time

## Compatibility

- Compatible with HK 1.5.78.11833
- Requires Modding API 74+ and HKMP 2.x

## License

MIT