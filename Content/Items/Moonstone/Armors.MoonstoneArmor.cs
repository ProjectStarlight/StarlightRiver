﻿using NetEasy;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
	[AutoloadEquip(EquipType.Head)]
	public class MoonstoneHead : ModItem
	{
		public int moonCharge = 0;
		public bool spearOn = false;

		internal static Item dummySpear = new();

		private int moonFlash = 0;

		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void Load()
		{
			On_Player.KeyDoubleTap += ActivateSpear;
			On_Main.DrawPendingMouseText += SpoofMouseItem;
			StarlightPlayer.PreDrawEvent += DrawMoonCharge;
			StarlightPlayer.OnHitNPCEvent += ChargeFromMelee;
			StarlightPlayer.OnHitNPCWithProjEvent += ChargeFromProjectile;
		}

		public override void Unload()
		{
			On_Player.KeyDoubleTap -= ActivateSpear;
			On_Main.DrawPendingMouseText -= SpoofMouseItem;
			StarlightPlayer.PreDrawEvent -= DrawMoonCharge;
			StarlightPlayer.OnHitNPCEvent -= ChargeFromMelee;
			StarlightPlayer.OnHitNPCWithProjEvent -= ChargeFromProjectile;

			dummySpear.TurnToAir();
			dummySpear = null;
		}

		private void ChargeFromProjectile(Player Player, Projectile proj, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (proj.DamageType.Type == DamageClass.Melee.Type && proj.type != ProjectileType<DatsuzeiProjectile>() && IsArmorSet(Player))
				AddCharge(Player, damageDone);
		}

		private void ChargeFromMelee(Player Player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Item.DamageType.Type == DamageClass.Melee.Type && IsArmorSet(Player))
				AddCharge(Player, damageDone);
		}

		private void AddCharge(Player Player, int damage)
		{
			var head = Player.armor[0].ModItem as MoonstoneHead;

			if (head is null)
				return;

			int oldCharge = head.moonCharge;
			head.moonCharge += (int)(damage * 0.45f);

			if (head.moonCharge >= 180 && oldCharge < 180 || head.moonCharge >= 720 && oldCharge < 720)
				head.moonFlash = 30;

			Player.GetModPlayer<StarlightPlayer>().SetHitPacketStatus(shouldRunProjMethods: false);
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Moonstone Helmet");
			Tooltip.SetDefault("2% increased melee critical strike chance\n+20 {{Barrier}}");
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.value = 1;
			Item.rare = ItemRarityID.Green;
			Item.defense = 6;
		}

		public override void UpdateEquip(Player Player)
		{
			Player.GetCritChance(DamageClass.Melee) += 2;
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 20;

			if (!IsArmorSet(Player))
			{
				moonCharge = 0;
				spearOn = false;
			}
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "Accumulate lunar energy by dealing melee damage\nDouble tap DOWN to summon the legendary spear Datsuzei\nDatsuzei consumes this lunar energy and dissapears at zero";

			if (moonCharge > 720)
				moonCharge = 720;

			if (moonFlash > 0)
				moonFlash--;

			Lighting.AddLight(player.Center + new Vector2(0, -16), new Vector3(0.55f, 0.5f, 0.9f) * moonCharge / 720f * 0.5f);

			ArmorChargeUI.SetMessage($"{Math.Truncate(moonCharge / 720f * 100)}%");

			if (spearOn)
			{
				if (!(Main.mouseItem.type == dummySpear.type) && !Main.mouseItem.IsAir)
					Main.LocalPlayer.QuickSpawnItem(null, Main.mouseItem, Main.mouseItem.stack);

				Main.mouseItem = dummySpear;
				player.inventory[58] = dummySpear;
				player.selectedItem = 58;

				moonCharge--;

				if (moonCharge <= 0)
				{
					spearOn = false;
					dummySpear.TurnToAir();
				}
			}
			else if (Main.mouseItem.type == ItemType<Datsuzei>())
			{
				Main.mouseItem = new Item();
			}
		}

		private void ActivateSpear(On_Player.orig_KeyDoubleTap orig, Player player, int keyDir)
		{
			if (keyDir == 0 && IsArmorSet(player))
			{
				var helm = player.armor[0].ModItem as MoonstoneHead;

				if (helm.spearOn)
				{
					helm.spearOn = false;
					dummySpear.TurnToAir();
				}
				else if (helm.moonCharge > 180 && Datsuzei.activationTimer == 0 && !Main.projectile.Any(n => n.active && n.type == ProjectileType<DatsuzeiProjectile>() && n.owner == player.whoAmI))
				{
					dummySpear.SetDefaults(ItemType<Datsuzei>());
					helm.spearOn = true;
					var packet = new MoonstoneArmorPacket(player.whoAmI, helm.moonCharge, helm.spearOn);
					packet.Send(-1, player.whoAmI, false);

					int i = Projectile.NewProjectile(null, player.Center, Vector2.Zero, ProjectileType<DatsuzeiProjectile>(), 1, 0, player.whoAmI, -1, 160);
					Main.projectile[i].timeLeft = 160;
				}
			}

			orig(player, keyDir);
		}

		private void SpoofMouseItem(On_Main.orig_DrawPendingMouseText orig)
		{
			Player player = Main.LocalPlayer;

			if (dummySpear.IsAir && !Main.gameMenu)
				dummySpear.SetDefaults(ItemType<Datsuzei>());

			if (IsMoonstoneArmor(Main.HoverItem) && IsArmorSet(player) && player.controlUp)
			{
				Main.HoverItem = dummySpear.Clone();
				Main.hoverItemName = dummySpear.Name;
			}

			orig();
		}

		public bool IsMoonstoneArmor(Item Item)
		{
			return Item.type == ItemType<MoonstoneHead>() ||
				Item.type == ItemType<MoonstoneChest>() ||
				Item.type == ItemType<MoonstoneLegs>();
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ItemType<MoonstoneHead>() && body.type == ItemType<MoonstoneChest>() && legs.type == ItemType<MoonstoneLegs>();
		}

		public bool IsArmorSet(Player Player)
		{
			return Player.armor[0].type == ItemType<MoonstoneHead>() && Player.armor[1].type == ItemType<MoonstoneChest>() && Player.armor[2].type == ItemType<MoonstoneLegs>();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			Player Player = Main.LocalPlayer;

			if (IsArmorSet(Player))
			{
				if (!Player.controlUp)
				{
					var spearQuery = new TooltipLine(Mod, "StarlightRiver:ArmorSpearQuery", "hold UP for Datsuzei stats")
					{
						OverrideColor = new Color(200, 200, 200)
					};

					tooltips.Add(spearQuery);
				}
			}
		}

		private void DrawMoonCharge(Player Player, SpriteBatch spriteBatch)
		{
			if (IsArmorSet(Player) && !Player.dead && PlayerTarget.canUseTarget)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

				var head = Player.armor[0].ModItem as MoonstoneHead;
				float charge = head.moonCharge / 720f;

				Texture2D texRing = Assets.Items.Vitric.BossBowRing.Value;
				Color color = new Color(130, 110, 225) * (0.5f + charge * 0.5f);

				if (charge <= 180 / 720f)
					color = new Color(150, 150, 150) * (0.5f + charge * 0.5f);

				if (charge >= 1 || head.spearOn)
					color = new Color(150, 150 + (int)(Math.Sin(Main.GameUpdateCount * 0.2f) * 20), 255) * (0.5f + charge * 0.5f);

				color = Color.Lerp(color, Color.White, head.moonFlash / 30f);

				spriteBatch.Draw(texRing, Player.MountedCenter + new Vector2(0, -16) + Vector2.UnitY * Player.gfxOffY - Main.screenPosition, null, color, Main.GameUpdateCount * 0.01f, texRing.Size() / 2, 0.08f + charge * 0.05f, 0, 0);

				spriteBatch.End();

				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<MoonstoneBarItem>(), 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	[AutoloadEquip(EquipType.Body)]
	public class MoonstoneChest : ModItem
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Moonstone Chestpiece");
			Tooltip.SetDefault("+35 {{Barrier}}");
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.value = 1;
			Item.rare = ItemRarityID.Green;
			Item.defense = 7;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ItemType<MoonstoneHead>() && body.type == ItemType<MoonstoneChest>() && legs.type == ItemType<MoonstoneLegs>();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			Player Player = Main.LocalPlayer;

			if (IsArmorSet(Player.armor[0], Player.armor[1], Player.armor[2]))
			{
				if (!Player.controlUp)
				{
					var spearQuery = new TooltipLine(Mod, "StarlightRiver:ArmorSpearQuery", "hold UP for Datsuzei stats")
					{
						OverrideColor = new Color(200, 200, 200)
					};

					tooltips.Add(spearQuery);
				}
			}
		}

		public override void UpdateEquip(Player Player)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 35;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<MoonstoneBarItem>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	public class MoonstoneLegs : ModItem
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Moonstone Greaves");
			Tooltip.SetDefault("Improved acceleration\n +25 {{Barrier}}");
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.value = 1;
			Item.rare = ItemRarityID.Green;
			Item.defense = 6;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ItemType<MoonstoneHead>() && body.type == ItemType<MoonstoneChest>() && legs.type == ItemType<MoonstoneLegs>();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			Player Player = Main.LocalPlayer;

			if (IsArmorSet(Player.armor[0], Player.armor[1], Player.armor[2]))
			{
				if (!Player.controlUp)
				{
					var spearQuery = new TooltipLine(Mod, "StarlightRiver:ArmorSpearQuery", "hold UP for Datsuzei stats")
					{
						OverrideColor = new Color(200, 200, 200)
					};

					tooltips.Add(spearQuery);
				}
			}
		}

		public override void UpdateEquip(Player Player)
		{
			Player.runAcceleration *= 1.5f;
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 25;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<MoonstoneBarItem>(), 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	[Serializable]
	public class MoonstoneArmorPacket : Module
	{
		public readonly byte whoAmI;
		public readonly int charge;
		public readonly bool spearOn;

		public MoonstoneArmorPacket(int whoAmI, int charge, bool spearOn)
		{
			this.whoAmI = (byte)whoAmI;
			this.charge = charge;
			this.spearOn = spearOn;
		}

		protected override void Receive()
		{
			if (Main.netMode == NetmodeID.Server)
			{
				Send(-1, whoAmI, false);
				return;
			}

			Player Player = Main.player[whoAmI];

			if (Player.armor[0] != null && Player.armor[0].type == ItemType<MoonstoneHead>())
			{
				var head = Player.armor[0].ModItem as MoonstoneHead;
				head.moonCharge = charge;
				head.spearOn = spearOn;
			}
		}
	}
}