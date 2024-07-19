# BannerlordsSyncItemRosterLists
Bannerlords Save Dictionaries of Item Rosters With Example

# What is this?
This is an example mod for Mound and Blade Warband 1.2.10 (most recent as of 7/19). 

# Why do i care?
We save and load lots of dictionaries with dictionaries inside. we have 3 item rosters for each cityt!

# How do I use this?
Look at the code, you can see where the itemrosters were defined, and you can see where the itemrosters were saved / loaded using a single function each.
say you have code like this (this is in the code):

private Dictionary<string, ItemRoster> townItemRosters = new Dictionary<string, ItemRoster>();

the game doesnt want to use datastore.datasync on the townItemRosterObject.
So we get around that using this function to save:

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

and this function to load:

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

and we call them like this:

//save the main inventory
 SaveRosters(dataStore, townItemRosters, "MainInventory");

 // Save the farm input inventories
 SaveRosters(dataStore, townFarmInputRosters, "FarmInputInventory"); (in the game menu it reads 'warehouse'

 // Save the farm output inventories
 SaveRosters(dataStore, townFarmOutputRosters, "FarmOutputInventory");

 #




        
