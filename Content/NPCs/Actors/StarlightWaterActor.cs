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
//using StarlightRiver.Content.Items.Vanity;

namespace StarlightRiver.Content.NPCs.Actors
{
	class StarlightWaterActor : ModNPC
	{
		public Item targetItem;

		public static float glowStrength;

		public override string Texture => AssetDirectory.Invisible;

		public override bool Autoload(ref string name)
		{
			StarlightPlayer.ResetEffectsEvent += ResetInventoryGlow;
			return true;
		}

		private void ResetInventoryGlow(StarlightPlayer player)
		{
			glowStrength = 0;
		}

		public override void SetDefaults()
		{
			npc.width = 1;
			npc.height = 1;
			npc.lifeMax = 100;
			npc.immortal = true;
			npc.dontTakeDamage = true;
			npc.friendly = true;
			npc.aiStyle = -1;
			npc.noGravity = true;
		}

		const int dustRange = 250;
		const int itemRange = 200;

		public override void AI()
		{
			if (npc.wet)
				npc.position.Y -= 1;

			if (Main.dayTime)
				npc.active = false;

			var dist = Vector2.Distance(Main.LocalPlayer.Center, npc.Center);

			if (dist < 500)
				glowStrength = 1 - dist / 500f;

            Vector2 pos = npc.Center + Vector2.UnitX * Main.rand.NextFloat(-dustRange, dustRange) + Vector2.UnitY * Main.rand.NextFloat(-6, 0);
            Tile	tile = Framing.GetTileSafely(pos);
            Tile	tileDown = Framing.GetTileSafely(pos + Vector2.UnitY * 16);

			if (((tile.liquid > 0 && tile.liquidType() == 0) || (tileDown.liquid > 0 && tileDown.liquidType() == 0)) && Main.rand.Next(10) > 3)//surface lights
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

            Vector2 pos2 = npc.Center + Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(-dustRange, dustRange);
            Tile	tile2 = Framing.GetTileSafely(pos2);

			if (tile2.liquid > 0 && tile2.liquidType() == 0 && Main.rand.Next(2) == 0)//under water lights
			{
				var d = Dust.NewDustPerfect(pos2, ModContent.DustType<Dusts.AuroraSuction>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(), 0, new Color(0, 50, 255), 0.5f);
				d.customData = new Dusts.AuroraSuctionData(this, Main.rand.NextFloat(0.4f, 0.5f));
			}

			if (targetItem is null)
			{
				for (int k = 0; k < Main.maxItems; k++)
				{
					var item = Main.item[k];

					if (Helpers.Helper.CheckCircularCollision(npc.Center, itemRange, item.Hitbox) && item.GetGlobalItem<TransformableItem>().transformType != 0 && item.wet)
						targetItem = item;
				}
			}
			else
			{
				if (targetItem.isBeingGrabbed)
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
						var player = Main.player[k];

						if (player.active && Vector2.Distance(player.Center, targetItem.Center) < 500)
							Helpers.Helper.UnlockEntry<Codex.Entries.StarlightWaterEntry>(player);
					}

					npc.active = false;
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
		public override bool CloneNewInstances => true;

		public override void SetDefaults(Item item) //TOOD: Probably move this later or make it modular? pee pee poo poo
		{ 
			if (item.type == ItemID.WoodHelmet) transformType = ModContent.ItemType<StarwoodHat>();
			if (item.type == ItemID.WoodBreastplate) transformType = ModContent.ItemType<StarwoodChest>();
			if (item.type == ItemID.WoodGreaves) transformType = ModContent.ItemType<StarwoodBoots>();

			if (item.type == ItemID.WoodenBoomerang) transformType = ModContent.ItemType<StarwoodBoomerang>();
			if (item.type == ItemID.WandofSparking) transformType = ModContent.ItemType<StarwoodStaff>();
		}

		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if(transformType != 0)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

				var tex = ModContent.GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
				spriteBatch.Draw(tex, position + frame.Size() / 2, null, new Color(130, 200, 255) * (StarlightWaterActor.glowStrength + (float)Math.Sin(StarlightWorld.rottime) * 0.2f), 0, tex.Size() / 2, 1, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);
			}

