//TODO:
//Value
//Bestiary
//Spawning
//Buff targets
//Make it able to be stood on and immortal
//Visuals
//Better chain texture
//Make it bob slightly
//Visuals for chain attachments and breaking

using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Misc
{
	class Fogbinder : ModNPC
	{
		private int frameCounter = 0;
		private int yFrame = 0;

		public List<NPC> targets = new();

		public Player player => Main.player[NPC.target];

		public override string Texture => AssetDirectory.MiscNPC + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Fogbinder");
			Main.npcFrameCount[NPC.type] = 4;
		}

		public override void SetDefaults()
		{
			NPC.width = 56;
			NPC.height = 114;
			NPC.knockBackResist = 0.1f;
			NPC.lifeMax = 100;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 1;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false; //harmless
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			NPC.spriteDirection = NPC.direction;
			frameCounter++;
			if (frameCounter % 4 == 0)
				yFrame++;
			yFrame %= Main.npcFrameCount[NPC.type];

			NPC.velocity = Vector2.Zero;

			targets = Main.npc.Where(n => n.active && n.knockBackResist > 0 && n.Distance(NPC.Center) < 500 && n.type != NPC.type).ToList();
			targets.ForEach(n => n.GetGlobalNPC<FogbinderGNPC>().fogbinder = NPC);
		}

		public override void FindFrame(int frameHeight)
		{
			int frameWidth = NPC.width;
			NPC.frame = new Rectangle(0, frameHeight * yFrame, frameWidth, frameHeight);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = Request<Texture2D>(Texture).Value;
			Texture2D glowTexture = Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D chainTex = Request<Texture2D>(Texture + "_Chain").Value;

			SpriteEffects effects = SpriteEffects.None;
			var origin = new Vector2(NPC.width / 2, NPC.height / 2);

			if (NPC.spriteDirection != 1)
				effects = SpriteEffects.FlipHorizontally;

			var slopeOffset = new Vector2(0, NPC.gfxOffY);

			foreach (NPC target in targets)
			{
				float distanceToTarget = (target.Center - NPC.Center).Length();

				Vector2 directionToTarget = Vector2.Normalize(target.Center - NPC.Center);

				for (float i = 0; i < distanceToTarget; i+= chainTex.Width)
				{
					Vector2 pos = Vector2.Lerp(NPC.Center, target.Center, i / distanceToTarget);
					Main.spriteBatch.Draw(chainTex, pos - screenPos, null, drawColor, directionToTarget.ToRotation(), chainTex.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
				}
			}

			Main.spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
			Main.spriteBatch.Draw(glowTexture, slopeOffset + NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, effects, 0f);

			return false;
		}
	}

	public class FogbinderGNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public NPC fogbinder;

		public override void AI(NPC npc)
		{
			if (fogbinder == default || fogbinder is null)
				return;

			if (fogbinder.active)
			{
				npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(fogbinder.Center), npc.Distance(fogbinder.Center) / 2000f);

				if (npc.Distance(fogbinder.Center) > 500)
					fogbinder = default;
			}
		}
	}
}
