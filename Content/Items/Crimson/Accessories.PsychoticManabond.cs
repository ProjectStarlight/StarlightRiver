using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.Manabonds;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	internal class PsychoticManabond : Manabond
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public PsychoticManabond() : base("Psychotic Manabond", "Your minions can store 40 mana\nYour minions siphon 6 mana per second from you untill full\nYour minions spend 6 mana per second to emit an aura inflicting {{BUFF:Psychosis}}") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 15);
			Item.rare = ItemRarityID.Orange;
		}

		public override void MinionAI(Projectile minion, ManabondProjectile mp)
		{
			if (mp.timer % 10 == 0 && mp.mana >= 40 && mp.target != null)
			{
				mp.mana -= 1;

				if (Main.myPlayer == minion.owner)
				{
					Projectile.NewProjectile(minion.GetSource_FromThis(), minion.Center, Vector2.Zero, ModContent.ProjectileType<PsychosisAura>(), 1, 0, minion.owner, minion.identity);
				}
			}
		}
	}

	internal class PsychosisAura : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public ref float Following => ref Projectile.ai[0];

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawHallucinations;
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.timeLeft = 11;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
		}

		public override void AI()
		{
			GraymatterBiome.forceGrayMatter = true;

			var parent = Main.projectile.FirstOrDefault(n => n.active && n.identity == Following);

			if (parent is null)
			{
				Projectile.timeLeft = 0;
			}
			else
			{
				Projectile.Center = parent.Center;
			}

			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 1.5f);
		}

		public override bool? CanHitNPC(NPC target)
		{
			return Vector2.Distance(target.Center, Projectile.Center) < 64 ? null : false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			BuffInflictor.Inflict<Psychosis>(target, 60);
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Assets.Misc.GlowRing.Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * 0.1f, 0, tex.Size() / 2f, 128f / tex.Width, 0, 0);
		}

		private void DrawHallucinations(SpriteBatch batch)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == Type)
				{
					Texture2D tex = Assets.Keys.GlowAlpha.Value;
					batch.Draw(tex, proj.Center - Main.screenPosition, null, new Color(255, 255, 255, 0), 0, tex.Size() / 2f, 64 * 5f / tex.Width, 0, 0);
				}
			}
		}
	}
}
