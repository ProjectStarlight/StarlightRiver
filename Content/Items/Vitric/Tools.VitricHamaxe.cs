using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	internal class VitricHamaxe : ModItem, IGlowingItem
	{
		public int heat = 0;
		public int heatTime = 0;

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vitric Hamaxe");
			Tooltip.SetDefault("<right> to charge heat\nHeat increases speed\nHeat dissipates over time");
		}

		public override void SetDefaults()
		{
			Item.damage = 26;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 32;
			Item.useTime = 15;
			Item.useAnimation = 32;
			Item.axe = 20;
			Item.hammer = 60;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 3.5f;
			Item.value = 1000;
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item18;
			Item.useTurn = true;
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(heat);
			writer.Write(heatTime);
		}

		public override void NetReceive(BinaryReader reader)
		{
			heat = reader.ReadInt32();
			heatTime = reader.ReadInt32();
		}

		public override void HoldItem(Player Player)
		{
			ControlsPlayer cPlayer = Player.GetModPlayer<ControlsPlayer>();

			if (Main.myPlayer == Player.whoAmI)
				Player.GetModPlayer<ControlsPlayer>().rightClickListener = true;

			Item.noMelee = false;

			if (cPlayer.mouseRight)
			{
				Item.noMelee = true;
				Player.itemAnimation = 13;

				if (heat < 100)
				{
					heat++;

					Vector2 off = Vector2.One.RotatedByRandom(6.28f) * 20;
					Dust.NewDustPerfect(Player.MountedCenter + (Vector2.One * 40).RotatedBy(Player.itemRotation - (Player.direction == 1 ? MathHelper.PiOver2 : MathHelper.Pi)) + off, DustType<Dusts.Stamina>(), -off * 0.05f);
				}

				if (heat == 98)
					Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, Player.Center);

			}
		}

		public override float UseTimeMultiplier(Player Player)
		{
			return 1 + heat / 100f;
		}

		public override void UpdateInventory(Player Player)
		{
			heatTime++;

			if (heatTime >= 10)
			{
				if (heat > 0)
					heat--;

				heatTime = 0;
			}
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			Texture2D tex = Request<Texture2D>(Texture + "Glow").Value;
			Color color = Color.White * (heat / 100f);

			spriteBatch.Draw(tex, position, frame, color, 0, origin, scale, 0, 0);
		}

		public void DrawGlowmask(PlayerDrawSet info)
		{
			Player Player = info.drawPlayer;

			if (Player.itemAnimation == 0)
				return;

			Texture2D tex = Request<Texture2D>(Texture + "Glow").Value;
			Color color = Color.White * (heat / 100f);
			Vector2 origin = Player.direction == 1 ? new Vector2(0, tex.Height) : new Vector2(tex.Width, tex.Height);

			var data = new DrawData(tex, info.ItemLocation - Main.screenPosition, null, color, Player.itemRotation, origin, Item.scale, Player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			info.DrawDataCache.Add(data);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<SandstoneChunk>(), 10);
			recipe.AddIngredient(ItemType<VitricOre>(), 20);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}