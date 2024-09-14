using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.NPCs.BaseTypes;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.CustomHooks
{
	class MovingPlatforms : HookGroup
	{
		//Orig is called when appropriate, but this is still messing with vanilla behavior. Also IL.
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
							self.position += target.velocity;
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

			foreach (NPC NPC in Main.npc)
			{
				if (!NPC.active || NPC.ModNPC == null || NPC.ModNPC is not MovingPlatform || (NPC.ModNPC as MovingPlatform).dontCollide)
					continue;

				var PlayerRect = new Rectangle((int)self.position.X, (int)self.position.Y + self.height, self.width, 1);
				var NPCRect = new Rectangle((int)NPC.position.X, (int)NPC.position.Y, NPC.width, 8 + (self.velocity.Y > 0 ? (int)self.velocity.Y : 0));

				if (PlayerRect.Intersects(NPCRect) && self.position.Y <= NPC.position.Y)
				{
					if (!self.justJumped && self.velocity.Y >= 0)
					{
						if (fallThrough)
							self.GetModPlayer<StarlightPlayer>().platformTimer = 10;

						self.gfxOffY = NPC.gfxOffY;
						self.velocity.Y = 0;
						self.jump = 0;
						self.fallStart = (int)(self.position.Y / 16f);
						self.position.Y = NPC.position.Y - self.height + 4;
						self.position += NPC.velocity;

						(NPC.ModNPC as MovingPlatform).beingStoodOn = true;
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