using System.Collections.Generic;

namespace DedicatedServer.Config
{
    public class ModConfig
    {
        public string FarmName { get; set; } = "Stardew";

        // Options are 0, 1, 2, or 3.
        public int StartingCabins { get; set; } = 1;

        // Options are "nearby" or "separate"
        public string CabinLayout { get; set; } = "separate";
        
        // Options are "normal", "75%", "50%", or "25%"
        public string ProfitMargin { get; set; } = "normal";

        // Options are "shared" or "separate"
        public string MoneyStyle { get; set; } = "shared";

        // Options are "standard", "riverland", "forest", "hilltop", "wilderness", "fourcorners", "beach".
        public string FarmType { get; set; } = "standard";

        // Options are "normal" or "remixed".
        public string CommunityCenterBundles { get; set; } = "normal";
        
        public bool GuaranteeYear1Completable { get; set; } = false;

        // Options are "normal" or "remixed".
        public string MineRewards { get; set; } = "normal";

        public bool SpawnMonstersOnFarmAtNight { get; set; } = false;

        public ulong? RandomSeed { get; set; } = null;

        public bool AcceptPet = true; // By default, accept the pet (of course).
        
        // Nullable. Must not be null if AcceptPet is true. Options are "dog" or "cat".
        public string PetSpecies { get; set; } = "dog";

        // Nullable. Must not be null if AcceptPet is true. Options are 0, 1, or 2.
        public int? PetBreed { get; set; } = 0;

        // Nullable. Must not be null if AcceptPet is true. Any string.
        public string PetName { get; set; } = "Stella";

        // Options are "Mushrooms" or "Bats" (case-insensitive)
        public string MushroomsOrBats { get; set; } = "Mushrooms";

        // Enables the crop saver
        public bool EnableCropSaver = true;

        // Configures the automated host to purchase a Joja membership once available,
        // committing to the Joja route and removing the community center.
        public bool PurchaseJojaMembership = false;

        // Changes farmhands permissions to move buildings from the Carpenter's Shop.
        // Is set each time the server is started and can be changed in the game.
        // "off" to entirely disable moving buildings.
        // "owned" to allow farmhands to move buildings that they purchased.
        // "on" to allow moving all buildings.
        public string MoveBuildPermission { get; set; } = "off";
    }
}
