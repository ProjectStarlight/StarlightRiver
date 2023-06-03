using StarlightRiver.Content.Items.BuriedArtifacts;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Geomancer
{
	[AutoloadEquip(EquipType.Head)]
	public class GeomancerHood : ModItem, IArmorLayerDrawable
	{
		internal static Item dummyItem = new();

		public override string Texture => AssetDirectory.GeomancerItem + Name;

		public override void Load()
		{
			On_Main.DrawPendingMouseText += SpoofMouseItem;
		}

		public override void Unload()
		{
			On_Main.DrawPendingMouseText -= SpoofMouseItem;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Geomancer's Hood");
			//Tooltip.SetDefault("15% increased ranged critical strike damage");
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.value = 8000;
			Item.defense = 5;
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateEquip(Player Player)
		{
			if (Player.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.Topaz || Player.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All)
				Player.GetModPlayer<BarrierPlayer>().maxBarrier += 100;
		}

		private void SpoofMouseItem(On_Main.orig_DrawPendingMouseText orig)
		{
			Player Player = Main.LocalPlayer;

			if (dummyItem.IsAir && !Main.gameMenu)
				dummyItem.SetDefaults(ModContent.ItemType<GeomancerItemDummy>());

			if (IsGeomancerArmor(Main.HoverItem) && IsArmorSet(Player) && Player.controlUp)
			{
				Main.HoverItem = dummyItem.Clone();
				Main.hoverItemName = dummyItem.Name;
			}

			orig();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			Player Player = Main.LocalPlayer;

			if (IsArmorSet(Player.armor[0], Player.armor[1], Player.armor[2]))
			{
				if (!Player.controlUp)
				{
					var gemQuery = new TooltipLine(Mod, "StarlightRiver:ArmorGemQuery", "hold UP for buff info")
					{
						OverrideColor = new Color(200, 200, 200)
					};

					tooltips.Add(gemQuery);
				}
			}
		}

		public bool IsGeomancerArmor(Item Item)
		{
			return Item.type == ModContent.ItemType<GeomancerHood>() ||
				Item.type == ModContent.ItemType<GeomancerRobe>() ||
				Item.type == ModContent.ItemType<GeomancerPants>();
		}

		public bool IsArmorSet(Player Player)
		{
			return Player.armor[0].type == ModContent.ItemType<GeomancerHood>() && Player.armor[1].type == ModContent.ItemType<GeomancerRobe>() && Player.armor[2].type == ModContent.ItemType<GeomancerPants>();
		}

		public void DrawArmorLayer(PlayerDrawSet info)
		{
			GeomancerPlayer modPlayer = info.drawPlayer.GetModPlayer<GeomancerPlayer>();
			if (modPlayer.SetBonusActive && modPlayer.storedGem != StoredGem.None && info.drawPlayer.armor[10].type == ItemID.None)
			{
				GeomancerDrawer.Draw(ModContent.Request<Texture2D>(AssetDirectory.GeomancerItem + "GeomancerHood_Head_Gems").Value, info, info.drawPlayer.bodyFrame, info.drawPlayer.headRotation);
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<ExoticGeodeArtifactItem>(), 2);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	[AutoloadEquip(EquipType.Body)]
	public class GeomancerRobe : ModItem, IArmorLayerDrawable
	{
		public override string Texture => AssetDirectory.GeomancerItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Geomancer's Cowl");
			//Tooltip.SetDefault("10% increased ranged damage");
		}

		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 20;
			Item.value = 6000;
			Item.defense = 6;
			Item.rare = ItemRarityID.Orange;
		}

		/*public override void UpdateEquip(Player Player)
        {
            Player.rangedDamage += 0.1f;
        }*/

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ModContent.ItemType<GeomancerHood>() && legs.type == ModContent.ItemType<GeomancerPants>();
		}

		public override void UpdateArmorSet(Player Player)
		{
			Player.setBonus = "Kills ands critical strikes with magic have a chance to drop magic gems\n" +
			"Each gem activates a different effect when picked up\n" +
			"Obtaining another gem stores the previous effect\n" +
			"Collecting all breifly activates every effect at once";

			Player.GetModPlayer<GeomancerPlayer>().SetBonusActive = true;
		}

		public void DrawArmorLayer(PlayerDrawSet info)
		{
			GeomancerPlayer modPlayer = info.drawPlayer.GetModPlayer<GeomancerPlayer>();
			if (modPlayer.SetBonusActive && modPlayer.storedGem != StoredGem.None && info.drawPlayer.armor[11].type == ItemID.None)
			{
				GeomancerDrawer.Draw(ModContent.Request<Texture2D>(AssetDirectory.GeomancerItem + "GeomancerRobe_Body_Gems").Value, info, info.drawPlayer.bodyFrame, info.drawPlayer.bodyRotation);
				GeomancerDrawer.Draw(ModContent.Request<Texture2D>(AssetDirectory.GeomancerItem + "GeomancerRobe_Body_Rims").Value, info, info.drawPlayer.bodyFrame, info.drawPlayer.bodyRotation);
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<ExoticGeodeArtifactItem>(), 3);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	public class GeomancerPants : ModItem, IArmorLayerDrawable
	{
		public override string Texture => AssetDirectory.GeomancerItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Geomancer's Greaves");
			//Tooltip.SetDefault("up to 20% ranged critical strike damage based on speed");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 20;
			Item.value = 4000;
			Item.defense = 5;
			Item.rare = ItemRarityID.Orange;
		}

		public void DrawArmorLayer(PlayerDrawSet info)
		{
			GeomancerPlayer modPlayer = info.drawPlayer.GetModPlayer<GeomancerPlayer>();

			if (modPlayer.SetBonusActive && modPlayer.storedGem != StoredGem.None && info.drawPlayer.armor[12].type == ItemID.None)
				GeomancerDrawer.Draw(ModContent.Request<Texture2D>(AssetDirectory.GeomancerItem + "GeomancerPants_Legs_Gems").Value, info, info.drawPlayer.legFrame, info.drawPlayer.bodyRotation);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<ExoticGeodeArtifactItem>(), 2);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class GeomancerItemDummy : ModItem
	{
		public override string Texture => AssetDirectory.GeomancerItem + "GeoDiamond";
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Effects of different gems:");
			
			Tooltip.SetDefault(
			"[i/s1:" + ItemType<GeoAmethyst>() + "]: Critical strikes partially ignore armor, with increased chance for missing enemy HP \n" +
			"[i/s1:" + ItemType<GeoTopaz>() + "]: +100 barrier. Gain a shield that points to your cursor and blocks attacks, consuming barrier \n" +
			"[i/s1:" + ItemType<GeoEmerald>() + "]: Immediately heal 20 hp. Hits have a chance to create a 5 HP life heart \n" +
			"[i/s1:" + ItemType<GeoSapphire>() + "]: Immediately refill mana. Hits have a chance to create 1 to 3 mana stars \n" +
			"[i/s1:" + ItemType<GeoAmethyst>() + "]: All strikes inflict Toxic Amethyst, a stacking poison debuff \n" +
			"[i/s1:" + ItemType<GeoRuby>() + "]: Hits have a chance to summon a Ruby Dagger to seek the struck enemy for 20% of the hit's damage");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 20;
		}
	}

	public static class GeomancerDrawer
	{
		public static void Draw(Texture2D texture, PlayerDrawSet info, Rectangle frame, float rotation) //TODO: Port this over to new system
		{
			Player armorOwner = info.drawPlayer;

			Vector2 drawPos = armorOwner.MountedCenter - Main.screenPosition - new Vector2(0, 3 - armorOwner.gfxOffY);
			float timerVar = (float)(Main.timeForVisualEffects * 0.05f % 2.4f / 2.4f) * 6.28f;
			float timer = (float)(Math.Sin(timerVar) / 2f) + 0.5f;

			Filters.Scene["RainbowArmor"].GetShader().Shader.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.005f);

			Filters.Scene["RainbowArmor2"].GetShader().Shader.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.005f);
			Filters.Scene["RainbowArmor2"].GetShader().Shader.Parameters["uOpacity"].SetValue(1.25f - timer);

			try
			{
				var value = new DrawData(
							texture,
							new Vector2((int)drawPos.X, (int)drawPos.Y),
							frame,
							GeomancerPlayer.GetArmorColor(armorOwner),
							rotation,
							new Vector2(frame.Width / 2, frame.Height / 2),
							1,
						   armorOwner.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
							0
						)
				{
					shader = armorOwner.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All ? GeomancerPlayer.shaderValue : 0
				};
				info.DrawDataCache.Add(value);
			}
			catch
			{
				Main.NewText("First wave of geomancer armor drawing not working");
			}

			try
			{
				for (float i = 0; i < 6.28f; i += 1.57f)
				{
					Vector2 offset = i.ToRotationVector2() * 2 * timer;
					var value2 = new DrawData(
							texture,
							new Vector2((int)drawPos.X, (int)drawPos.Y) + offset,
							frame,
							GeomancerPlayer.GetArmorColor(armorOwner) * 0.25f,
							rotation,
							new Vector2(frame.Width / 2, frame.Height / 2),
							1,
							armorOwner.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
							0
						)
					{
						shader = armorOwner.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All ? GeomancerPlayer.shaderValue2 : 0
					};

					info.DrawDataCache.Add(value2);
				}
			}
			catch
			{
				Main.NewText("Second wave of geomancer armor drawing not working");
			}
		}
	}
}