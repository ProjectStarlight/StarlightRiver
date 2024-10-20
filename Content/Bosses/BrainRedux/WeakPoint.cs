using System;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class WeakPoint : ModNPC
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{

		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 4000;
			NPC.damage = 2;
			NPC.width = 46;
			NPC.height = 46;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.aiStyle = -1;
			NPC.knockBackResist = 0f;
			NPC.defense = 3;

			NPC.HitSound = SoundID.NPCDeath12.WithPitchOffset(-0.25f);
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override bool PreKill()
		{
			return false;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var tex = Assets.Bosses.BrainRedux.Neurysm.Value;
			spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, Color.Red, 0, tex.Size() / 2f, 1, 0, 0);

			return false;
		}

		public override void AI()
		{
			NPC.realLife = DeadBrain.TheBrain?.thinker?.whoAmI ?? NPC.realLife;
		}
	}
}
