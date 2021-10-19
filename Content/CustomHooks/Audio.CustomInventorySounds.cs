using Microsoft.Xna.Framework.Audio;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Configs;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
	class CustomInventorySounds : HookGroup
	{
        public override SafetyLevel Safety => SafetyLevel.Questionable;

        public override void Load()
        {
            IL.Terraria.UI.ItemSlot.LeftClick_ItemArray_int_int += PlayCustomSound;
        }

		private void PlayCustomSound(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			while (c.TryGotoNext(n => n.MatchCall<Main>("PlaySound"))) //swap every sound call
			{
				var branchTarget = il.DefineLabel(c.Next.Next); //so we can skip to after the vanilla sound call later

				c.Index -= 5; //back up 5 instructions to just under where the soundID is pushed

				c.Emit(OpCodes.Ldarg_0); //load item array
				c.Emit(OpCodes.Ldarg_2); //load index
				c.Emit(OpCodes.Ldelem_Ref); //push item ref

				c.EmitDelegate<Func<int, Item, SoundEffectInstance>>(PlayNewSound); //play the custom sound if applicable
				c.Emit(OpCodes.Br, branchTarget); //move past sound call

				c.Index += 7; //so we wont keep patching the same call lol
			}
		}

		private SoundEffectInstance PlayNewSound(int originalSoundID, Item item)
		{
			Config config = GetInstance<Config>();

			if (config.InvSounds == CustomSounds.None)
				return Main.PlaySound(originalSoundID, -1, -1, 1, 1, 0);

			float pitch = -0.6f;

			if (item.IsAir)
			{
				item = Main.mouseItem;
				pitch += 0.6f;
			}

			if (originalSoundID != 7) 
				pitch += 0.3f;

			//this is gross. The alternative is moving this into a GlobalItem instead. Thus its here.
			if (item.modItem is ICustomInventorySound)
				return (item.modItem as ICustomInventorySound).InventorySound(pitch);

			else if (config.InvSounds == CustomSounds.Specific)
				return Main.PlaySound(originalSoundID, -1, -1, 1, 1, 0);

			else if (item.potion || item.healMana != 0 || (item.buffType != -1 && (item.Name.Contains("potion") || item.Name.Contains("Potion")))) //should probably figure a better check for this somehow? 1.4 content tags maybe?
				return Helper.PlayPitched("Pickups/PickupPotion", 1, 0.5f + pitch * 0.5f);

			else if (item.type == ItemID.CopperCoin || item.type == ItemID.SilverCoin || item.type == ItemID.GoldCoin || item.type == ItemID.PlatinumCoin) //coins are early since they're ammo and have damage and place tiles. 
				return Helper.PlayPitched(SoundID.Coins, 1, 0.3f + pitch);

			else if (item.dye > 0) //dyes
				return Helper.PlayPitched("Pickups/PickupPotion", 1, 0.9f + pitch * 0.25f);

			else if (item.createTile != -1) //placables
				return Helper.PlayPitched("Pickups/PickupGeneric", 1, 1 + pitch);

			else if (item.defense > 0) //armor and shields
				return Helper.PlayPitched("Pickups/PickupArmor", 1, 0.5f + pitch);

			else if (item.vanity) //vanity
				return Helper.PlayPitched("Pickups/PickupVanity", 1, 0.5f + pitch);

			else if (item.accessory) //non-vanity non-shield accessories
				return Helper.PlayPitched("Pickups/PickupAccessory", 1, 0.1f + pitch);

			else if (item.pick > 0 || item.axe > 0 || item.hammer > 0) //tools
				return Helper.PlayPitched("Pickups/PickupTool", 1, 0.5f + pitch);

			else if (item.damage > 0) //weapons and ammo
			{
				if (item.melee) //melee weapons
					return Helper.PlayPitched("Pickups/PickupMelee", 1, 0.5f + pitch);

				else if (item.useAmmo == ItemID.MusketBall) //guns
					return Helper.PlayPitched("Pickups/PickupGun", 1, 0 + pitch);

				else if (item.ranged) //other ranged weapons
					return Helper.PlayPitched("Pickups/PickupGeneric", 1, 0.9f + pitch * 0.5f);

				else if (item.magic || item.summon) //magic and summoning weapons
					return Helper.PlayPitched("Pickups/PickupGeneric", 1, 0.5f + pitch);

				else if (item.ammo == ItemID.WoodenArrow) //arrows
					return Helper.PlayPitched("Pickups/PickupGeneric", 1, 0.5f + pitch);

				else if (item.ammo > 0) //other ammo
					return Helper.PlayPitched("Pickups/PickupGeneric", 1, 0.5f + pitch);

				else //edge cases
					return Helper.PlayPitched("Pickups/PickupGeneric", 1, 0.5f + pitch);
			}

			else
				return Main.PlaySound(originalSoundID, -1, -1, 1, 1, 0);
		}
	}
}
