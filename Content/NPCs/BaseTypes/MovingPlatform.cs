using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.NPCs.BaseTypes
{
	internal abstract class MovingPlatform : ModNPC
	{
		public Vector2 prevPos;

		public bool beingStoodOn;

		public bool dontCollide = false;

		public virtual bool CanFallThrough => true;

		public virtual void SafeSetDefaults() { }

		public virtual void SafeAI() { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("");
		}

		public sealed override void SetDefaults()
		{
			SafeSetDefaults();

			NPC.lifeMax = 10;
			NPC.immortal = true;
			NPC.dontTakeDamage = true;
			NPC.noGravity = true;
			NPC.knockBackResist = 0; //very very important!!
			NPC.aiStyle = -1;
			NPC.damage = 0;
			NPC.netAlways = true;

			for (int k = 0; k < NPC.buffImmune.Length; k++)
			{
				NPC.buffImmune[k] = true;
			}
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			database.Entries.Remove(bestiaryEntry);
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override bool? CanBeHitByProjectile(Projectile Projectile)
		{
			return false;
		}

		public override bool? CanBeHitByItem(Player Player, Item Item)
		{
			return false;
		}

		public sealed override void AI()
		{
			SafeAI();

			//TODO: More elegant sync guarantee later perhaps, this should ensure platforms always eventually exist in MP
			if (Main.netMode == NetmodeID.Server && Main.GameUpdateCount % 60 == 0)
				NPC.netUpdate = true;

			if (!dontCollide)
			{
				float yDistTraveled = NPC.position.Y - NPC.oldPosition.Y;

				if (NPC.velocity != Vector2.Zero && NPC.velocity.Y < -1f && yDistTraveled < NPC.velocity.Y * 1.5 && yDistTraveled > NPC.velocity.Y * 6)
				{
					//this loop outside of the normal moving platform loop in Mechanics is mainly for multiPlayer with some potential for extreme lag situations on fast platforms
					//what is happening is that when terraria skips frames (or lags in mp) they add the NPC velocity multiplied by the skipped frames up to 5x a normal frame until caught up, but only run the ai once
					//so we can end up with frames where the platform skips 5x its normal velocity likely clipping through Players since the platform is thin.
					//to solve this, the collision code takes into account the previous platform position accessed by this AI for the hitbox to cover the whole travel from previous fully processed frame.
					//only handling big upwards y movements since the horizontal skips don't seem as jarring to the user since platforms tend to be wide, and vertical down skips aren't jarring since Player drops onto platform anyway instead of clipping through.
					for (int k = 0; k < Main.maxPlayers; k++)
					{
						Player player = Main.player[k];

						if (!player.active || player.dead || player.GoingDownWithGrapple || CanFallThrough && player.GetModPlayer<StarlightPlayer>().platformTimer > 0)
							continue;

						if (player.position.Y <= NPC.position.Y && player.velocity.Y >= 0 && !player.justJumped)
						{
							var PlayerRect = new Rectangle((int)player.position.X, (int)player.position.Y + player.height, player.width, 1);
							var NPCRect = new Rectangle((int)NPC.position.X, (int)NPC.position.Y, NPC.width, 8 + (player.velocity.Y > 0 ? (int)player.velocity.Y : 0) + (int)Math.Abs(yDistTraveled));

							if (PlayerRect.Intersects(NPCRect))
							{
								player.velocity.Y = 0;
								player.position.Y = NPC.position.Y - player.height + 4;
								player.position += NPC.velocity;
							}
						}
					}
				}
			}

			prevPos = NPC.oldPosition;
			beingStoodOn = false;
		}
	}

	internal class MovingPlatformSystem : ModSystem
	{
		public override void Load()
		{
			On_Player.SlopingCollision += PlatformCollision;

			IL_Projectile.AI_007_GrapplingHooks += GrapplePlatforms;
		}

		public override void Unload()
		{
			IL_Projectile.AI_007_GrapplingHooks -= GrapplePlatforms;
		}

		private void PlatformCollision(On_Player.orig_SlopingCollision orig, Player self, bool fallThrough, bool ignorePlats)
		{
			if (self.grapCount == 1)
			{
				//if the Player is using a single grappling hook we can check if they are colliding with it and its embedded in the moving platform, while its changing Y position so we can give the Player their jump back
				foreach (int eachGrappleIndex in self.grappling)
				{
					if (eachGrappleIndex < 0 || eachGrappleIndex > Main.maxProjectiles)//somehow this can be invalid at this point?
						continue;

					Projectile grappleHookProj = Main.projectile[eachGrappleIndex];
					if (grappleHookProj.TryGetGlobalProjectile(out GrapplingHookGlobal globalGrappleProj))
					{
						NPC target = globalGrappleProj.grappledTo;

						if (target is not null && target.active && grappleHookProj.active && self.Hitbox.Intersects(grappleHookProj.Hitbox))
						{
							self.position = grappleHookProj.position + new Vector2(grappleHookProj.width / 2 - self.width / 2, grappleHookProj.height / 2 - self.height / 2);
							self.position.Y += target.velocity.Y;
							self.velocity.X += target.velocity.X;
							self.velocity.Y = 0;
							self.jump = 0;
							self.fallStart = (int)(self.position.Y / 16f);
						}
					}
				}
			}

			if (self.GoingDownWithGrapple)
			{
				orig(self, fallThrough, ignorePlats);
				return;
			}

			if (self.GetModPlayer<StarlightPlayer>().platformTimer > 0)
			{
				orig(self, fallThrough, ignorePlats);
				return;
			}

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.ModNPC == null || npc.ModNPC is not MovingPlatform || (npc.ModNPC as MovingPlatform).dontCollide)
					continue;

				var plat = npc.ModNPC as MovingPlatform;

				var PlayerRect = new Rectangle((int)self.position.X, (int)self.position.Y + self.height, self.width, 1);
				var NPCRect = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, 8 + (self.velocity.Y > 0 ? (int)self.velocity.Y : 0));
				var NPCOldRect = new Rectangle((int)plat.prevPos.X, (int)plat.prevPos.Y, npc.width, 8 + (self.velocity.Y > 0 ? (int)self.velocity.Y : 0));

				if ((PlayerRect.Intersects(NPCRect) || PlayerRect.Intersects(NPCOldRect)) && self.position.Y <= npc.position.Y)
				{
					if (!self.justJumped && self.velocity.Y >= 0)
					{
						if ((npc.ModNPC as MovingPlatform).CanFallThrough && self.GetModPlayer<StarlightPlayer>().platformTimer > 0)
							continue;

						if (fallThrough)
							self.GetModPlayer<StarlightPlayer>().platformTimer = 10;

						self.gfxOffY = npc.gfxOffY;
						self.velocity.Y = 0;
						self.jump = 0;
						self.fallStart = (int)(self.position.Y / 16f);
						self.position.Y = npc.position.Y - self.height + 4;
						self.position += npc.velocity;

						(npc.ModNPC as MovingPlatform).beingStoodOn = true;
					}
				}
			}

			GravityPlayer mp = self.GetModPlayer<GravityPlayer>();

			if (mp.controller != null && mp.controller.NPC.active)
				self.velocity.Y = 0;

			orig(self, fallThrough, ignorePlats);
		}

		private void GrapplePlatforms(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(i => i.MatchLdfld<Projectile>("aiStyle"), i => i.MatchLdcI4(7));
			c.TryGotoNext(i => i.MatchLdfld<Projectile>("ai"), i => i.MatchLdcI4(0), i => i.MatchLdelemR4(), i => i.MatchLdcR4(0));
			c.TryGotoNext(i => i.MatchLdloc(44)); //flag in source code
			c.Index++;
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<GrapplePlatformDelegate>(EmitGrapplePlatformDelegate);
			c.TryGotoNext(i => i.MatchStfld<Player>("grapCount"));
			c.Index++;
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<UngrapplePlatformDelegate>(EmitUngrapplePlatformDelegate);
		}

		private delegate bool GrapplePlatformDelegate(bool fail, Projectile proj);
		private bool EmitGrapplePlatformDelegate(bool fail, Projectile proj)
		{
			if (proj.timeLeft < 36000 - 3 && proj.TryGetGlobalProjectile(out GrapplingHookGlobal global))
			{
				NPC n = global.grappledTo;
				if (n != null && n.active && n.ModNPC is MovingPlatform && !(n.ModNPC as MovingPlatform).dontCollide && n.Hitbox.Intersects(proj.Hitbox))
				{
					proj.position += n.velocity;

					if (!proj.tileCollide) //this is kinda hacky but... oh well 
						Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, proj.Center);

					proj.tileCollide = true;

					return false;
				}
			}

			return fail;
		}

		private delegate void UngrapplePlatformDelegate(Projectile proj);
		private void EmitUngrapplePlatformDelegate(Projectile proj)
		{
			Player Player = Main.player[proj.owner];
			int numHooks = 3;
			//time to replicate retarded vanilla hardcoding, wheee
			if (proj.type == 165)
				numHooks = 8;
			if (proj.type == 256)
				numHooks = 2;
			if (proj.type == 372)
				numHooks = 2;
			if (proj.type == 652)
				numHooks = 1;
			if (proj.type >= 646 && proj.type <= 649)
				numHooks = 4;
			//end vanilla zoink

			ProjectileLoader.NumGrappleHooks(proj, Player, ref numHooks);
			if (Player.grapCount > numHooks)
				Main.projectile[Player.grappling.OrderBy(n => (Main.projectile[n].active ? 0 : 999999) + Main.projectile[n].timeLeft).ToArray()[0]].Kill();
		}
	}

	class GrapplingHookGlobal : GlobalProjectile
	{
		public NPC grappledTo;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.aiStyle == 7;
		}

		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
		{
			int grappleIndex = -1;
			if (grappledTo != null)
				grappleIndex = grappledTo.whoAmI;

			if (projectile.ai[0] == 2)
				binaryWriter.Write(grappleIndex);
		}

		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
		{
			if (projectile.ai[0] == 2)
			{
				int grappledIndex = binaryReader.ReadInt32();
				
				if (grappledIndex != -1)
					grappledTo = Main.npc[grappledIndex];
			}
		}

		public override void PostAI(Projectile projectile)
		{
			if (grappledTo is null && projectile.ai[0] == 0 && projectile.owner == Main.myPlayer && projectile.timeLeft < 36000 - 3)
			{
				for (int k = 0; k < Main.maxNPCs; k++)
				{
					NPC n = Main.npc[k];
					if (n.active && n.ModNPC is MovingPlatform && !(n.ModNPC as MovingPlatform).dontCollide && n.Hitbox.Intersects(projectile.Hitbox))
					{
						projectile.ai[0] = 2;
						grappledTo = n;

						if (projectile.type == ProjectileID.QueenSlimeHook)
							Main.player[projectile.owner].DoQueenSlimeHookTeleport(projectile.Center);

						projectile.netUpdate = true;
					}
				}
			}
		}
	}
}