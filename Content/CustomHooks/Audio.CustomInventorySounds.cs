using Microsoft.Xna.Framework.Audio;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Utilities;
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
		public override SafetyLevel Safety => SafetyLevel.Fragile; //gonna break if anyone else touches playSound

		public override void Load()
		{
			//TODO: Reimplement
			//IL.Terraria.UI.ItemSlot.LeftClick_ItemArray_int_int += PlayCustomSound;
		}

        /*
		private void PlayCustomSound(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			while (c.TryGotoNext(n => n.MatchCall<Main>("PlaySound"))) //swap every sound call
			{
				var branchTarget = il.DefineLabel(c.Next.Next); //so we can skip to after the vanilla sound call later

				c.Index -= 5; //back up 5 instructions to just under where the soundID is pushed

				c.Emit(OpCodes.Ldarg_0); //load Item array
				c.Emit(OpCodes.Ldarg_2); //load index
				c.Emit(OpCodes.Ldelem_Ref); //push Item ref

				c.EmitDelegate<Func<int, Item, SoundEffectInstance>>(PlayNewSound); //play the custom sound if applicable
				c.Emit(OpCodes.Br, branchTarget); //move past sound call

				//load a sound ID back onto the stack since mac / linux check both paths even if its an unconditional branch ()
				c.Emit(OpCodes.Ldc_I4_0); //putting a 0 but the actual value does not matter

				c.Index += 7; //so we wont keep patching the same call lol
			}
		}

		private SlotId PlayNewSound(int originalSoundID, Item Item)
		{
			AudioConfig config = GetInstance<AudioConfig>();

			if (config.InvSounds == CustomSounds.None)
				return SlotId.Invalid;

			float pitch = -0.6f;

			if (Item.IsAir)
			{
				Item = Main.mouseItem;
				pitch += 0.6f;
			}

			if (originalSoundID != 7)
				pitch += 0.3f;

			//this is gross. The alternative is moving this into a GlobalItem instead. Thus its here.
			if (Item.ModItem is ICustomInventorySound)
				return (Item.ModItem as ICustomInventorySound).InventorySound(pitch);

			else if (config.InvSounds == CustomSounds.Specific)
				return Terraria.Audio.SoundEngine.PlaySound(originalSoundID, -1, -1, 1, 1, 0);

			else if (Item.potion || Item.healMana != 0 || (Item.buffType != -1 && (Item.Name.Contains("potion") || Item.Name.Contains("Potion")))) //should probably figure a better check for this somehow? 1.4 content tags maybe?
				return Helper.PlayPitched("Pickups/PickupPotion", 1, 0.5f + pitch * 0.5f);

			else if (Item.type == ItemID.CopperCoin || Item.type == ItemID.SilverCoin || Item.type == ItemID.GoldCoin || Item.type == ItemID.PlatinumCoin) //coins are early since they're ammo and have damage and place tiles. 
				return Helper.PlayPitched(SoundID.Coins, 1, 0.3f + pitch);

			else if (Item.dye > 0) //dyes
				return Helper.PlayPitched("Pickups/PickupPotion", 1, 0.9f + pitch * 0.25f);

			else if (Item.createTile != -1) //placables
				return Helper.PlayPitched("Pickups/PickupGeneric", 1, 1 + pitch);

			else if (Item.defense > 0) //armor and shields
				return Helper.PlayPitched("Pickups/PickupArmor", 1, 0.5f + pitch);

			else if (Item.vanity) //vanity
				return Helper.PlayPitched("Pickups/PickupVanity", 1, 0.5f + pitch);

			else if (Item.accessory) //non-vanity non-shield accessories
				return Helper.PlayPitched("Pickups/PickupAccessory", 1, 0.1f + pitch);

			else if (Item.pick > 0 || Item.axe > 0 || Item.hammer > 0) //tools
				return Helper.PlayPitched("Pickups/PickupTool", 1, 0.5f + pitch);

			else if (Item.damage > 0) //weapons and ammo
			{
				if (Item.DamageType.Type == Terraria.ModLoader.DamageClass.Melee.Type) //melee weapons
					return Helper.PlayPitched("Pickups/PickupMelee", 1, 0.5f + pitch);

				else if (Item.useAmmo == ItemID.MusketBall) //guns
					return Helper.PlayPitched("Pickups/PickupGun", 1, 0 + pitch);

				else if (Item.DamageType.Type == Terraria.ModLoader.DamageClass.Ranged.Type) //other ranged weapons
					return Helper.PlayPitched("Pickups/PickupGeneric", 1, 0.9f + pitch * 0.5f);

				else if (Item.DamageType.Type == Terraria.ModLoader.DamageClass.Magic.Type || Item.DamageType.Type == Terraria.ModLoader.DamageClass.Summon.Type) //magic and summoning weapons
					return Helper.PlayPitched("Pickups/PickupGeneric", 1, 0.5f + pitch);

				else if (Item.ammo == ItemID.WoodenArrow) //arrows
					return Helper.PlayPitched("Pickups/PickupGeneric", 1, 0.5f + pitch);

				else if (Item.ammo > 0) //other ammo
					return Helper.PlayPitched("Pickups/PickupGeneric", 1, 0.5f + pitch);

				else //edge cases
					return Helper.PlayPitched("Pickups/PickupGeneric", 1, 0.5f + pitch);
			}

			else
				return Terraria.Audio.SoundEngine.PlaySound(originalSoundID, -1, -1, 1, 1, 0);
		}
        */
	}
}
