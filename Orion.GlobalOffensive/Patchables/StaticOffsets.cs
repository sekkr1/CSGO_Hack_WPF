// Copyright (C) 2015 aevitas
// See the file LICENSE for copying permission.

namespace Orion.GlobalOffensive.Patchables
{
	// Contains offsets that hardly ever, if ever at all, change.
	public enum StaticOffsets
	{
		// Entity
		Position = 0x134,
		Team = 0xF0,
		Armor = 0xA8A4,
		Health = 0xFC,
		Dormant = 0xE9,
		Index = 0x64,
		Flags = 0x100,
		LifeState = 0x25B,
        FlashMaxAlpha = 0x0000A304,
        ForceJump = 0x04F5FD5C,
        BoneMatrix = 0x2698,
        CrosshairId = 0x0000AA70,
        ActiveWeapon = 0x2EE8,
        WeaponID= 0x32EC,
        CollisionGroup = 0x0470,
        Collision = 0x0318,
        vecMin = 0x0320,
        ItemDefinitionIndex = 0x2F88,
        vecMax = 0x032C,
        // GameClient
        LocalPlayerIndex = 0x178,
		GameState = 0x100,

        //Glow

        glowObjectsCount=0xC,

		// EntityList/ObjectManager
		EntitySize = 0x10
	}
}