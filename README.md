# Risk of Raindrops

Collection of mods for Risk of Rain 2 compatible with BepInEx.

  - DropItems
  - StatsDisplay

## DropItems

  - Drop items / equipment by clicking on their icon.
  - There will be a notification that you dropped the item.

![DropItemsMod](https://github.com/kookehs/risk-of-raindrops/blob/master/images/drop-items.png)

## StatsDisplay

  - Displays character stats on the Info Screen.
  - Configurable with properties on CharacterBody.

![Stats Display Mod](https://github.com/kookehs/RiskOfRain2Mods/blob/master/images/stats-display.png)

### Configure

Edit Risk of Rain 2/BepInEx/config/com.kookehs.statsdisplay.cfg

```
[Display]

# Text to display for the title.
Title = STATS

# A comma-separated list of stats to display based on CharacterBody properties.
StatsToDisplay = crit,damage,attackSpeed,armor,regen,moveSpeed,maxJumpCount,experience

# A comma-separated list of names for the stats.
StatsToDisplayNames = Crit,Damage,Attack Speed,Armor,Regen,Move Speed,Jump Count,Experience

# The X position as percent of screen width of the stats display.
X = 10

# The Y position as percent of screen height of the stats display.
Y = 35

# The width of the stats display.
Width = 250

# The height of the stats display.
Height = 250
```

## Download
  - [Stable Builds On Thunderstore](https://thunderstore.io/package/kookehs/)
  - [Nightly Builds On GitHub](https://github.com/kookehs/risk-of-raindrops/releases)

## Installation

  - [Install BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/)
  - Place DLLs in Risk of Rain 2/BepInEx/plugins
