using System.Collections.Generic;

namespace DedicatedServer.Config
{
    public class ModConfig
    {
        // Options are 0, 1, 2, or 3.
        public int StartingCabins { get; set; } = 1;

        // Options are "nearby" or "separate"
        public string CabinLayout { get; set; } = "separate";
        
        // Options are "normal", "75%", "50%", or "25%"
        public string ProfitMargin { get; set; } = "normal";

        // Options are "shared" or "separate"
        public string MoneyStyle { get; set; } = "shared";

        public string FarmName { get; set; } = "Stardew";

        public bool AcceptPet = true; // By default, accept the pet (of course).
        
        // Nullable. Must not be null if AcceptPet is true. Options are "dog" or "cat".
        public string PetSpecies { get; set; } = "dog";

        // Nullable. Must not be null if AcceptPet is true. Options are 0, 1, or 2.
        public int? PetBreed { get; set; } = 0;

        // Nullable. Must not be null if AcceptPet is true. Any string.
        public string PetName { get; set; } = "Stella";

        // Options are "Mushrooms" or "Bats" (case-insensitive)
        public string MushroomsOrBats { get; set; } = "Mushrooms";

        // Options are "standard", "riverland", "forest", "hilltop", "wilderness", "fourcorners", "beach".
        public string FarmType { get; set; } = "standard";

        // Options are "normal" or "remixed".
        public string CommunityCenterBundles { get; set; } = "normal";
        
        public bool GuaranteeYear1Completable { get; set; } = false;

        // Options are "normal" or "remixed".
        public string MineRewards { get; set; } = "normal";

        public bool SpawnMonstersOnFarmAtNight { get; set; } = false;

        public ulong? RandomSeed { get; set; } = null;
    }
}
