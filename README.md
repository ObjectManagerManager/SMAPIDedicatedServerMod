# SMAPI Dedicated Server Mod for Stardew Valley
This mod provides a dedicated (headless) server for Stardew Valley, powered by SMAPI. It turns the host farmer into an automated bot to facilitate multiplayer gameplay.

## Configuration File
Upon running SMAPI with the mod installed for the first time, a `config.json` file will be generated in the mod's folder. This file specifies which farm will be loaded on startup, farm creation options, host automation details, and other mod configuration options. Default values will be provided, which can then be modified. Here is an overview of the available settings:

### Startup options

- `FarmName`: The name of the farm. If a farm with this name exists, it will automatically be loaded and hosted for co-op. Otherwise, a new farm will be created using the specified farm creation options and then hosted for co-op.

### Farm Creation Options

- `StartingCabins`: The number of starting cabins for the farm. Must be an integer in {0, 1, 2, 3}.
- `CabinLayout`: Specifies the starting cabin layout. Options are "nearby" or "separate".
- `ProfitMargin`: The farm's profit margin. Options are "normal", "75%", "50%", and "25%".
- `MoneyStyle`: Determines whether money is shared or separate among farmers. Options are "shared" or "separate".
- `FarmType`: The type of farm. Options include "standard", "riverland", "forest", "hilltop", "wilderness", "fourcorners", and "beach".
- `CommunityCenterBundles`: The community center bundle type. Options are "normal" or "remixed".
- `GuaranteeYear1Completable`: Set to `true` or `false` to determine if the community center should be guaranteed completable during the first year.
- `MineRewards`: The mine rewards type. Options are "normal" or "remixed".
- `SpawnMonstersOnFarmAtNight`: Set to `true` or `false` to determine if monsters should spawn on the farm at night.
- `RandomSeed`: An optional integer specifying the farm's random seed.

### Host Automation Options

- `AcceptPet`: Set to `true` or `false` to determine if the farm pet should be accepted.
- `PetSpecies`: The desired pet species. Options are "dog" or "cat". Irrelevant if `AcceptPet` is `false`.
- `PetBreed`: An integer in {0, 1, 2} specifying the pet breed index. 0 selects the leftmost breed; 1 selects the middle breed; 2 selects the rightmost breed. Irrelevant if `AcceptPet` is `false`.
- `PetName`: The desired pet name. Irrelevant if `AcceptPet` is `false`.
- `MushroomsOrBats`: Choose between the mushroom or bat cave. Options are "mushrooms" or "bats" (case insensitive).
- `PurchaseJojaMembership`: Set to `true` or `false` to determine if the automated host should "purchase" (acquire for free) a Joja membership when available, committing to the Joja route. Defaults to `false`.

### Additional Options

- `EnableCropSaver`: Set to `true` or `false` to enable or disable the crop saver feature. When enabled, seasonal crops planted by players and fully grown before the season's end are guaranteed to give at least one more harvest before dying. For example, a spring crop planted by a player and fully grown before Summer 1 will not die immediately on Summer 1. Instead, it'll provide exactly one more harvest, even if it's a crop that ordinarily produces multiple harvests. Defaults to `true`.

### Host Options

- `MoveBuildPermission`: Changes farmhands permissions to move buildings from the Carpenter's shop. Is set each time the server is started and can be changed in the game. Set to `off` to entirely disable moving buildings, set to `owned` to allow farmhands to move buildings that they purchased, or set to `on` to allow moving all buildings.

## In Game Command

All commands in the game must be sent privately to the player `ServerBot`. For example, you must write the following `/message ServerBot MoveBuildPermission on`:

- `SafeInviteCode`: A file `invite_code.txt` with the invitation code is created in this mods folder. If there is no invitation code, an empty string is saved.
- `InviteCode`: The invitation code is printed.
- `Sleep`: (Toggle command) \
  When it is sent, the host goes to bed. When all players leave the game or go to bed, the next day begins. On a second send, the host will get up and the mod's normal behavior will be restored.
- `ResetDay`: Kicks all players and restarts the day.
- `Shutdown`: Kicks all players and starts a new day.
- `SpawnMonster`: (Toggle command, Saved in config) \
  Changes the settings so that monsters spawn on the farm or not. Spawned monsters are not reset.
- `MoveBuildPermission` or
- `MovePermissiong` or
- `MBP`: (Saved in config) \
Changes farmhands permissions to move buildings from the Carpenter's shop. Set to `off` to entirely disable moving buildings, set to `owned` to allow farmhands to move buildings that they purchased, or set to `on` to allow moving all buildings.

## Host in Game Command

All these commands only work if you are the host. This allows you to take control of the server. The host sends the commands by entering them directly, without anything before or after:

- `letmeplay`: Lets the player take over the host. All host functions are switched off.
- `takeover`: The player returns control to the host, all host functions are switched on.

## Running the Server on Linux Without GUI

This mod can be run without the use of a GUI. To start the game, you must enter the following command:

```bash
xvfb-run -a "$HOME/GOG Games/Stardew Valley/game/StardewModdingAPI"
```

You can shut down the server from the started terminal session by pressing `Control-C`.
From another terminal session, you can send `Control-C` with `kill -SIGINT ....`.

```bash
ps -aux | grep StardewModdingAPI
kill -SIGINT ....
```

## Development

If Stardew Valley was not installed in the default path, the installation path must be added to the project file `DedicatedServer.csproj`. Add the path with the tag `GamePath` to the `PropertyGroup`. Depending on the path, it should look something like this:

```text
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <GamePath>D:\SteamLibrary\steamapps\common\Stardew Valley</GamePath>
  </PropertyGroup>
```
