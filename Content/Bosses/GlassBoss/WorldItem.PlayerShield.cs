using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
	class PlayerShield : InworldItem
	{
		public override int NPCType => NPCType<PlayerShieldNPC>();

		public override string Texture => AssetDirectory.GlassBoss + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Woven Shield");
		}

		public override void SetDefaults()
		{
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useTime = 16;
			item.useAnimation = 16;
			item.noUseGraphic = true;
			item.channel = true;
		}

		public override bool CanUseItem(Player player)
		{
			return (inWorldNPC as PlayerShieldNPC).ShieldPower > 0;
		}

		public override bool UseItem(Player player)
		{
			return true;
		}
	}

	class PlayerShieldNPC : InworldItemNPC, IDrawAdditive
	{
		public Vector2 startPoint;
		public Vector2 endPoint;
		public VitricBoss parent;

		public override string Texture => AssetDirectory.GlassBoss + "PlayerShieldOut";

		public ref float Timer => ref npc.ai[0];
		public ref float StaleTimer => ref npc.ai[1];
		public ref float ShieldPower => ref npc.ai[2];

		public override bool CanPickup(Player player)
		{
			return StaleTimer > 60;
		}

		public override void SetDefaults()
		{
			npc.lifeMax = 200;
			npc.damage = 1;
			npc.dontTakeDamage = true;
			npc.width = 32;
			npc.height = 32;
			npc.noGravity = true;
			npc.aiStyle = -1;
		}

		public override void AI()
		{
			Timer++;
			StaleTimer++;

			if (held)
			{
				npc.Center = owner.Center + Vector2.UnitX.RotatedBy(npc.rotation) * 30;

				if (!owner.channel || ShieldPower <= 0)
				{
					owner.channel = false;
					startPoint = Vector2.Zero;

					if (npc.scale > 0)
						npc.scale -= 0.1f;

					if (ShieldPower < 240)
						ShieldPower++;
				}
				else if (ShieldPower > 0)
				{
					if (npc.scale < 1)
						npc.scale += 0.1f;

					npc.rotation = (Main.MouseWorld - owner.Center).ToRotation();

					ShieldPower--;
				}

				if(startPoint != Vector2.Zero)
				{
					for(int k = 0; k < parent.orbitals.Count; k++) //check for hitting the shields
					{
						var victim = parent.orbitals[k];

						if(victim.active && victim.type == NPCType<FinalOrbital>())
						{
							if(Helpers.Helper.CheckLinearCollision(startPoint, endPoint, victim.Hitbox, out Vector2 possibleEnd))
							{
								endPoint = possibleEnd;
								victim.StrikeNPC(5, 0, 0);
								CombatText.NewText(victim.Hitbox, Color.White, 5);
								return;
							}
						}
					}

					for (int k = 0; k < 160; k++) //raycast to find the laser's endpoint
					{
						Vector2 posCheck = startPoint + Vector2.UnitX.RotatedBy(npc.rotation) * k * 8;

						if (!parent.arena.Contains(posCheck.ToPoint()))
						{
							endPoint = posCheck;
							break;
						}
					}
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			if (held)
			{
				var tex = GetTexture(Texture);
				spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, drawColor, npc.rotation, tex.Size() / 2, npc.scale, Math.Abs(npc.rotation) < 1.6f ? SpriteEffects.None : SpriteEffects.FlipVertically, 0);

				if(ShieldPower < 240)
				{
					var barTex = GetTexture(AssetDirectory.GUI + "ShieldBar1");
					var barUnderTex = GetTexture(AssetDirectory.GUI + "ShieldBar0");

					var source = new Rectangle(0, 0, (int)(ShieldPower / 240f * barTex.Width), barTex.Height);
					var target = new Rectangle((int)(owner.Center.X - barTex.Width / 2 - Main.screenPosition.X), (int)(owner.Center.Y - 64 - Main.screenPosition.Y), (int)(ShieldPower / 240f * barTex.Width), barTex.Height);

					var color = Color.Lerp(Color.Blue, Color.Cyan, ShieldPower / 240f);

					spriteBatch.Draw(barUnderTex, target.TopLeft() + Vector2.UnitY * 2, color);
					spriteBatch.Draw(barTex, target, source, color);
				}
			}

			else
			{
				var tex = GetTexture(AssetDirectory.GlassBoss + "PlayerShield");
				spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, drawColor, 0, tex.Size() / 2, 1, 0, 0);
			}

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (startPoint == Vector2.Zero)
				return;

			int sin = (int)(Math.Sin(StarlightWorld.rottime * 3) * 40f);
			var color = new Color(255, 160 + sin, 40 + sin / 2);

			var rotation = (endPoint - startPoint).ToRotation();

			var texBeam = GetTexture(AssetDirectory.MiscTextures + "BeamCore");
			var texBeam2 = GetTexture(AssetDirectory.MiscTextures + "BeamTrail");

			Vector2 origin = new Vector2(0, texBeam.Height / 2);
			Vector2 origin2 = new Vector2(0, texBeam2.Height / 2);

			var effect = StarlightRiver.Instance.GetEffect("Effects/GlowingDust");

			effect.Parameters["uColor"].SetValue(color.ToVector3());

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

			float height = texBeam.Height / 3f;
			int width = (int)(npc.Center - endPoint).Length();

			var pos = startPoint - Main.screenPosition;

			var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 1.2f));
			var target2 = new Rectangle((int)pos.X, (int)pos.Y, width, (int)height);

			var source = new Rectangle((int)((Timer / 20f) * -texBeam.Width), 0, texBeam.Width, texBeam.Height);
			var source2 = new Rectangle((int)((Timer / 45f) * -texBeam2.Width), 0, texBeam2.Width, texBeam2.Height);

			spriteBatch.Draw(texBeam, target, source, color, rotation, origin, 0, 0);
			spriteBatch.Draw(texBeam2, target2, source2, color * 0.5f, rotation, origin2, 0, 0);

			for (int i = 0; i < width; i += 10)
			{
				Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(rotation) * i + Main.screenPosition, color.ToVector3() * height * 0.010f);

				if (Main.rand.Next(20) == 0)
					Dust.NewDustPerfect(startPoint + Vector2.UnitX.RotatedBy(rotation) * i, DustType<Dusts.Glow>(), Vector2.UnitY * Main.rand.NextFloat(-1.5f, -0.5f), 0, color, 0.4f);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			var impactTex = GetTexture(AssetDirectory.Assets + "Keys/GlowSoft");
			var impactTex2 = GetTexture(AssetDirectory.GUI + "ItemGlow");
			var glowTex = GetTexture(AssetDirectory.Assets + "GlowTrail");

			spriteBatch.Draw(glowTex, target, source, color * 0.95f, rotation, new Vector2(0, glowTex.Height / 2), 0, 0);

			spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color * (height * 0.006f), 0, impactTex.Size() / 2, 6.4f, 0, 0);
			spriteBatch.Draw(impactTex2, endPoint - Main.screenPosition, null, color * (height * 0.01f), StarlightWorld.rottime * 2, impactTex2.Size() / 2, 0.75f, 0, 0);

			for (int k = 0; k < 4; k++)
			{
				float rot = Main.rand.NextFloat(6.28f);
				int variation = Main.rand.Next(30);

				color.G -= (byte)variation;

				Dust.NewDustPerfect(npc.Center + Vector2.UnitX.RotatedBy(rotation) * width + Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(40), DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot) * 2, 0, color, 0.9f - (variation * 0.03f));
			}
		}

		protected override void PutDownNatural(Player player)
		{
			StaleTimer = 0;
		}
	}
}
