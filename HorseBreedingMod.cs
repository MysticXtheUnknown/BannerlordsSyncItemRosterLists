using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Roster;
using System;
using System.IO;


namespace LandCharterMod
{  
    public class LandCharterSubModule : MBSubModuleBase
    {
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarterObject;
                campaignStarter.AddBehavior(new LandCharterCampaignBehavior());
                InformationManager.DisplayMessage(new InformationMessage("LandCharterCampaignBehavior added."));
            }
        }

        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);
            if (game.GameType is Campaign)
            {
                CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnCampaignStart);
                InformationManager.DisplayMessage(new InformationMessage("LandCharterSubModule initialized."));
            }
        }

        private void OnCampaignStart(CampaignGameStarter campaignStarter)
        {
            campaignStarter.AddBehavior(new LandCharterCampaignBehavior());
            InformationManager.DisplayMessage(new InformationMessage("LandCharterCampaignBehavior added."));
        }
    }

    public class LandCharterCampaignBehavior : CampaignBehaviorBase
    {
        private Dictionary<string, bool> townLandCharterOwnership = new Dictionary<string, bool>();

        // Dictionary to store ItemRosters for each town
        private Dictionary<string, ItemRoster> townItemRosters = new Dictionary<string, ItemRoster>();
        // Dictionary to store ItemRosters for each town's farm input inventory
        private Dictionary<string, ItemRoster> townFarmInputRosters = new Dictionary<string, ItemRoster>();

        private Dictionary<string, ItemRoster> townFarmOutputRosters = new Dictionary<string, ItemRoster>();

     

        //Animal breeding progress... 1.0
        private Dictionary<string, Dictionary<string, float>> animalBreedingProgress = new Dictionary<string, Dictionary<string, float>>();


        public static float horseBreedingRate = 100f; // 1 horse per 100 days
        public static float pigBreedingRate = 4f; // 1 pig per 4 days
        public static float sheepBreedingRate = 50f; // 1 sheep per 50 days
        public static float cowBreedingRate = 100f; // 1 cow per 100 days


        // Add more cities as needed...

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.GameMenuOpened.AddNonSerializedListener(this, OnMenuOpen);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            //LogAllSettlements("RussCityList.txt");
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("townLandCharterOwnership", ref townLandCharterOwnership);
            SyncAllTownRosters(dataStore);
        }

        public void SyncAllTownRosters(IDataStore dataStore)
        { //WORKS...
            russLog("SyncData called");
            if (dataStore.IsSaving)
            {
                russLog("SYNCDATA: Starting SAVE");
            }
            if (dataStore.IsLoading)
            {
                russLog("SYNCDATA: Starting LOAD");
            }



            //string result = string.Concat(firstString, " ", secondString);

            if (dataStore.IsSaving) //save the rosters and breeding data
            {// Save the town rosters
                SaveRosters(dataStore, townItemRosters, "MainInventory");

                // Save the farm input inventories
                SaveRosters(dataStore, townFarmInputRosters, "FarmInputInventory");

                // Save the farm output inventories
                SaveRosters(dataStore, townFarmOutputRosters, "FarmOutputInventory");

                /*
                //save the town roster
                ItemRoster heldRoster;
                foreach (var kvp in townItemRosters)
                {
                    string uniqueName = string.Concat("Russ_MainInventory", "_", kvp.Key);
                    heldRoster = kvp.Value;
                    dataStore.SyncData(uniqueName, ref heldRoster);
                    russLog($"Saved {kvp.Key}");
                    //townItemRosters[kvp.Key] = heldRoster;
                }
                
                //save the other rosters.
                foreach(var kvp in townFarmInputRosters)
                {
                    string uniqueName = string.Concat("Russ_FarmInputInventory", "_", kvp.Key);
                    heldRoster = kvp.Value;
                    dataStore.SyncData(uniqueName, ref heldRoster);
                    russLog($"Saved {kvp.Key}");
                }

                foreach (var kvp in townFarmOutputRosters)
                {
                    string uniqueName = string.Concat("Russ_FarmOutputInventory", "_", kvp.Key);
                    heldRoster = kvp.Value;
                    dataStore.SyncData(uniqueName, ref heldRoster);
                    russLog($"Saved {kvp.Key}");
                }
                */
                //save the breeding progress
                foreach (var kvp in animalBreedingProgress) //string float
                {
                    string id1 = kvp.Key;
                    foreach (var kvp2 in animalBreedingProgress[kvp.Key])
                    {   
                        string id2 = kvp2.Key;
                        string uniqueSubName = string.Concat("Russ_BreedingSubData", "_", id1, "_",id2);
                        var myFloat = 0;
                        dataStore.SyncData(uniqueSubName, ref myFloat);
                        russLog($"Saved Breeding Data {kvp2.Key}");

                    }
                }
                return;
                
            }

            if (dataStore.IsLoading)
            {
                // Load the town rosters
                LoadRosters(dataStore, townItemRosters, "MainInventory");

                // Load the farm input inventories
                LoadRosters(dataStore, townFarmInputRosters, "FarmInputInventory");

                // Load the farm output inventories
                LoadRosters(dataStore, townFarmOutputRosters, "FarmOutputInventory");

                /*
                //load main inventory

                foreach (var settlement in Settlement.All)
                {
                    string id = settlement.StringId;
                    string uniqueName = string.Concat("Russ_MainInventory", "_", id); //tested, works
                                                                                      //string id = settlement.StringId;
                    ItemRoster temporaryRoster = new ItemRoster();
                    dataStore.SyncData(uniqueName, ref temporaryRoster); //load the saved data
                    townItemRosters[id] = temporaryRoster;

                    russLog($"Loaded {id}");
                }
                //load farm input inventory
                foreach (var settlement in Settlement.All)
                {
                    string id = settlement.StringId;
                    string uniqueName = string.Concat("Russ_FarmInputInventory", "_", id); //tested, works
                                                                                      //string id = settlement.StringId;
                    ItemRoster temporaryRoster = new ItemRoster();
                    dataStore.SyncData(uniqueName, ref temporaryRoster); //load the saved data
                    townFarmInputRosters[id] = temporaryRoster;

                    russLog($"Loaded {id}");
                }
                //load farm output inventory
                foreach (var settlement in Settlement.All)
                {
                    string id = settlement.StringId;
                    string uniqueName = string.Concat("Russ_FarmOutputInventory", "_", id); //tested, works
                                                                                           //string id = settlement.StringId;
                    ItemRoster temporaryRoster = new ItemRoster();
                    dataStore.SyncData(uniqueName, ref temporaryRoster); //load the saved data
                    townFarmOutputRosters[id] = temporaryRoster;

                    russLog($"Loaded {id}");
                }
                */
                //Loading Breeding Progression
                foreach (var settlement in Settlement.All)
                {
                    russLog($"Starting Breed Load");
                    string id1 = settlement.StringId;

                    if (!animalBreedingProgress.ContainsKey(id1))
                    {
                        animalBreedingProgress[id1] = new Dictionary<string, float>();
                        russLog($"Initialized new dictionary for {id1}");
                    }


                    russLog($"First loop");
                    foreach (var kpv2 in animalBreedingProgress[settlement.StringId])
                    {
                        russLog($"Inner Loop");
                        string id2 = kpv2.Key;
                        string uniqueSubName = string.Concat("Russ_BreedingSubData", "_", id1, "_", id2);                                     //string id = settlement.StringId;
                        float tempfloat = 0;
                        dataStore.SyncData(uniqueSubName, ref tempfloat); //load the saved data

                        russLog($"Attempting Assignment:");
                        animalBreedingProgress[id1][id2] = tempfloat;
                        russLog($"Loaded {string.Concat(id1, " ", id2)}");


                    }

                    

                }
                russLog("Loaded all town rosters");
            }
            return;
            
            

        }
        private void InitializeItemRosters()
        {
            foreach (var settlement in Settlement.All)
            {
                if (!townItemRosters.ContainsKey(settlement.StringId))
                {
                    townItemRosters[settlement.StringId] = new ItemRoster();

                }
            }
            InformationManager.DisplayMessage(new InformationMessage("Item rosters initialized for all settlements."));
        }

        private void OnSessionLaunched(CampaignGameStarter campaignStarter)
        {
            InitializeLandCharterOwnership();
            AddGameMenus(campaignStarter);
            InformationManager.DisplayMessage(new InformationMessage("Game menus added."));
        }

        private void InitializeLandCharterOwnership()
        {
            foreach (var settlement in Settlement.All)
            {
                if (!townItemRosters.ContainsKey(settlement.StringId) )
                {
                    townItemRosters[settlement.StringId] = new ItemRoster();
                }
                
                if (!townLandCharterOwnership.ContainsKey(settlement.StringId))
                {
                    townLandCharterOwnership[settlement.StringId] = false;
                }
            }
            InformationManager.DisplayMessage(new InformationMessage("Land charter ownership initialized for all settlements."));
        }

        private void OnMenuOpen(MenuCallbackArgs args)
        {
            string itsName = args.MenuContext.GameMenu.StringId;
            InformationManager.DisplayMessage(new InformationMessage($"Menu {itsName} opened."));
        }

        private void AddGameMenus(CampaignGameStarter campaignStarter)
        {
            InformationManager.DisplayMessage(new InformationMessage("Adding game menus..."));

            campaignStarter.AddGameMenuOption("town_backstreet", "buy_land_charter", "Buy Land Charter",
                args => true, OnBuyLandCharter, false, 1);
            InformationManager.DisplayMessage(new InformationMessage("Added Buy Land Charter menu option."));

            campaignStarter.AddGameMenuOption("town_backstreet", "sell_land_charter", "Sell Land Charter",
                args => PlayerOwnsLandCharter(Settlement.CurrentSettlement), OnSellLandCharter, false, 2);
            InformationManager.DisplayMessage(new InformationMessage("Added Sell Land Charter menu option."));

            campaignStarter.AddGameMenuOption("town", "visit_your_lands", "Visit your Lands",
                args => PlayerOwnsLandCharter(Settlement.CurrentSettlement), OnVisitYourLands, false, 1);
            InformationManager.DisplayMessage(new InformationMessage("Added Visit Your Lands menu option."));

            campaignStarter.AddGameMenu("land_charter_menu", "You are at your lands. What would you like to do?", null);
            campaignStarter.AddGameMenuOption("land_charter_menu", "access_inventory", "Access Inventory",
                args => true, OnAccessInventory, false, 1);
            //InformationManager.DisplayMessage(new InformationMessage("Added Access Inventory menu option."));
            campaignStarter.AddGameMenuOption("land_charter_menu", "visit_farming_office", "Visit Farming Office",
    args => true, OnVisitFarmingOffice, false, 2);
            campaignStarter.AddGameMenuOption("land_charter_menu", "leave_lands", "Leave your lands",
                args => true, args => GameMenu.SwitchToMenu("town"), false, 3);
            //InformationManager.DisplayMessage(new InformationMessage("Added Leave Your Lands menu option."));

            //Farming Office Menu
            campaignStarter.AddGameMenu("farming_office_menu", "You are in the farming office. What would you like to do?", null);
            campaignStarter.AddGameMenuOption("farming_office_menu", "visit_warehouse", "Visit Warehouse",
                args => true, OnVisitWarehouse, false, 1);
            campaignStarter.AddGameMenuOption("farming_office_menu", "visit_output_inventory", "Visit Output Inventory",
                args => true, OnVisitOutputInventory, false, 2);
            campaignStarter.AddGameMenuOption("farming_office_menu", "leave_office", "Leave the office",
                args => true, args => GameMenu.SwitchToMenu("land_charter_menu"), false, 3);


        }

        private void OnVisitFarmingOffice(MenuCallbackArgs args) // switch menu to Farming Office
        {
            InformationManager.DisplayMessage(new InformationMessage("Visiting the farming office..."));
            GameMenu.SwitchToMenu("farming_office_menu");
        }

        private void OnVisitWarehouse(MenuCallbackArgs args) //FARMING INPUT STASH
        {
            InformationManager.DisplayMessage(new InformationMessage("Accessing warehouse inventory..."));
            OpenFarmInputStash(Settlement.CurrentSettlement);
        }

        private void OnVisitOutputInventory(MenuCallbackArgs args)
        {
            InformationManager.DisplayMessage(new InformationMessage("Accessing output inventory..."));
            OpenOutputStash(Settlement.CurrentSettlement);
        }

        private void OpenOutputStash(Settlement settlement)
        {
            if (townFarmOutputRosters.ContainsKey(settlement.StringId))
            {
                InventoryManager.OpenScreenAsStash(townFarmOutputRosters[settlement.StringId]);
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("You do not own a land charter in this settlement."));
            }
        }

        private void OpenFarmInputStash(Settlement settlement)
        {
            if (townFarmInputRosters.ContainsKey(settlement.StringId))
            {
                ItemRoster farmInputRoster = townFarmInputRosters[settlement.StringId];

                // Set the flag to true
                //isFarmInputStashOpen = true;

                // Debug message
                InformationManager.DisplayMessage(new InformationMessage("Opening farm input stash."));

                // Open the inventory screen as stash
                InventoryManager.OpenScreenAsStash(farmInputRoster);

                // Hook into the campaign tick event to monitor when the inventory screen is closed
                // CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, CheckAnimalLimitAfterClose);

                // Debug message
                //InformationManager.DisplayMessage(new InformationMessage("Event listener for OnSessionLaunchedEvent added."));
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("You do not own a land charter in this settlement."));
            }
        }

        private bool PlayerOwnsLandCharter(Settlement settlement)
        {
            return townLandCharterOwnership.ContainsKey(settlement.StringId) && townLandCharterOwnership[settlement.StringId];
        }

        private void OnBuyLandCharter(MenuCallbackArgs args)
        {
            var settlement = Settlement.CurrentSettlement;
            if (settlement == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("Settlement is null. Cannot buy land charter."));
                return;
            }

            if (!PlayerOwnsLandCharter(settlement))
            {
                townLandCharterOwnership[settlement.StringId] = true;
                townItemRosters[settlement.StringId] = new ItemRoster();
                townFarmInputRosters[settlement.StringId] = new ItemRoster();
                townFarmOutputRosters[settlement.StringId] = new ItemRoster();
                animalBreedingProgress[settlement.StringId] = new Dictionary<string, float>();

            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage($"You already own a land charter in {settlement.Name}."));
            }
        }

        private void OnSellLandCharter(MenuCallbackArgs args)
        {
            var settlement = Settlement.CurrentSettlement;
            if (PlayerOwnsLandCharter(settlement))
            {
                townLandCharterOwnership[settlement.StringId] = false;
                GetCityInventory(settlement.StringId).Clear(); // Clear the inventory
                InformationManager.DisplayMessage(new InformationMessage($"You have sold your land charter in {settlement.Name}."));
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage($"You do not own a land charter in {settlement.Name}."));
            }
        }


        private void OnVisitYourLands(MenuCallbackArgs args)
        {
            InformationManager.DisplayMessage(new InformationMessage("Visiting your lands..."));
            GameMenu.SwitchToMenu("land_charter_menu");
        }

        private void OnAccessInventory(MenuCallbackArgs args)
        {
            var settlement = Settlement.CurrentSettlement;
            if (settlement == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("No current settlement."));
                return;
            }

            InformationManager.DisplayMessage(new InformationMessage($"Accessing inventory for {settlement.Name}..."));
            OpenRussInventory(settlement);
        }


        private void OpenRussInventory(Settlement settlement)
        {
            if (PlayerOwnsLandCharter(settlement))
            {
                var itemRoster = GetCityInventory(settlement.StringId);
                if (itemRoster == null)
                {
                    InformationManager.DisplayMessage(new InformationMessage($"Failed to get inventory for {settlement.Name}."));
                    return;
                }

                try
                {
                    InventoryManager.OpenScreenAsStash(itemRoster);
                    InformationManager.DisplayMessage(new InformationMessage($"Opened inventory for {settlement.Name}."));
                }
                catch (Exception ex)
                {
                    InformationManager.DisplayMessage(new InformationMessage($"Error opening inventory: {ex.Message}"));
                }
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("You do not own a land charter in this settlement."));
            }
        }

        private void OnDailyTick()
        {
            foreach (var settlementId in townFarmInputRosters.Keys)
            {
                ItemRoster farmInputRoster = townFarmInputRosters[settlementId];
                Dictionary<string, float> breedingProgress = animalBreedingProgress[settlementId];

                foreach (var element in farmInputRoster)
                {
                    if (element.EquipmentElement.Item.ItemCategory.IsAnimal)
                    {
                        string animalId = element.EquipmentElement.Item.StringId;
                        int count = element.Amount;
                        float breedingRate = GetBreedingRateForAnimal(animalId);

                        if (!breedingProgress.ContainsKey(animalId))
                        {
                            breedingProgress[animalId] = 0f;
                        }

                        breedingProgress[animalId] += count / breedingRate;

                        while (breedingProgress[animalId] >= 1f)
                        {
                            breedingProgress[animalId] -= 1f;
                            townFarmOutputRosters[settlementId].AddToCounts(element.EquipmentElement, 1);

                            //debug
                            InformationManager.DisplayMessage(new InformationMessage($"A new {element.EquipmentElement.Item.Name} has grown up in {Settlement.Find(settlementId).Name}."));
                        }
                    }
                }
            }
        }

        private float GetBreedingRateForAnimal(string animalId)
        {

            //InformationManager.DisplayMessage(new InformationMessage($"animal {animalId}."));
            switch (animalId)
            {
                case "horse":
                    return horseBreedingRate;
                case "hog":
                    return pigBreedingRate;
                case "sheep":
                    return sheepBreedingRate;
                case "cow":
                    return cowBreedingRate;
                default:
                    return horseBreedingRate; // Default rate if not specified
            }
        }

        public static void russLog(string message)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "RussBannerlordLogs.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }

        private ItemRoster GetCityInventory(string cityId)
        {
            return townItemRosters[cityId];
        }

        public static void LogAllSettlements(string filename)
        { 
            string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{filename}.txt");

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                foreach (var settlement in Settlement.All)
                {
                    string logMessage = $"Settlement Name: {settlement.Name}, StringId: {settlement.StringId}";
                    writer.WriteLine($"{DateTime.Now}: {logMessage}");
                    InformationManager.DisplayMessage(new InformationMessage(logMessage));
                }
            }
        }

        private void SaveRosters(IDataStore dataStore, Dictionary<string, ItemRoster> rosterDictionary, string rosterType)
        {
            ItemRoster heldRoster;
            foreach (var kvp in rosterDictionary)
            {
                string uniqueName = string.Concat("Russ_", rosterType, "_", kvp.Key);
                heldRoster = kvp.Value;
                dataStore.SyncData(uniqueName, ref heldRoster);
                russLog($"Saved {rosterType} for {kvp.Key}");
            }
        }
        private void LoadRosters(IDataStore dataStore, Dictionary<string, ItemRoster> rosterDictionary, string rosterType)
        {
            foreach (var settlement in Settlement.All)
            {
                string id = settlement.StringId;
                string uniqueName = string.Concat("Russ_", rosterType, "_", id); // Construct the unique name
                ItemRoster temporaryRoster = new ItemRoster();
                dataStore.SyncData(uniqueName, ref temporaryRoster); // Load the saved data
                rosterDictionary[id] = temporaryRoster;
                russLog($"Loaded {id} for {rosterType}");
            }
        }
    }



}