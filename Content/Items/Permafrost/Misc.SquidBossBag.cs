using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Permafrost
{
	class SquidBossBag : ModItem
	{
		public override string Texture => AssetDirectory.PermafrostItem + Name;

		public override int BossBagNPC => NPCType<Bosses.SquidBoss.SquidBoss>();

		public override bool CanRightClick()
		{
			return true;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return Color.Lerp(lightColor, Color.White, 0.4f);
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Treasure Bag (Auroracle)");

			ItemID.Sets.BossBag[Type] = true;
			ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;
		}

		public override void SetDefaults()
		{
			Item.consumable = true;
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;

			Item.width = 32;
			Item.height = 32;

			Item.maxStack = 999;
		}

		public override void OpenBossBag(Player Player)
		{
			int weapon = Main.rand.Next(4);

			for (int k = 0; k < 2; k++) //PORT: k < Main.MasterMode ? 3 : 2
			{
				switch (weapon % 2)
				{
					case 0: Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<OverflowingUrn>()); break;
					case 1: Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<AuroraBell>()); break;
						//TODO: Add drops as they're implemented
				}

				weapon++;
			}

			if (Main.rand.NextBool(3))
				Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<SquidFins>());
			//Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<ShatteredAegis>()); Expert item?
		}

		//This method is stolen from examplemod and I trust it to emulate vanilla accurately
		public override void PostUpdate()
		{
			Lighting.AddLight(Item.Center, Color.White.ToVector3() * 0.4f);

			if (Item.timeSinceItemSpawned % 12 == 0)
			{
				Vector2 center = Item.Center + new Vector2(0f, Item.height * -0.1f);

				Vector2 direction = Main.rand.NextVector2CircularEdge(Item.width * 0.6f, Item.height * 0.6f);
				float distance = 0.3f + Main.rand.NextFloat() * 0.5f;
				var velocity = new Vector2(0f, -Main.rand.NextFloat() * 0.3f - 1.5f);

				var dust = Dust.NewDustPerfect(center + direction * distance, DustID.SilverFlame, velocity);
				dust.scale = 0.5f;
				dust.fadeIn = 1.1f;
				dust.noGravity = true;
				dust.noLight = true;
				dust.alpha = 0;
			}
		}

		//This method is stolen from examplemod and I trust it to emulate vanilla accurately
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D texture = TextureAssets.Item[Item.type].Value;

			Rectangle frame;

			if (Main.itemAnimations[Item.type] != null)
				frame = Main.itemAnimations[Item.type].GetFrame(texture, Main.itemFrameCounter[whoAmI]);
			else
				frame = texture.Frame();

			Vector2 frameOrigin = frame.Size() / 2f;
			var offset = new Vector2(Item.width / 2 - frameOrigin.X, Item.height - frame.Height);
			Vector2 drawPos = Item.position - Main.screenPosition + frameOrigin + offset;

			float time = Main.GlobalTimeWrappedHourly;
			float timer = Item.timeSinceItemSpawned / 240f + time * 0.04f;

			time %= 4f;
			time /= 2f;

			if (time >= 1f)
				time = 2f - time;

			time = time * 0.5f + 0.5f;

			for (float i = 0f; i < 1f; i += 0.25f)
			{
				float radians = (i + timer) * MathHelper.TwoPi;
				spriteBatch.Draw(texture, drawPos + new Vector2(0f, 8f).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50), rotation, frameOrigin, scale, SpriteEffects.None, 0);
			}

			for (float i = 0f; i < 1f; i += 0.34f)
			{
				float radians = (i + timer) * MathHelper.TwoPi;
				spriteBatch.Draw(texture, drawPos + new Vector2(0f, 4f).RotatedBy(radians) * time, frame, new Color(140, 120, 255, 77), rotation, frameOrigin, scale, SpriteEffects.None, 0);
			}

			return true;
		}
	}
}
