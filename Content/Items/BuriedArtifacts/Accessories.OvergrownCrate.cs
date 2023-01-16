using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
	public class OvergrownCrate : ModItem
	{
		public override string Texture => AssetDirectory.ArtifactItem + Name;

		public override void Load()
		{
			StarlightPlayer.OnHitByNPCEvent += StarlightPlayer_OnHitByNPCEvent;

			StarlightPlayer.OnHitByProjectileEvent += StarlightPlayer_OnHitByProjectileEvent;
		}

		private void StarlightPlayer_OnHitByProjectileEvent(Player player, Projectile projectile, int damage, bool crit)
		{
			DropItem(player, projectile);
		}

		private void StarlightPlayer_OnHitByNPCEvent(Player player, NPC npc, int damage, bool crit)
		{
			DropItem(player, npc);
		}

		private void DropItem(Player player, Entity attacker)
		{
			if (Main.rand.NextBool(2))
			{
				//int Type = Main.rand.Next(new int[] { ModContent.ItemType<>(), ModContent.ItemType<>(), ModContent.ItemType<>(), ModContent.ItemType<>() });
				int Type = ModContent.ItemType<OvergrownCrateLifeLeaf>();


				Vector2 offset = new Vector2(-10f, -10f) * player.direction;
				Rectangle pos = new Rectangle(player.getRect().X + (int)offset.X, player.getRect().Y + (int)offset.Y, player.getRect().Width, player.getRect().Height);
				Item dropped = Main.item[Item.NewItem(player.GetSource_OnHurt(attacker), pos, Type)];
				dropped.noGrabDelay = 60;

				SoundEngine.PlaySound(SoundID.DoorOpen, player.Center);
			}
		}

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Getting hit has a chance to drops a random buff item");
			SacrificeTotal = 1;
		}

		public override void SetDefaults()
		{
			Item.accessory = true;
			Item.Size = new Vector2(32);
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Green;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<OvergrownCratePlayer>().equipped = true;
		}
	}

	abstract class OvergrownCrateDroppedItem : ModItem
	{
		public override string Texture => AssetDirectory.ArtifactItem + Name;

		public string displayName;

		public int buffID;

		public Color bloomColor;

		protected OvergrownCrateDroppedItem(string displayName, int buffID, Color bloomColor) : base() 
		{
			this.displayName = displayName;
			this.buffID = buffID;
			this.bloomColor = bloomColor;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(displayName);
			Tooltip.SetDefault("You shouldn't see this");
		}

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = 1;
		}

		public override bool ItemSpace(Player Player)
		{
			return true;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D glowTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Color drawColor = bloomColor;
			drawColor.A = 0;

			spriteBatch.Draw(glowTex, Item.Center - Main.screenPosition, null, drawColor * 0.5f, 0, glowTex.Size() / 2, 0.75f, SpriteEffects.None, 0f);

			return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
		}

		public override bool OnPickup(Player Player)
		{
			SoundEngine.PlaySound(SoundID.Grab, Player.position);
			Player.AddBuff(buffID, 480);

			return false;
		}
	}

	internal class OvergrownCratePlayer: ModPlayer
	{
		public bool equipped;

		public int frame;
		public int frameCounter;
		public override void ResetEffects()
		{
			if (!equipped)
				frameCounter = 0;
			else
			{
				frameCounter++;

				if ((int)(frameCounter * Math.Abs(Player.velocity.X)) >= 30)
				{
					frameCounter = 0;
					frame++;
				}

				if (frame >= 4 || frame < 0)
					frame = 0;

				if (Player.velocity.Length() < 0.1f)
				{
					frame = 0;
					frameCounter = 0;
				}
			}

			equipped = false;
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			if (drawInfo.shadow != 0f)
				return;

			Texture2D tankTexture = ModContent.Request<Texture2D>(AssetDirectory.ArtifactItem + "OvergrownCrate_BackOpening").Value;

			Player drawplayer = drawInfo.drawPlayer;

			Item heldItem = drawplayer.HeldItem;

			if (equipped && !drawplayer.frozen && !drawplayer.dead && (!drawplayer.wet || !heldItem.noWet) && drawplayer.wings <= 0)
			{
				SpriteEffects spriteEffects = drawplayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

				var drawPos = new Vector2((int)(drawplayer.position.X - Main.screenPosition.X + drawplayer.width / 2 - 12 * drawplayer.direction),
					(int)(drawplayer.position.Y - Main.screenPosition.Y + drawplayer.height / 2 + 2f * drawplayer.gravDir - 2f * drawplayer.gravDir + drawplayer.gfxOffY + 50));

				Rectangle rect = tankTexture.Frame(verticalFrames: 4, frameY: frame);

				var tankData = new DrawData(tankTexture, drawPos, rect,
					drawInfo.colorArmorBody, drawplayer.bodyRotation, tankTexture.Size() / 2f, 1f, spriteEffects, 0);

				drawInfo.DrawDataCache.Add(tankData);
			}
		}
	}

	class OvergrownCrateLifeLeaf : OvergrownCrateDroppedItem
	{
		public OvergrownCrateLifeLeaf() : base("Life Leaf", BuffID.Regeneration, Color.GreenYellow) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(displayName);
			Tooltip.SetDefault("You shouldn't see this");

			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 5));
			ItemID.Sets.AnimatesAsSoul[Type] = true;
		}
	}

	/*class OvergrownCrateBluntWood : OvergrownCrateDroppedItem
	{
		
	}

	class OvergrownCrateSharpWood : OvergrownCrateDroppedItem
	{
		
	}

	class OvergrownCrateRustyNail : OvergrownCrateDroppedItem
	{
		
	}*/
}
