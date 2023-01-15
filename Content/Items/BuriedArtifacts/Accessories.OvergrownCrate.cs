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

				Item dropped = Main.item[Item.NewItem(player.GetSource_OnHurt(attacker), player.getRect(), Type)];
				dropped.noGrabDelay = 60;
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
