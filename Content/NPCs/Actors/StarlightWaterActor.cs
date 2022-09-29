using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Reflection;
using Terraria.ID;
using StarlightRiver.Content.Items.Starwood;
using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Vanity;
using StarlightRiver.Content.Items.Misc;

namespace StarlightRiver.Content.NPCs.Actors
{
	class StarlightWaterActor : ModNPC
	{
		public Item targetItem;

		public static float glowStrength;

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			StarlightPlayer.ResetEffectsEvent += ResetInventoryGlow;
		}

		private void ResetInventoryGlow(StarlightPlayer Player)
		{
			glowStrength = 0;
		}

		public override void SetDefaults()
		{
			NPC.width = 1;
			NPC.height = 1;
			NPC.lifeMax = 100;
			NPC.immortal = true;
			NPC.dontTakeDamage = true;
			NPC.friendly = true;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
		}

		const int dustRange = 250;
		const int ItemRange = 200;

		public override void AI()
		{
			if (NPC.wet)
				NPC.position.Y -= 1;

			if (Main.dayTime)
				NPC.active = false;

			var dist = Vector2.Distance(Main.LocalPlayer.Center, NPC.Center);

			if (dist < 500)
				glowStrength = 1 - dist / 500f;

            Vector2 pos = NPC.Center + Vector2.UnitX * Main.rand.NextFloat(-dustRange, dustRange) + Vector2.UnitY * Main.rand.NextFloat(-6, 0);
            Tile	tile = Framing.GetTileSafely(pos);
            Tile	tileDown = Framing.GetTileSafely(pos + Vector2.UnitY * 16);

			if (((tile .LiquidAmount > 0 && tile.LiquidType == LiquidID.Water) || (tileDown .LiquidAmount > 0 && tileDown.LiquidType == LiquidID.Water)) && Main.rand.Next(10) > 3)//surface lights
			{
				var d = Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.AuroraSuction>(), Vector2.Zero, 200, new Color(Main.rand.Next(30) == 0 ? 200 : 0, Main.rand.Next(150), 255));
				d.customData = new Dusts.AuroraSuctionData(this, Main.rand.NextFloat(0.6f, 0.8f));

                if (Main.rand.NextBool())
                {
					bool red = Main.rand.Next(35) == 0;
					bool green = Main.rand.Next(15) == 0 && !red;
					Color color = new Color(red ? 255 : Main.rand.Next(10), green ? 255 : Main.rand.Next(100), Main.rand.Next(240, 255));

					Dust.NewDustPerfect(pos + new Vector2(0, Main.rand.Next(-4, 1)), ModContent.DustType<Dusts.VerticalGlow>(), Vector2.UnitX * Main.rand.NextFloat(-0.15f, 0.15f), 200, color);
				}
			}

            Vector2 pos2 = NPC.Center + Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(-dustRange, dustRange);
            Tile	tile2 = Framing.GetTileSafely(pos2);

			if (tile2 .LiquidAmount > 0 && tile2.LiquidType == LiquidID.Water && Main.rand.Next(2) == 0)//under water lights
			{
				var d = Dust.NewDustPerfect(pos2, ModContent.DustType<Dusts.AuroraSuction>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(), 0, new Color(0, 50, 255), 0.5f);
				d.customData = new Dusts.AuroraSuctionData(this, Main.rand.NextFloat(0.4f, 0.5f));
			}

