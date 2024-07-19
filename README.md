# BannerlordsSyncItemRosterLists
Bannerlords Save Dictionaries of Item Rosters With Example. Because I was too lazy (efficient) a programmer to use butterlib or serialize stuff apparently.

Requires NO OTHER MODS
Does not affect settlements other than adding the menu option in the tavern (which adds an option in the main city menu)

# What is this?
This is an example mod for Mound and Blade Warband 1.2.10 (most recent as of 7/19).  It is fully working.  It adds a menu option to the tavern - start there.
this mod has 3 item rosters per city, they persist across saves using datastore.syncdata.
They are Unique per city.
There is a function to save and a function to load an item roster list.
This is done without any fancy serialization or butterlib, using bannerlords built in datastore.syncdata properly.

# Why do i care?
We save and load lots of item rosters and organize them really easily.

# How do I use this?
Look at the code, you can see where the itemrosters were defined, and you can see where the itemrosters were saved / loaded using a single function each.
say you have code like this (this is in the code):
its a list of cities id's (string), and itemrosters (for trading your loot or items into).

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

//and Save the farm input inventories
 
SaveRosters(dataStore, townFarmInputRosters, "FarmInputInventory"); (in the game menu it reads 'warehouse'

 // Save the farm output inventories
 SaveRosters(dataStore, townFarmOutputRosters, "FarmOutputInventory");

 //THEN, we can load them later.

  // Load the town rosters
 LoadRosters(dataStore, townItemRosters, "MainInventory");

 // Load the farm input inventories
 LoadRosters(dataStore, townFarmInputRosters, "FarmInputInventory");

 // Load the farm output inventories
 LoadRosters(dataStore, townFarmOutputRosters, "FarmOutputInventory");

 //!! Make sure these calls are made during an if then statement like in my example code!

 if (datastore.IsSaving)
 {
 ...call functions to save the roster lists.
return; //sanity
}

if (datastore.IsLoading)
{
...call the functions to load the roster lists.
}



        