			return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
		}

		public override void PostUpdate(Item item)
		{
			if (transformTime > 0)
			{
				item.velocity *= 0.8f;

				if(item.wet)
					item.velocity.Y -= 0.15f;
			}

			if(windDown > 0)
			{
				var d = Dust.NewDustPerfect(item.Center + Vector2.One.RotatedByRandom(6.28f) * 16 * windDown / 240f, ModContent.DustType<Dusts.Aurora>(), Vector2.UnitY * Main.rand.NextFloat(-2, -4), 0, new Color(0, Main.rand.Next(255), 255), 1);
				d.customData = Main.rand.NextFloat(0.2f, 0.3f) * windDown / 240f;

				Lighting.AddLight(item.Center, new Vector3(10, 13, 25) * 0.08f * windDown / 240f);

				if (item.velocity.Y > 0)
					item.velocity.Y *= 0.7f;

				windDown--;
			}

			//Might move this later? idk. Kind of 2 things in 1 class but eh
			if(item.type == ItemID.FallenStar && item.wet)
			{
				item.active = false;
				NPC.NewNPC((int)item.Center.X, (int)item.Center.Y + 16, ModContent.NPCType<StarlightWaterActor>());

				for(int k = 0; k < 40; k++)
				{
					var rot = Main.rand.NextFloat(6.28f);
					Dust.NewDustPerfect(item.Center + Vector2.One.RotatedBy(rot) * 16, ModContent.DustType<Dusts.BlueStamina>(), Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(5));
				}
			}
		}

		public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			if(transformTime > 0)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

				var tex = ModContent.GetTexture("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall");

				var alphaMaster = (float)Math.Sin(transformTime / 300f * 3.14f);

				var alpha = (1.0f + (float)Math.Sin(transformTime / 75f * 3.14f) * 0.5f) * alphaMaster;
				spriteBatch.Draw(tex, item.Center + Vector2.UnitX * 20 - Main.screenPosition, null, new Color(100, 100 + (int)(50 * alpha), 255) * alpha, 0, new Vector2(tex.Width / 2, tex.Height - 15), 4.5f * alphaMaster, 0, 0);

				var alpha2 = (1.0f + (float)Math.Sin(transformTime / 150f * 3.14f) * 0.5f) * alphaMaster;
				spriteBatch.Draw(tex, item.Center - Main.screenPosition, null, new Color(100, 70 + (int)(50 * alpha2), 255) * alpha2, 0, new Vector2(tex.Width / 2, tex.Height - 15), 5 * alphaMaster, 0, 0);

				var alpha3 = (1.0f + (float)Math.Sin(transformTime / 37.5f * 3.14f) * 0.5f) * alphaMaster;
				spriteBatch.Draw(tex, item.Center + Vector2.UnitX * -20 - Main.screenPosition, null, new Color(100, 30 + (int)(50 * alpha3), 255) * alpha3, 0, new Vector2(tex.Width / 2, tex.Height - 15), 3f * alphaMaster, 0, 0);

				var rot = Main.rand.NextFloat(6.28f);
				Dust.NewDustPerfect(item.Center + Vector2.One.RotatedBy(rot) * 16, ModContent.DustType<Dusts.BlueStamina>(), Vector2.One.RotatedBy(rot) * -1.2f);

				var d = Dust.NewDustPerfect(item.Center + Vector2.One.RotatedBy(rot) * (16 + 8 * alphaMaster), ModContent.DustType<Dusts.Aurora>(), Vector2.UnitY * Main.rand.NextFloat(-9, -6), 0, new Color(0, Main.rand.Next(255), 255), 1);
				d.customData = Main.rand.NextFloat(0.2f, 0.5f) * alphaMaster;

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
			}

			if(windDown > 0)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

				var tex = ModContent.GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
				spriteBatch.Draw(tex, item.Center - Main.screenPosition, null, new Color(100, 150, 255) * (windDown / 240f), 0, tex.Size() / 2, (windDown / 240f) * 2, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
			}

			return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
		}
	}
}
