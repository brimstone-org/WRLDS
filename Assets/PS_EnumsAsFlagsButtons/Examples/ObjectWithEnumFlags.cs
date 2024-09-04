using UnityEngine;
using System; // To enable proper Debug Output.

public class ObjectWithEnumFlags : ScriptableObject {

	// The test ENUM definitions. Note the System.Flags attribute

	[System.Flags]
	public enum Fruits {        // Use the << Bit shift operator to set the values of the enum
        Apple = 1 << 0,         // 000001: This equals 1   
        Pear = 1 << 1,          // 000010: This equals 2
        Peach = 1 << 2,         // 000100: This equals 4
        Melon = 1 << 3,         // 001000: This equals 8
        Strawberry = 1 << 4,    // 010000: This equals 16
        Rasperry = 1 << 5       // 100000: This equals 32
    }                           // Note: There may be nothing for value 0 as this is used when no flag is active

	[System.Flags]
	public enum Proficiencies {
        Swords = 1 << 0,
        Axes = 1 << 1,
        Spears = 1 << 2,
        Whips = 1 << 3,
        Bows = 1 << 4,
        Boomerangs = 1 << 5,
        Rayguns = 1 << 6
    }

	[System.Flags]
	public enum Factions {
        HumanSpaceAlliance = 1 << 0,
        UnitedGalacticFederation = 1 << 1,
        EmpireOfOverminds = 1 << 2,
        NebulaPirateConglomerate = 1 << 3,
        TimeTravelEnforcementCorps = 1 << 4
    }

	// Some dummy stats to make the test assets look different

	[Header("Dummy Stats")]
	public string displayName = "Unnamed";
	public int health = 100;
	public int boosterPower = 100;

	// The actual enums

	[Header("Enum Flags")] 

	[EnumFlags]						// An enum that fits as many buttons as possible
	public Fruits fruits;

    [EnumFlags(6)]
    public Fruits[] allergies;       // An array of enums, 6 values per line

    [EnumFlags(2, false)] 			// An enum that always shows two buttons per row and no tooltip
	public Proficiencies proficiencies;

    [Header("Factions")]
	[EnumFlags(1, true, false)]		// An enum that always show a single button per row, has a tooltip and no label
	public Factions factions;


    // This prints a line to the Log whenver a Scriptable Object is changed
    void OnValidate() {
        /*global::Logger.Log("[ObjectWithEnumFlags] "+name+" set to\n" +
            "Fruits:\t\t" + GetBitsString((byte) fruits) + " = " + (int) fruits + "\n" +
            "Proficiencies:\t" + GetBitsString((byte) proficiencies) + " = " + (int)proficiencies + "\n" +
            "Factions:\t\t" + GetBitsString((byte) factions) + " = " + (int)factions);*/
    }

    string GetBitsString(byte value, int numLeadingZeros = 6) {
        return Convert.ToString(value, 2).PadLeft(numLeadingZeros, '0');
    }

}
