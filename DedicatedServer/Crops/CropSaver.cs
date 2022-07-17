using DedicatedServer.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DedicatedServer.Crops
{
    public class CropSaver
    {
        private IModHelper helper;
        private IMonitor monitor;
        private ModConfig config;
        private SerializableDictionary<CropLocation, CropData> cropDictionary = new SerializableDictionary<CropLocation, CropData>();
        private SerializableDictionary<CropLocation, CropComparisonData> beginningOfDayCrops = new SerializableDictionary<CropLocation, CropComparisonData>();
        private XmlSerializer cropSaveDataSerializer = new XmlSerializer(typeof(CropSaveData));

        public struct CropSaveData
        {
            public SerializableDictionary<CropLocation, CropData> cropDictionary { get; set; }
            public SerializableDictionary<CropLocation, CropComparisonData> beginningOfDayCrops { get; set; }
        }

        public struct CropLocation
        {
            public string LocationName { get; set; }
            public int TileX { get; set; }
            public int TileY { get; set; }
        }

        public struct CropGrowthStage
        {
            public int CurrentPhase { get; set; }
            public int DayOfCurrentPhase { get; set; }
        }

        public struct CropComparisonData
        {
            public CropGrowthStage CropGrowthStage { get; set; }
            public bool Dead { get; set; }
            public bool ForageCrop { get; set; }
            public int WhichForageCrop { get; set; }
        }

        public struct CropData
        {
            public CropGrowthStage CropGrowthStage { get; set; }
            public bool MarkedForDeath { get; set; }
            public List<string> OriginalSeasonsToGrowIn { get; set; }
            public bool HasExistedInIncompatibleSeason { get; set; }
        }

        public CropSaver(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.config = config;
        }

        public void Enable()
        {
            helper.Events.GameLoop.DayStarted += onDayStarted;
            helper.Events.GameLoop.DayEnding += onDayEnding;
            helper.Events.GameLoop.Saving += onSaving;
            helper.Events.GameLoop.SaveLoaded += onLoaded;
        }

        private void onLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            /**
             * Loads the cropDictionary and beginningOfDayCrops.
             */
            string str = SaveGame.FilterFileName(Game1.GetSaveGameName());
            string filenameNoTmpString = str + "_" + Game1.uniqueIDForThisGame;
            string save_directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves", filenameNoTmpString + Path.DirectorySeparatorChar);
            if (Game1.savePathOverride != "")
            {
                save_directory = Game1.savePathOverride;
            }
            string saveFile = Path.Combine(save_directory, "AdditionalCropData");

            // Deserialize crop data from temp save file
            Stream fstream = null;
            try
            {
                fstream = new FileStream(saveFile, FileMode.Open);
                CropSaveData cropSaveData = (CropSaveData)cropSaveDataSerializer.Deserialize(fstream);
                fstream.Close();
                beginningOfDayCrops = cropSaveData.beginningOfDayCrops;
                cropDictionary = cropSaveData.cropDictionary;
            } catch (IOException)
            {
                fstream?.Close();
            }
        }

        private void onSaving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            /**
             * Saves the cropDictionary and beginningOfDayCrops. In most cases, the day is started
             * immediately after loading, which in-turn clears beginningOfDayCrops. However, in case
             * some other mod is installed which allows mid-day saving and loading, it's a good idea
             * to save both dictionaries anyways.
             */

            // Determine save paths
            string tmpString = "_STARDEWVALLEYSAVETMP";
            bool save_backups_and_metadata = true;
            string str = SaveGame.FilterFileName(Game1.GetSaveGameName());
            string filenameNoTmpString = str + "_" + Game1.uniqueIDForThisGame;
            string filenameWithTmpString = str + "_" + Game1.uniqueIDForThisGame + tmpString;
            string save_directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves", filenameNoTmpString + Path.DirectorySeparatorChar);
            if (Game1.savePathOverride != "")
            {
                save_directory = Game1.savePathOverride;
                if (Game1.savePathOverride != "")
                {
                    save_backups_and_metadata = false;
                }
            }
            SaveGame.ensureFolderStructureExists();
            string tmpSaveFile = Path.Combine(save_directory, "AdditionalCropData" + tmpString);
            string saveFile = Path.Combine(save_directory, "AdditionalCropData");
            string backupSaveFile = Path.Combine(save_directory, "AdditionalCropData_old");

            // Serialize crop data to temp save file
            TextWriter writer = null;
            try
            {
                writer = new StreamWriter(tmpSaveFile);
            }
            catch (IOException)
            {
                writer?.Close();
            }

            cropSaveDataSerializer.Serialize(writer, new CropSaveData {cropDictionary = cropDictionary, beginningOfDayCrops = beginningOfDayCrops});
            writer.Close();

            // If appropriate, move old crop data file to backup
            if (save_backups_and_metadata)
            {
                try
                {
                    if (File.Exists(backupSaveFile))
                    {
                        File.Delete(backupSaveFile);
                    }
                }
                catch (Exception) {}

                try
                {
                    File.Move(saveFile, backupSaveFile);
                }
                catch (Exception) {}
            }

            // Delete previous save file if it still exists (hasn't been moved to
            // backup)
            if (File.Exists(saveFile))
            {
                File.Delete(saveFile);
            }

            // Move new temp save file to non-temp save file
            try
            {
                File.Move(tmpSaveFile, saveFile);
            }
            catch (IOException ex)
            {
                Game1.debugOutput = Game1.parseText(ex.Message);
            }
        }

        private void onDayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            // In order to check for crops that have been destroyed and need to be removed from
            // the cropDictionary all together, we need to keep track of which crop locations
            // from the cropDictionary are found during the iteration over all crops in all
            // locations. Any which are not found must no longer exist (and have not been
            // replaced) and can be removed.
            var locationSet = new HashSet<CropLocation>();
            foreach (var location in Game1.locations)
            {
                if (location.IsOutdoors && !location.SeedsIgnoreSeasonsHere() && !(location is IslandLocation))
                {
                    // Found an outdoor location where seeds don't ignore seasons. Find all the
                    // crops here to cache necessary data for protecting them.
                    foreach (var pair in location.terrainFeatures.Pairs)
                    {
                        var tileLocation = pair.Key;
                        var terrainFeature = pair.Value;
                        if (terrainFeature is HoeDirt)
                        {
                            var hoeDirt = terrainFeature as HoeDirt;
                            var crop = hoeDirt.crop;
                            if (crop != null)
                            {
                                // Found a crop. Construct a CropLocation key
                                var cropLocation = new CropLocation
                                {
                                    LocationName = location.NameOrUniqueName,
                                    TileX = (int)tileLocation.X,
                                    TileY = (int)tileLocation.Y
                                };
                                
                                // Mark it as found via the locationSet, so we know not to remove
                                // the corresponding cropDictionary entry if one exists
                                locationSet.Add(cropLocation);

                                // Construct its growth stage so we can compare it to beginningOfDayCrops
                                // to see if it was newly-planted.
                                var cropGrowthStage = new CropGrowthStage
                                {
                                    CurrentPhase = crop.currentPhase.Value,
                                    DayOfCurrentPhase = crop.dayOfCurrentPhase.Value
                                };

                                var cropComparisonData = new CropComparisonData
                                {
                                    CropGrowthStage = cropGrowthStage,
                                    Dead = crop.dead.Value,
                                    ForageCrop = crop.forageCrop.Value,
                                    WhichForageCrop = crop.whichForageCrop.Value
                                };

                                // Determine if this crop was planted today or was pre-existing, based on whether
                                // or not it's different from the crop at this location at the beginning of the day.
                                if (!beginningOfDayCrops.ContainsKey(cropLocation) || !beginningOfDayCrops[cropLocation].Equals(cropComparisonData))
                                {
                                    // No crop was found at this location at the beginning of the day, or the comparison data
                                    // is different. Consider it a new crop, and add a new CropData for it in the cropDictionary.
                                    var cd = new CropData
                                    {
                                        CropGrowthStage = cropGrowthStage,
                                        MarkedForDeath = false,
                                        OriginalSeasonsToGrowIn = crop.seasonsToGrowIn.ToList(),
                                        HasExistedInIncompatibleSeason = false
                                    };
                                    cropDictionary[cropLocation] = cd;
                                    monitor.Log($"Found newly planted crop: {location.NameOrUniqueName} | {tileLocation.X}, {tileLocation.Y} | {crop.phaseDays.Count - 1} | {crop.currentPhase.Value} | {crop.phaseDays[crop.phaseDays.Count - 1] - 1} | {crop.dayOfCurrentPhase.Value}", LogLevel.Debug);
                                }
                            }
                        }
                    }
                }
            }

            // Lastly, if there were any CropLocations in the cropDictionary that we DIDN'T see throughout the entire
            // iteration, then they must've been destroyed, AND they weren't replaced with a new crop at the same location.
            // In such a case, we can remove it from the cropDictionary.
            var locationSetComplement = new HashSet<CropLocation>();
            foreach (var kvp in cropDictionary)
            {
                if (!locationSet.Contains(kvp.Key))
                {
                    locationSetComplement.Add(kvp.Key);
                }
            }
            foreach (var cropLocation in locationSetComplement)
            {
                cropDictionary.Remove(cropLocation);
            }

            // There might be one small issue with this whole methodology. Suppose a crop is planted by a player.
            // Now, suppose that later, over a single night, the crop is removed from the game
            // (e.g. at the turn of the season, some crops are removed rather than simply dying), and then
            // immediately replaced with forage crop (e.g. a naturally-occurring spring onion). The spring
            // onion's growth stage will be the same at the end of the day as the beginning of the day, and its location will
            // be filled throughout the entire day. So, as intended, it won't be recognized as a "player-planted crop". However,
            // since it's still occupying the location of a previous crop, said location crop won't be removed from the
            // cropDictionary, and it'll be treated as a pre-existing player-planted crop (even though it isn't one).
            // It seems like we might be able to check this edge case by comparing the growth stages and looking for
            // inconsistencies, but in extreme coincidences, the growth stages can line up as well.

            // But all of that seems incredibly unlikely, and all it would do is wrongly modify a forage crop, possibly killing
            // it and / or setting it to survive through incompatible seasons. But killing a forage crop doesn't seem to be a
            // big deal, and they seem to be removed at the end of the day anyways by some external mechanism, so the season
            // compatibility also doesn't seem to matter much. So we'll just ignore this issue, until a better solution becomes
            // viable (such as planting and harvesting SMAPI events, or something similar).
        }

        private void onDayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            beginningOfDayCrops.Clear();
            foreach (var location in Game1.locations)
            {
                if (location.IsOutdoors && !location.SeedsIgnoreSeasonsHere() && location is not IslandLocation)
                {
                    // Found an outdoor location where seeds don't ignore seasons. Find all the
                    // crops here to cache necessary data for protecting them.
                    foreach (var pair in location.terrainFeatures.Pairs)
                    {
                        var tileLocation = pair.Key;
                        var terrainFeature = pair.Value;
                        if (terrainFeature is HoeDirt)
                        {
                            var hoeDirt = terrainFeature as HoeDirt;
                            var crop = hoeDirt.crop;
                            if (crop != null) {
                                // Found a crop. Construct a CropLocation key
                                var cropLocation = new CropLocation
                                {
                                    LocationName = location.NameOrUniqueName,
                                    TileX = (int) tileLocation.X,
                                    TileY = (int) tileLocation.Y
                                };

                                var cropGrowthStage = new CropGrowthStage
                                {
                                    CurrentPhase = crop.currentPhase.Value,
                                    DayOfCurrentPhase = crop.dayOfCurrentPhase.Value
                                };

                                CropData cropData;
                                CropComparisonData cropComparisonData;
                                // Now, we have to update the properties of the CropData entry
                                // in the cropDictionary. Firstly, check if such a CropData entry exists
                                // (it won't exist for auto-spawned crops, like spring onion, since they'll
                                // never have passed the previous "newly planted test")
                                if (!cropDictionary.TryGetValue(cropLocation, out cropData))
                                {
                                    // The crop was not planted by the player. However, we do want to
                                    // record its comparison information so that we can check this evening
                                    // if it has changed, which would indicate that it HAS been replaced
                                    // by a player-planted crop.

                                    cropComparisonData = new CropComparisonData
                                    {
                                        CropGrowthStage = cropGrowthStage,
                                        Dead = crop.dead.Value,
                                        ForageCrop = crop.forageCrop.Value,
                                        WhichForageCrop = crop.whichForageCrop.Value
                                    };

                                    beginningOfDayCrops[cropLocation] = cropComparisonData;

                                    // Now move on to the next crop; we don't want to mess with this one.
                                    continue;
                                }

                                // As of last night, the crop at this location was considered to have been
                                // planted by the player. Let's hope that it hasn't somehow been replaced
                                // by an entirely different crop overnight; though that seems unlikely.

                                // Check if it's currently a season which is incompatible with the
                                // crop's ORIGINAL compatible seasons. If so, update the crop data to
                                // reflect this. Additionally, if it's out-of-season and the crop is still
                                // growing (i.e. today's growth stage is different from what's in the
                                // cropDictionary from the previous day), then it should be marked for death;
                                // crops planted too late to fully grow before the turn of the season should
                                // not be saved.
                                if (!cropData.OriginalSeasonsToGrowIn.Contains(location.GetSeasonForLocation()))
                                {
                                    cropData.HasExistedInIncompatibleSeason = true;
                                    if (crop.phaseDays.Count > 0 && crop.currentPhase.Value < crop.phaseDays.Count - 1)
                                    {
                                        cropData.MarkedForDeath = true;
                                    }
                                }

                                // Next, update the crop's growth stage in the CropData.
                                cropData.CropGrowthStage = cropGrowthStage;

                                // Now we have to update the crop itself. If it's existed out-of-season,
                                // then its regrowAfterHarvest value should be set to -1, so that the
                                // farmer only gets one more harvest out of it.
                                if (cropData.HasExistedInIncompatibleSeason)
                                {
                                    crop.regrowAfterHarvest.Value = -1;
                                }

                                // And if the crop has been marked for death because it was planted too close to
                                // the turn of the season, then we should make sure it's killed.
                                if (cropData.MarkedForDeath)
                                {
                                    crop.Kill();
                                }

                                // Make sure that the crop is set to survive in all seasons, so that it
                                // only dies if it's harvested for the last time or manually killed after being
                                // marked for death
                                if (!crop.seasonsToGrowIn.Contains("spring"))
                                {
                                    crop.seasonsToGrowIn.Add("spring");
                                }
                                if (!crop.seasonsToGrowIn.Contains("summer"))
                                {
                                    crop.seasonsToGrowIn.Add("summer");
                                }
                                if (!crop.seasonsToGrowIn.Contains("fall"))
                                {
                                    crop.seasonsToGrowIn.Add("fall");
                                }
                                if (!crop.seasonsToGrowIn.Contains("winter"))
                                {
                                    crop.seasonsToGrowIn.Add("winter");
                                }

                                // Lastly, now that the crop has been updated, construct the comparison data for later
                                // so that we can check if this has been replaced by a newly planted crop in the evening.

                                cropComparisonData = new CropComparisonData
                                {
                                    CropGrowthStage = cropGrowthStage,
                                    Dead = crop.dead.Value,
                                    ForageCrop = crop.forageCrop.Value,
                                    WhichForageCrop = crop.whichForageCrop.Value
                                };

                                beginningOfDayCrops[cropLocation] = cropComparisonData;
                            }
                        }
                    }
                }
            }
        }
    }
}
