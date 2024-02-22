using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Permafrost
{
	internal class WaterCube : ModNPC
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			NPC.width = 100;
			NPC.height = 100;
			NPC.lifeMax = 10;
			NPC.damage = 1;
			NPC.dontTakeDamage = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
		}

		public override void AI()
		{
			NPC.velocity.X = 1;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			if (target.Hitbox.Intersects(NPC.Hitbox))
				target.AddBuff(BuffType<Buffs.PrismaticDrown>(), 2);

			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			return false;
		}

		public void DrawToTarget(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value;
			Vector2 pos = (NPC.position - Main.screenPosition) / 2f;
			var target = new Rectangle((int)pos.X, (int)pos.Y, NPC.width / 2, NPC.height / 2);

			spriteBatch.Draw(tex, target, Color.Red);
		}
	}
}