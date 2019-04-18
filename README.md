# Risk of Raindrops

Collection of mods for Risk of Rain 2 compatible with BepInEx.

  - DropItems
  - StatsDisplay

## Download
  - [Stable Builds On Thunderstore](https://thunderstore.io/package/kookehs/)
  - [Nightly Builds On GitHub](https://github.com/kookehs/risk-of-raindrops/releases)

## Installation

  - [Install BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/)
  - Place DLLs in Risk of Rain 2/BepInEx/plugins

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
CharacterBodyStats = crit,damage,attackSpeed,armor,regen,moveSpeed,maxJumpCount,experience

# A comma-separated list of names for the CharacterBody stats.
CharacterBodyStatsNames = Crit,Damage,Attack Speed,Armor,Regen,Move Speed,Jump Count,Experience

# A comma-separated list of stats to display based on StatSheet fields.
StatSheetStats = totalKills,totalDamageDealt,goldCollected,totalStagesCompleted

# A comma-separated list of names for the StatSheet stats.
StatSheetStatsNames = Kills,Damage Dealt,Gold Collected,Stages Completed

# The font size of the title.
TitleFontSize = 18

# The font size of the description.
DescriptionFontSize = 14

# The X position as percent of screen width of the stats display.
X = 10

# The Y position as percent of screen height of the stats display.
Y = 35

# The width of the stats display.
Width = 250

# The height of the stats display.
Height = 250

# Whether the stats display always shows or only on Info Screen.
Persistent = false
```

Available properties on CharacterBody

||||||
| --- | --- | --- | --- | --- |
| master  | inventory  | isPlayerControlled | masterObject | teamComponent |
| healthComponent | equipmentSlot | modelLocator | hurtBoxGroup | mainHurtBox |
| coreTransform | isSprinting | outOfDanger | experience | level |
| maxHealth | regen | maxShield | moveSpeed | acceleration |
| jumpPower | maxJumpCount | maxJumpHeight | damage | attackSpeed |
| crit | armor | critHeal | shouldAim | warCryReady |
| bestFitRadius | spreadBloomAngle | multiKillCount | corePosition | footPosition |
| radius | aimOrigin | isElite | isBoss |  |

Available fields on StatDef
||||||
| --- | --- | --- | --- | --- |
| totalGamesPlayed | totalTimeAlive | totalKills | totalDeaths | totalDamageDealt |
| totalDamageTaken | totalHealthHealed | highestDamageDealt | highestLevel | goldCollected |
| maxGoldCollected | totalDistanceTraveled | totalItemsCollected | highestItemsCollected | totalStagesCompleted |
| highestStagesCompleted | totalPurchases | highestPurchases | totalGoldPurchases | highestGoldPurchases |
| totalBloodPurchases | highestBloodPurchases | totalLunarPurchases | highestLunarPurchases | totalTier1Purchases |
| highestTier1Purchases | totalTier2Purchases | highestTier2Purchases | totalTier3Purchases | highestTier3Purchases |
| totalDronesPurchased | totalGreenSoupsPurchased | totalRedSoupsPurchased | suicideHermitCrabsAchievementProgress | firstTeleporterCompleted |
