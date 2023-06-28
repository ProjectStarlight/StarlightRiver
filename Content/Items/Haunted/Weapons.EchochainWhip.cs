using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Enums;
using Terraria.GameContent.Creative;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Haunted
{
	public class EchochainWhip : ModItem
	{
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Echochain");
			Tooltip.SetDefault("Chains enemies together, sharing summon damage between them\nHold and release <right> to chain enemies near you to the ground, chaining all effected enemies");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToWhip(ModContent.ProjectileType<EchochainWhipProjectile>(), 15, 3f, 3.75f, 40);
			Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(gold: 1));
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.GoldBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.PlatinumBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class EchochainWhipProjectile : BaseWhip
	{
		public List<Vector2> tipPositions = new();
		public List<float> tipRotations = new();

		public List<NPC> hitTargets = new();
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public EchochainWhipProjectile() : base("Echochain", 40, 1.25f, new Color(150, 255, 20) * 0.5f) { }

		public override void ArcAI()
		{
			if (Projectile.ai[0] > flyTime * 0.45f)
			{
				var points = new List<Vector2>();
				points.Clear();
				SetPoints(points);

				Vector2 tipPos = points[39];

				tipPositions.Add(tipPos);
				if (tipPositions.Count > 15)
					tipPositions.RemoveAt(0);

				Vector2 difference = points[39] - points[38];
				float rotation = difference.ToRotation() - MathHelper.PiOver2;

				tipRotations.Add(rotation);
				if (tipRotations.Count > 15)
					tipRotations.RemoveAt(0);

				Dust.NewDustPerfect(tipPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(1f, 1f), 0, new Color(100, 200, 10), 0.5f);

				Dust.NewDustPerfect(tipPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f), 0, new Color(150, 255, 25), 0.35f);
			}
		}

		public override void DrawBehindWhip(ref Color lightColor)
		{
			Texture2D texBlur = ModContent.Request<Texture2D>(Texture + "_TipBlur").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_TipGlow").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);
			Rectangle whipFrame = texture.Frame(1, 5, 0, 0);
			int height = whipFrame.Height;

			float fadeOut = 1f;
			if (Projectile.ai[0] > flyTime * 0.4f && Projectile.ai[0] < flyTime * 0.9f)
				fadeOut *= 1f - (Projectile.ai[0] - flyTime * 0.4f) / (flyTime * 0.5f);
			else if (Projectile.ai[0] >= flyTime * 0.9f)
				fadeOut = 0f;

			for (int i = 15; i > 0; i--)
			{
				float fade = 1 - (15f - i) / 15f;

				if (i > 0 && i < tipPositions.Count)
				{
					whipFrame.Y = height * 4;
					Color color = Color.Lerp(new Color(20, 135, 15, 0), new Color(100, 200, 10, 0), fade);
					Main.EntitySpriteDraw(texture.Value, tipPositions[i] - Main.screenPosition, whipFrame, Color.White * fade * fadeOut, tipRotations[i], whipFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
					
					Main.spriteBatch.Draw(texBlur, tipPositions[i] - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut, tipRotations[i], texBlur.Size() / 2f, Projectile.scale, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, color * fade * fadeOut, 0f, bloomTex.Size() / 2f, 1f * fade, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, color * fade * fadeOut * 0.5f, 0f, bloomTex.Size() / 2f, 1.5f * fade, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut * 0.6f, 0f, bloomTex.Size() / 2f, 0.8f * fade, 0f, 0f);

					if (i < 15 && i + 1 < tipPositions.Count)
					{
						Vector2 newPosition = Vector2.Lerp(tipPositions[i], tipPositions[i + 1], 0.5f);
						float newRotation = MathHelper.Lerp(tipRotations[i], tipRotations[i + 1], 0.5f);

						Main.EntitySpriteDraw(texture.Value, newPosition - Main.screenPosition, whipFrame, Color.White * fade * fadeOut, newRotation, whipFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

						Main.spriteBatch.Draw(texBlur, newPosition - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut, newRotation, texBlur.Size() / 2f, Projectile.scale, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, color * fade * fadeOut, 0f, bloomTex.Size() / 2f, 1f * fade, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, color * fade * fadeOut * 0.5f, 0f, bloomTex.Size() / 2f, 1.5f * fade, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut * 0.6f, 0f, bloomTex.Size() / 2f, 0.8f * fade, 0f, 0f);
					}
				}
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			hitTargets.Add(target);
		}

		public override void Kill(int timeLeft)
		{

		}
	}

	class EchochainEdge
	{
		public EchochainNode start;
		public EchochainNode end;
		public int timer;

		public EchochainEdge(EchochainNode start, EchochainNode end, int timer)
		{
			this.start = start;
			this.end = end;
			this.timer = timer;
		}
	}

	class EchochainNode
	{
		public List<EchochainEdge> edges;
		public NPC npc;

		public EchochainNode(List<EchochainEdge> edges, NPC npc)
		{
			this.edges = edges;
			this.npc = npc;
		}
	}

	class EchochainSystem : ModSystem
	{
		public static List<EchochainNode> nodes = new();
		public static List<EchochainEdge> edges = new();

		public override void PostUpdateEverything()
		{
			
		}

		public static void AddNPCS(List<NPC> npcsToAdd)
		{
			List<EchochainNode> list = new();

			for (int i = 0; i < npcsToAdd.Count; i++)
			{
				NPC npc = npcsToAdd[i];

				EchochainNode node = nodes.FirstOrDefault(n => n.npc == npc);

				if (node == null)
				{
					node = new EchochainNode(new List<EchochainEdge>(), npc);
					nodes.Add(node);
				}

				list.Add(node);
			}

			for (int i = 0; i < list.Count; i++)
			{
				
			}
		}
	}

	class EchochainGlobalNPC : GlobalNPC
	{
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			/*EchochainChain chainToUse = null;

			foreach (EchochainChain chain in EchochainSystem.chains)
			{
				if (chain.chainedTargets.Contains(npc))
				{
					chainToUse = chain;
					break;
				}
			}

			if (chainToUse != null)
			{
				EchochainChainAttack attack = new(60, npc, chainToUse.chainedTargets);
				EchochainSystem.attacks.Add(attack);
			}*/
		}
	}
}