			if (targetItem is null)
			{
				for (int k = 0; k < Main.maxItems; k++)
				{
					var Item = Main.item[k];

					if (Helpers.Helper.CheckCircularCollision(NPC.Center, ItemRange, Item.Hitbox) && Item.GetGlobalItem<TransformableItem>().transformType != 0 && Item.wet)
						targetItem = Item;
				}
			}
			else
			{
				if (targetItem.beingGrabbed)
				{
					targetItem.GetGlobalItem<TransformableItem>().transformTime = 0;
					targetItem = null;
					return;
				}

				targetItem.GetGlobalItem<TransformableItem>().transformTime++;

				Lighting.AddLight(targetItem.Center, new Vector3(10, 13, 25) * 0.04f * targetItem.GetGlobalItem<TransformableItem>().transformTime / 300f);

				if (targetItem.GetGlobalItem<TransformableItem>().transformTime > 300)
				{
					targetItem.SetDefaults(targetItem.GetGlobalItem<TransformableItem>().transformType);
					targetItem.velocity.Y -= 5;
					targetItem.GetGlobalItem<TransformableItem>().windDown = 240;

					for (int i = 0; i < 40; i++)
					{
						Dust.NewDustPerfect(targetItem.Center, ModContent.DustType<Dusts.BlueStamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10));
					}

					for(int k = 0; k < Main.maxPlayers; k++)
					{
						var Player = Main.player[k];

						if (Player.active && Vector2.Distance(Player.Center, targetItem.Center) < 500)
							Helpers.Helper.UnlockCodexEntry<Codex.Entries.StarlightWaterEntry>(Player);
					}

					NPC.active = false;
				}
			}
		}
	}

	public class TransformableItem : GlobalItem
	{
		public int transformType;
		public int transformTime;
		public int windDown;

		public override bool InstancePerEntity => true;

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			return item.TryGetGlobalItem<TransformableItem>(out var gi) ? gi : base.Clone(item, itemClone);
		}

		public override void SetDefaults(Item Item) //TOOD: Probably move this later or make it modular? pee pee poo poo
		{ 
			if (Item.type == ItemID.WoodHelmet) transformType = ModContent.ItemType<StarwoodHat>();
			if (Item.type == ItemID.WoodBreastplate) transformType = ModContent.ItemType<StarwoodChest>();
			if (Item.type == ItemID.WoodGreaves) transformType = ModContent.ItemType<StarwoodBoots>();

			if (Item.type == ItemID.WoodenBoomerang) transformType = ModContent.ItemType<StarwoodBoomerang>();
			if (Item.type == ItemID.WandofSparking) transformType = ModContent.ItemType<StarwoodStaff>();
			if (Item.type == ModContent.ItemType<Sling>()) transformType = ModContent.ItemType<StarwoodSlingshot>();

            if (Item.vanity)
            {
				if (Item.headSlot != -1 && Item.type != ModContent.ItemType<AncientStarwoodHat>()) transformType = ModContent.ItemType<AncientStarwoodHat>();
				else if (Item.bodySlot != -1 && Item.type != ModContent.ItemType<AncientStarwoodChest>()) transformType = ModContent.ItemType<AncientStarwoodChest>();
				else if (Item.legSlot != -1 && Item.type != ModContent.ItemType<AncientStarwoodBoots>()) transformType = ModContent.ItemType<AncientStarwoodBoots>();
			}
		}

		public override bool PreDrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if(transformType != 0)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

				var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
				spriteBatch.Draw(tex, position + frame.Size() / 2, null, new Color(130, 200, 255) * (StarlightWaterActor.glowStrength + (float)Math.Sin(StarlightWorld.rottime) * 0.2f), 0, tex.Size() / 2, 1, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);
			}

			return base.PreDrawInInventory(Item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
		}

		public override void PostUpdate(Item Item)
		{
			if (transformTime > 0)
			{
				Item.velocity *= 0.8f;

				if(Item.wet)
					Item.velocity.Y -= 0.15f;
			}

			if(windDown > 0)
			{
				var d = Dust.NewDustPerfect(Item.Center + Vector2.One.RotatedByRandom(6.28f) * 16 * windDown / 240f, ModContent.DustType<Dusts.Aurora>(), Vector2.UnitY * Main.rand.NextFloat(-2, -4), 0, new Color(0, Main.rand.Next(255), 255), 1);
				d.customData = Main.rand.NextFloat(0.2f, 0.3f) * windDown / 240f;

				Lighting.AddLight(Item.Center, new Vector3(10, 13, 25) * 0.08f * windDown / 240f);

				if (Item.velocity.Y > 0)
					Item.velocity.Y *= 0.7f;

				windDown--;
			}

			//Might move this later? idk. Kind of 2 things in 1 class but eh
			if(Item.type == ItemID.FallenStar && Item.wet)
			{
				Item.active = false;
				NPC.NewNPC(null, (int)Item.Center.X, (int)Item.Center.Y + 16, ModContent.NPCType<StarlightWaterActor>());

				for(int k = 0; k < 40; k++)
				{
					var rot = Main.rand.NextFloat(6.28f);
					Dust.NewDustPerfect(Item.Center + Vector2.One.RotatedBy(rot) * 16, ModContent.DustType<Dusts.BlueStamina>(), Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(5));
				}
			}
		}

		public override bool PreDrawInWorld(Item Item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			if(transformTime > 0)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

				var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall").Value;

				var alphaMaster = (float)Math.Sin(transformTime / 300f * 3.14f);

				var alpha = (1.0f + (float)Math.Sin(transformTime / 75f * 3.14f) * 0.5f) * alphaMaster;
				spriteBatch.Draw(tex, Item.Center + Vector2.UnitX * 20 - Main.screenPosition, null, new Color(100, 100 + (int)(50 * alpha), 255) * alpha, 0, new Vector2(tex.Width / 2, tex.Height - 15), 4.5f * alphaMaster, 0, 0);

				var alpha2 = (1.0f + (float)Math.Sin(transformTime / 150f * 3.14f) * 0.5f) * alphaMaster;
				spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, new Color(100, 70 + (int)(50 * alpha2), 255) * alpha2, 0, new Vector2(tex.Width / 2, tex.Height - 15), 5 * alphaMaster, 0, 0);

				var alpha3 = (1.0f + (float)Math.Sin(transformTime / 37.5f * 3.14f) * 0.5f) * alphaMaster;
				spriteBatch.Draw(tex, Item.Center + Vector2.UnitX * -20 - Main.screenPosition, null, new Color(100, 30 + (int)(50 * alpha3), 255) * alpha3, 0, new Vector2(tex.Width / 2, tex.Height - 15), 3f * alphaMaster, 0, 0);

				var rot = Main.rand.NextFloat(6.28f);
				Dust.NewDustPerfect(Item.Center + Vector2.One.RotatedBy(rot) * 16, ModContent.DustType<Dusts.BlueStamina>(), Vector2.One.RotatedBy(rot) * -1.2f);

				var d = Dust.NewDustPerfect(Item.Center + Vector2.One.RotatedBy(rot) * (16 + 8 * alphaMaster), ModContent.DustType<Dusts.Aurora>(), Vector2.UnitY * Main.rand.NextFloat(-9, -6), 0, new Color(0, Main.rand.Next(255), 255), 1);
				d.customData = Main.rand.NextFloat(0.2f, 0.5f) * alphaMaster;

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
			}

			if(windDown > 0)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

				var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
				spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, new Color(100, 150, 255) * (windDown / 240f), 0, tex.Size() / 2, (windDown / 240f) * 2, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
			}

			return base.PreDrawInWorld(Item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
		}
	}
}
