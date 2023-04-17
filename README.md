# SMAPIDedicatedServerMod
A dedicated (headless) server mod for Stardew Valley, powered by SMAPI, turning the host farmer into an automated bot.

## Config file
After running SMAPI with the mod installed for the first time, a file called "config" will appear in the mod's folder. This file specifies which farm will be loaded on startup, how to create the farm if it does not already exist, details about the host automation, and other mod configuration options. The file will be populated with default values, after which they can be changed. Here is a breakdown of the values:

### Startup options

- FarmName: The name of the farm. If a farm with this name already exists, then it will automatically be loaded and hosted for co-op. If a farm with this name does not exist, then it will be created with a configuration defined by the farm creation options (see next subsection), and then hosted for co-op.

### Farm creation options

- StartingCabins: The number of starting cabins for the farm. Must be an integer in {0, 1, 2, 3}.
- CabinLayout: Specifies how the starting cabins should be laid out. Must be either "nearby" or "separate".
- ProfitMargin: Specifies the farm's profit margin. Options are "normal", "75%", "50%", and "25%".
- MoneyStyle: Specifies whether money should be shared between farmers. Must be either "shared" or "separate".
- FarmType: Specifies the type of farm. Must be "standard", "riverland", "forest", "hilltop", "wilderness", "fourcorners", or "beach".
- CommunityCenterBundles: Specifies the community center bundle type. Must be "normal" or "remixed".
- GuaranteeYear1Completable: True or false, specifying whether or not the community center should be guaranteed to be completable during the first year.
- MineRewards: Specifies the mine rewards type. Must be "normal" or "remixed".
- SpawnMonstersOnFarmAtNight: True or false, specifying whether monsters should spawn on the farm at night.
- RandomSeed: In integer specifying the farm's random seed. Optional.

### Host automation options

- AcceptPet: true or false, specifying whether or not the farm pet should be accepted.
- PetSpecies: Specifies the desired pet species. Must be either "dog" or "cat". This value is irrelevant if AcceptPet is false.
- PetBreed: An integer in {0, 1, 2} specifying the pet breed index. 0 selects the leftmost breed within the pet selection menu during farm creation; 1 selects the middle breed; and 2 selects the rightmost breed. This value is irrelevant if AcceptPet is false.
- PetName: The desired name of the pet. This value is irrelevant if AcceptPet is false.
- MushroomsOrBats: Specifies whether the mushroom or bat cave should be selected. Must be either "mushrooms" or "bats" (case insensitive).
- PurchaseJojaMembership: true or false, specifying whether the automated host should "purchase" (acquire for free) a Joja membership once available, committing to the Joja route. Defaults to false.

### Other options

- EnableCropSaver: true or false, specifying whether the crop saver should be enabled. When enabled, all seasonal crops that are planted by players and fully grown before the turn of the season are guaranteed to give at least one more harvest before dying. For instance, a spring crop planted by a player and fully grown before Summer 1 will not die immediately on Summer 1. Instead, it'll give exactly one more harvest, even if it's a crop that ordinarily produces multiple harvests. Defaults to true.
