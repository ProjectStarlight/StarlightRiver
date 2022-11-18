using CsvHelper.TypeConversion;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using StarlightRiver.Content.Items.Geomancer;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
    public class ArchaeologistsWhip : ModItem
    {
        public override string Texture => AssetDirectory.ArtifactItem + Name;

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Archaeologist's Whip");
            Tooltip.SetDefault("Strike enemies to make them drop treasure \nCollect treasure to empower minions");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<ArchaeologistsWhip_Whip>(), 15, 1.2f, 5f, 25);
            Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Green;
        }
    }

    public class ArchaeologistsWhip_Whip : BaseWhip
    {
        public override string Texture => AssetDirectory.ArtifactItem + Name;

		protected bool empowered => Main.player[Projectile.owner].HasBuff(ModContent.BuffType<ArchaeologistsBuff>());

        public ArchaeologistsWhip_Whip() : base("Archaeologist's Whip", 15, 0.87f, new Color(153, 122, 97)) { xFrames = 1; yFrames = 5; }

        public override int SegmentVariant(int segment)
        {
            int variant;
            switch (segment)
            {
                default:
                    variant = 1;
                    break;
                case 6:
				case 7:
				case 8:
				case 9:
					variant = 2;
                    break;
                case 10:
				case 11:
                case 12:
                case 13:
                    variant = 3;
                    break;
            }
            return variant;
        }

		public override void ArcAI()
		{
			xFrame = 0;
		}

		public override bool ShouldDrawSegment(int segment) => true;// segment % 2 == 0;

		public override void DrawBehindWhip(ref Color lightColor)
		{
			if (!empowered)
				return;

			List<Vector2> points = new List<Vector2>();
			points.Clear();
			SetPoints(points);
			Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture + "_Glow");
			Rectangle whipFrame = texture.Frame(xFrames, yFrames, xFrame, 0);
			int height = whipFrame.Height;
			Vector2 firstPoint = points[0];
			for (int i = 0; i < points.Count - 1; i++)
			{
				Vector2 origin = whipFrame.Size() * 0.5f;
				bool draw = true;
				if (i == 0)
					origin.Y += _handleOffset;
				else if (i == points.Count - 2)
					whipFrame.Y = height * (yFrames - 1);
				else
				{
					whipFrame.Y = height * SegmentVariant(i);
					draw = ShouldDrawSegment(i);
				}

				Vector2 difference = points[i + 1] - points[i];
				if (draw)
				{
					Color alpha = Color.Gold;
					alpha.A = 0;
					float rotation = difference.ToRotation() - MathHelper.PiOver2;
					Main.EntitySpriteDraw(texture.Value, points[i] - Main.screenPosition, whipFrame, alpha * 0.5f, rotation, origin, Projectile.scale, SpriteEffects.None, 0);
				}
				firstPoint += difference;
			}
		}

		public override Color? GetAlpha(Color lightColor) 
        {
            Color minLight = lightColor;
            Color minColor = new Color(10, 25, 33);
            if (minLight.R < minColor.R)
                minLight.R = minColor.R;            
            if (minLight.G < minColor.G)
                minLight.G = minColor.G;            
            if (minLight.B < minColor.B)
                minLight.B = minColor.B;
            return minLight;
        }

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			int[] treasure = new int[] {
			ModContent.ItemType<AWhip_BlueGem>(),
			ModContent.ItemType<AWhip_GreenGem>(),
			ModContent.ItemType<AWhip_RedGem>(),
			ModContent.ItemType<AWhip_Coin>(),
			ModContent.ItemType<AWhip_Necklace>(),
			};

			if (Main.rand.NextBool(9))
			{
				if (Main.rand.NextBool(100))
					Item.NewItem(target.GetSource_Loot(), target.Hitbox, ModContent.ItemType<AWhip_Cloud>());
				else
					Item.NewItem(target.GetSource_Loot(), target.Hitbox, treasure[Main.rand.Next(treasure.Length)]);
			}
			base.OnHitNPC(target, damage, knockback, crit);
		}
	}

	public abstract class ArchaeologistsWhipTreasure : ModItem
	{
		public override string Texture => AssetDirectory.ArtifactItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Treasure");
			Tooltip.SetDefault("You shouldn't see this");
		}

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = 1;
		}

		public override bool ItemSpace(Player Player) => true;

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			if (Main.rand.NextBool(60))
				Dust.NewDustPerfect(Item.Center + Main.rand.NextVector2Circular(8, 8), ModContent.DustType<Dusts.ArtifactSparkles.GoldArtifactSparkle>(), Vector2.Zero);
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D glowTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Color drawColor = Color.Gold;
			drawColor.A = 0;
			spriteBatch.Draw(glowTex, Item.Center - Main.screenPosition, null, drawColor, 0, glowTex.Size() / 2, 0.55f, SpriteEffects.None, 0f);
			return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
		}

		public override bool OnPickup(Player Player)
		{
			SoundEngine.PlaySound(SoundID.Grab, Player.position);
			Player.AddBuff(ModContent.BuffType<ArchaeologistsBuff>(), 240); 
			return false;
		}
	}

	public class AWhip_BlueGem : ArchaeologistsWhipTreasure { }

	public class AWhip_GreenGem : ArchaeologistsWhipTreasure { }

	public class AWhip_RedGem : ArchaeologistsWhipTreasure { }

	public class AWhip_Coin : ArchaeologistsWhipTreasure { }

	public class AWhip_Cloud : ArchaeologistsWhipTreasure { }

	public class AWhip_Necklace : ArchaeologistsWhipTreasure { }

	public class ArchaeologistsBuff : ModBuff
	{
		public override string Texture => AssetDirectory.ArtifactItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Treasure buff");
			Description.SetDefault("Your minions do more damage");
		}
	}

	public class ArchaeologistsWhipGProj : GlobalProjectile
	{
		public override void AI(Projectile projectile)
		{
			Player player = Main.player[projectile.owner];
			if (projectile.minion && player.HasBuff(ModContent.BuffType<ArchaeologistsBuff>()) && Main.rand.NextBool(90))
				Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(12, 12), ModContent.DustType<Dusts.ArtifactSparkles.GoldArtifactSparkle>(), Vector2.Zero);
		}

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[projectile.owner];
			if (projectile.minion && player.HasBuff(ModContent.BuffType<ArchaeologistsBuff>()))
				damage = (int)(damage * 1.2f);
		}
	}
}
