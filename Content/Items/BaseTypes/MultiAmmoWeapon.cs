﻿using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI.Chat;

namespace StarlightRiver.Content.Items.BaseTypes
{
	public abstract class MultiAmmoWeapon : ModItem
	{
		[CloneByReference]
		public Item ammoItem;

		public bool hasAmmo;

		public AmmoStruct currentAmmoStruct;

		public virtual List<AmmoStruct> ValidAmmos { get; }

		public override void Load()
		{
			StarlightPlayer.ResetEffectsEvent += ResetAmmos;
			Terraria.UI.On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawAmmoNumber;
		}

		private void DrawAmmoNumber(Terraria.UI.On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor)
		{
			orig.Invoke(spriteBatch, inv, context, slot, position, lightColor);
			if (!Main.playerInventory)
			{
				if (context == 13)
				{
					if (inv[slot].ModItem is MultiAmmoWeapon weapon && weapon.Type == Type)
					{
						if (weapon.ammoItem != null)
						{
							int AmmoNumber = 0;
							for (int x = 0; x < ValidAmmos.Count; x++)
							{
								for (int j = 0; j < 58; j++)
								{
									if (inv[j].stack > 0 && inv[j].type == ValidAmmos[x].ammoID)
										AmmoNumber += inv[j].stack; //find all valid ammo to get total amount of ammo for the number
								}
							}

							if (AmmoNumber > 0)
							{
								ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, $"{AmmoNumber}", position + new Vector2(8f, 30f) * Main.inventoryScale,
										lightColor, 0f, Vector2.Zero, new Vector2(Main.inventoryScale * 0.8f), -1f, Main.inventoryScale); //draw total ammo number
							}
						}
						else
						{
							ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, "0", position + new Vector2(8f, 30f) * Main.inventoryScale,
															lightColor, 0f, Vector2.Zero, new Vector2(Main.inventoryScale * 0.8f), -1f, Main.inventoryScale);// if ammoItem is null, the player has no valid ammo in their inventory, so draw number 0.
						}
					}
				}
			}
		}

		private void ResetAmmos(StarlightPlayer Player)
		{
			ammoItem = null;
			hasAmmo = false;
		}

		public override void Unload()
		{
			Terraria.UI.On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color -= DrawAmmoNumber;
		}

		public virtual void SafeSetDefaults() { }
		public sealed override void SetDefaults()
		{
			SafeSetDefaults();
			Item.DamageType = DamageClass.Ranged;
			Item.useAmmo = AmmoID.None;
			Item.shoot = ProjectileID.PurificationPowder;
		}

		public virtual void SafeUpdateInventory() { }
		public sealed override void UpdateInventory(Player player)
		{
			SafeUpdateInventory();

			for (int i = 0; i < player.inventory.Length; i++)
			{
				bool exit = false;

				Item item = player.inventory[i];

				for (int x = 0; x < ValidAmmos.Count; x++)
				{
					if (ValidAmmos[x].ammoID == item.type)
					{
						hasAmmo = true;
						ammoItem = item;
						currentAmmoStruct = ValidAmmos[x];
						exit = true;
						break;
					}
				}

				if (exit)
					break;
			}

			if (ammoItem != null)
			{
				if (!player.HasItem(ammoItem.type)) //makes ammo get refreshed if thrown out of inventory or crafted, etc.
				{
					hasAmmo = false;
					ammoItem = null;
				}
			}
		}

		public virtual bool SafeCanUseItem(Player player) { return true; }
		public sealed override bool CanUseItem(Player player)
		{
			return base.CanUseItem(player) && SafeCanUseItem(player) && hasAmmo;
		}

		public virtual void SafeModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }
		public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			SafeModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
			float speed = velocity.Length();
			velocity.Normalize();
			velocity *= speed + currentAmmoStruct.shootSpeed;

			damage += currentAmmoStruct.damage;
			knockback += currentAmmoStruct.knockBack;

			type = currentAmmoStruct.projectileID;
		}

		public virtual bool? SafeUseItem(Player player) { return null; }
		public sealed override bool? UseItem(Player player)
		{
			SafeUseItem(player);

			if (Item.ModItem.CanConsumeAmmo(ammoItem, player))
			{
				int type = currentAmmoStruct.projectileID; // this code sucks ass
				bool dontConsumeAmmo = false;

				if (player.magicQuiver && ammoItem.ammo == AmmoID.Arrow && Main.rand.NextBool(5))
					dontConsumeAmmo = true;
				if (player.ammoBox && Main.rand.NextBool(5))
					dontConsumeAmmo = true;
				if (player.ammoPotion && Main.rand.NextBool(5))
					dontConsumeAmmo = true;
				if (player.ammoCost80 && Main.rand.NextBool(5))
					dontConsumeAmmo = true;
				if (player.ammoCost75 && Main.rand.NextBool(4))
					dontConsumeAmmo = true;
				if (type == 85 && player.itemAnimation < player.itemAnimationMax - 6)
					dontConsumeAmmo = true;
				if ((type == 145 || type == 146 || type == 147 || type == 148 || type == 149) && player.itemAnimation < player.itemAnimationMax - 5)
					dontConsumeAmmo = true;

				if (!dontConsumeAmmo)
				{
					ammoItem.ModItem?.OnConsumedAsAmmo(Item, player); //idk why a non-ammo item would ever override onconsumedasammo but if it does this runs

					Item.ModItem.OnConsumeAmmo(ammoItem, player);

					ammoItem.stack--;
					if (ammoItem.stack <= 0)
						ammoItem.TurnToAir();
				}
			}

			return base.UseItem(player);
		}

		public virtual void SafeModifyTooltips(List<TooltipLine> tooltips) { }
		public sealed override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			SafeModifyTooltips(tooltips);

			if (ammoItem == null)
				return;

			if (!Main.LocalPlayer.controlDown)
			{
				tooltips.Add(new TooltipLine(Mod, "Ammo", "[c/646464:Press DOWN for ammo information]"));
			}
			else
			{
				string ammoList = "Valid Ammunition:\n";

				List<AmmoStruct> ammos = [.. ValidAmmos.OrderBy(a => string.IsNullOrEmpty(a.tooltip) ? 0 : 1)];

				foreach (AmmoStruct ammo in ammos)
				{
					Item dummy = new();
					dummy.SetDefaults(ammo.ammoID);

					string tooltip = !string.IsNullOrEmpty(ammo.tooltip) ? ": " + ammo.tooltip : "";

					ammoList += $"[i:{dummy.type}] {dummy.Name}{tooltip}\n";
				}

				tooltips.Add(new TooltipLine(StarlightRiver.Instance, "AmmoListTooltip", ammoList));
			}

			TooltipLine AmmoLine = new TooltipLine(StarlightRiver.Instance, "AmmoLineToolTip", $"Current Ammo:[i:{ammoItem.type}]{ammoItem.stack} {ammoItem.Name}:" +
				$" {ValidAmmos.Find(a => a.ammoID == ammoItem.type).tooltip}");

			TooltipLine kbLine = tooltips.Find(n => n.Name == "Knockback");
			int index = kbLine is null ? tooltips.Count - 1 : tooltips.IndexOf(kbLine);
			tooltips.Insert(index + 1, AmmoLine);
		}
	}

	public struct AmmoStruct // contains everything needed for the ammo, ID, projectile, etc
	{
		public int damage;
		public float shootSpeed;
		public float knockBack;

		public int projectileID;
		public int ammoID;

		public string tooltip;
		public AmmoStruct(int ammoid, int projectileid, string tooltip, int damage = 0, float shootspeed = 0f, float knockback = 0f)
		{
			ammoID = ammoid;
			projectileID = projectileid;
			this.damage = damage;
			shootSpeed = shootspeed;
			knockBack = knockback;
			this.tooltip = tooltip;
		}
	}
}