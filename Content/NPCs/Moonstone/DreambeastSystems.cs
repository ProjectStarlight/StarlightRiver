using NetEasy;
using ReLogic.Utilities;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.MetaballSystem;
using System;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Moonstone
{
	internal class DreamBeastDrawSystem : ModSystem
	{
		public override void Load()
		{
			On_Main.DrawNPCs += DrawDreamBeast;
		}

		private void DrawDreamBeast(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
		{
			orig(self, behindTiles);

			if (!behindTiles)
				DreamBeastActor.DrawSpecial();
		}
	}

	internal class DreamBeastActor : MetaballActor
	{
		public NPC activeBeast;

		public override bool Active => NPC.AnyNPCs(ModContent.NPCType<Dreambeast>());

		public override Color OutlineColor => new Color(220, 200, 255) * (activeBeast?.Opacity ?? 0) * 0.5f;

		public override void DrawShapes(SpriteBatch spriteBatch)
		{
			for (int k = 0; k < Main.maxNPCs; k++)
			{
				NPC npc = Main.npc[k];

				if (npc.ModNPC is Dreambeast && npc.active)
					(npc.ModNPC as Dreambeast).DrawToMetaballs(spriteBatch);
			}
		}

		public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			activeBeast = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<Dreambeast>()); //TODO: proper find for onscreen beast

			return false;
		}

		public static void DrawSpecial()
		{
			MetaballSystem.actorsSem.WaitOne();

			NPC activeBeast = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<Dreambeast>());

			if (activeBeast is null)
			{
				MetaballSystem.actorsSem.Release();
				return;
			}

			Texture2D target = MetaballSystem.actors.FirstOrDefault(n => n is DreamBeastActor).Target.RenderTarget;

			float lunacy = Main.LocalPlayer.GetModPlayer<LunacyPlayer>().lunacy;
			if (lunacy >= 0 && lunacy < 100)
			{
				Effect effect = Filters.Scene["MoonstoneBeastEffect"].GetShader().Shader;
				effect.Parameters["baseTexture"].SetValue(target);
				effect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise2").Value);
				effect.Parameters["size"].SetValue(target.Size());
				effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.005f);
				effect.Parameters["opacity"].SetValue(lunacy / 100f);
				effect.Parameters["noiseSampleSize"].SetValue(new Vector2(800, 800));
				effect.Parameters["noisePower"].SetValue(100f);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);
			}

			Main.spriteBatch.Draw(target, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2, 0, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			MetaballSystem.actorsSem.Release();
		}
	}

	internal class DreamBeastSpawner : PlayerTicker
	{
		public override int TickFrequency => 30;

		public override bool Active(Player Player)
		{
			return Player.InModBiome(ModContent.GetInstance<MoonstoneBiome>());
		}

		public override void Tick(Player Player)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return; //only spawn for singlePlayer or on the server

			if (Player.GetModPlayer<LunacyPlayer>().lunacy > 0 && Main.npc.Count(n => n.active && n.type == ModContent.NPCType<Dreambeast>() && n.position.Distance(Player.position) < 3000) == 0)
			{
				Vector2 pos = Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(300, 500);
				NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)pos.X, (int)pos.Y, ModContent.NPCType<Dreambeast>());
			}
		}
	}

	public partial class LunacyPlayer : ModPlayer, ILoadable
	{
		public float lunacy = 0;
		public int sanityTimer = 0;

		private int fullyInsaneTimer = 0;
		private bool awarded = false;
		private SlotId? insaneChargeSound;

		public bool Insane => lunacy > 100;

		public override void Load()
		{
			if (Main.dedServ)
				return;

			On_Main.GUIBarsDraw += DrawLunacyMeter;
		}

		public override void PostUpdateBuffs()
		{

			if (sanityTimer > 0)
			{
				sanityTimer--;
				lunacy = MathHelper.Lerp(lunacy, 99, 0.1f);
			}

			if (Player.HasBuff<Dreamwarp>())
			{
				lunacy += 0.2f;
			}
			else if (Player.InModBiome<MoonstoneBiome>())
			{
				if (lunacy < 99)
					lunacy += 0.025f;
				else
					lunacy = MathHelper.Lerp(lunacy, 99, 0.01f);
			}
			else
			{
				lunacy = Math.Max(MathHelper.Lerp(lunacy, 0, 0.05f) - 0.25f, 0);
			}

			if (fullyInsaneTimer == 1)
				insaneChargeSound = Helpers.Helper.PlayPitched("Magic/MysticCast", 1, -0.2f);
			else if (fullyInsaneTimer == 90)
				Helpers.Helper.PlayPitched("Magic/HolyCastShort", 1, 0.2f);

			if (lunacy < 100)
				awarded = false;

			if (lunacy > 1000)
			{
				fullyInsaneTimer++;
			}
			else
			{
				fullyInsaneTimer = 0;

				if (insaneChargeSound != null)
				{	
					SoundEngine.TryGetActiveSound((SlotId)insaneChargeSound, out ActiveSound sound);

					if (sound != null)
						sound.Volume = 0;

					insaneChargeSound = null;
				}
			}

			if (lunacy > 1000 && fullyInsaneTimer < 90)
			{
				Vector2 offset = -Vector2.UnitY * (50 + Player.gfxOffY);

				if (fullyInsaneTimer % 12 == 0)
				{
					Dust.NewDustDirect(Player.Center + offset, 0, 0, ModContent.DustType<Dusts.MoonstoneShimmer>(), 0, 0, 35, new Color(150, 120, 255, 0) * 0.5f, Main.rand.NextFloat(0.4f, 0.5f)).velocity *= 0.2f;
				}

				if (fullyInsaneTimer % 5 == 0)
				{
					Vector2 pos = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
					Dust.NewDustDirect(Player.Center + offset + pos * 50, 0, 0, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0, 0, 35, new Color(150, 120, 255) * 0.5f, Main.rand.NextFloat(0.4f, 0.8f)).velocity = -pos * 4;

					pos = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
					Dust.NewDustDirect(Player.Center + offset + pos * 50, 0, 0, DustID.Shadowflame, 0, 0, 35, new Color(150, 120, 255) * 0.5f, Main.rand.NextFloat(0.6f, 1f)).velocity = -pos * 3;
				}
			}
			else if (lunacy > 1000 && fullyInsaneTimer == 90 && !awarded)
			{
				//Player.QuickSpawnItem(Player.GetSource_GiftOrReward("Going insane"), ModContent.ItemType<InsomniacsGaze>());
				awarded = true;
			}
		}

		/// <summary>
		/// Snaps you back to reality and exponentially lowers lunacy
		/// </summary>
		/// <param name="intensity"></param>
		public void ReturnSanity(int intensity)
		{
			sanityTimer = intensity;
		}

		/// <summary>
		/// Get damage multipler dealt to player by hallucinations
		/// </summary>
		/// <returns></returns>
		public float GetInsanityDamageMult()
		{
			if (lunacy < 100)
				return lunacy / 100;
			else if (lunacy < 500)
				return 1 + (lunacy - 100) / 200;
			else
				return 3 + (lunacy - 500) / 500;
		}

		private static void DrawLunacyMeter(On_Main.orig_GUIBarsDraw orig, Main self)
		{
			Main.LocalPlayer.GetModPlayer<LunacyPlayer>().DrawLunacyMeter();
			orig(self);
		}

		private void DrawLunacyMeter()
		{
			if (Player.dead)
				return;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneNPC + "LunaticEye").Value;

			Vector2 offset = -Vector2.UnitY * (50 + Player.gfxOffY);
			Rectangle drawRect = new(0, 0, tex.Width, tex.Height / 5);

			float opacity = 1;
			float pulse = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * Math.Clamp((lunacy - 300) / 450, 0, 1) * 0.3f;

			float insanePulse = (float)Math.Pow(Math.Max(0, fullyInsaneTimer - 90), 2);
			float insanePulseOpacity = Math.Max(0, 1 - (fullyInsaneTimer - 90) / 30f);

			if (lunacy < 100)
			{
				opacity = lunacy / 100;
				drawRect.Y = 0;
			}
			else if (lunacy < 300)
			{
				drawRect.Y = drawRect.Height;
			}
			else if (lunacy < 600)
			{
				drawRect.Y = drawRect.Height * 2;
			}

			else if (fullyInsaneTimer < 90)
			{
				drawRect.Y = drawRect.Height * 3;
			}
			else
			{
				drawRect.Y = drawRect.Height * 4;
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			if (fullyInsaneTimer < 0)
			{
				Main.spriteBatch.Draw(tex, Player.Center + offset - Main.screenPosition, drawRect, Color.White * opacity * pulse, 0, drawRect.Size() / 2, 1f + pulse, 0, 0);
			}
			else if (fullyInsaneTimer > 90)
			{
				Main.spriteBatch.Draw(tex, Player.Center + offset - Main.screenPosition, drawRect, Color.White * 0.2f * insanePulseOpacity, 0, drawRect.Size() / 2, 1f + insanePulse, 0, 0);
			}

			Main.spriteBatch.Draw(tex, Player.Center + offset - Main.screenPosition, drawRect, Color.White * opacity, 0, drawRect.Size() / 2, 1f, 0, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (damageSource.SourceProjectileType == ModContent.ProjectileType<DreambeastProj>() || damageSource.SourceProjectileType == ModContent.ProjectileType<DreambeastProjHome>() || damageSource.SourceNPCIndex != -1 && Main.npc[damageSource.SourceNPCIndex].type == ModContent.NPCType<Dreambeast>())
				damageSource = PlayerDeathReason.ByCustomReason(Player.name + "'s mind was torn apart by their hallucinations");

			return true;
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			//for syncing on world joins
			var packet = new LunacyPacket(this);
			packet.Send(toWho, Player.whoAmI, false);
		}
	}

	[Serializable]
	public class LunacyPacket : Module
	{
		private readonly byte player;
		private readonly float lunacy;

		public LunacyPacket(LunacyPlayer lPlayer)
		{
			this.player = (byte)lPlayer.Player.whoAmI;
			this.lunacy = lPlayer.lunacy;
		}

		protected override void Receive()
		{
			Player player = Main.player[this.player];

			player.GetModPlayer<LunacyPlayer>().lunacy = lunacy;

			if (Main.netMode == NetmodeID.Server)
				Send(-1, this.player, false);
		}
	}

	/// <summary>
	/// Packet to be sent when a player is hit when lunatic so the other clients can know about it
	/// </summary>
	[Serializable]
	public class SanityHitPacket : Module
	{
		private readonly byte sanityReceiver;
		private readonly int sanityReturned;

		public SanityHitPacket(int sanityReceiver, int sanityReturned)
		{
			this.sanityReceiver = (byte)sanityReceiver;
			this.sanityReturned = sanityReturned;
		}
		protected override void Receive()
		{
			Player player = Main.player[sanityReceiver];

			player.GetModPlayer<LunacyPlayer>().ReturnSanity(sanityReturned);

			if (Main.netMode == NetmodeID.Server)
				Send(-1, sanityReceiver, false);
		}
	}
}
