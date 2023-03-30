using Terraria.ID;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	internal class TentacleHurtbox : ModNPC
	{
		public Tentacle tentacle;

		public SquidBoss Parent => tentacle.Parent;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			NPC.width = 80;
			NPC.height = 80;
			NPC.lifeMax = 500;
			NPC.damage = 0;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0f;
			NPC.HitSound = SoundID.NPCHit1;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			NPC.lifeMax = Main.masterMode ? (int)(1000 * bossLifeScale) : (int)(750 * bossLifeScale);
		}

		public override void AI()
		{
			NPC.lifeMax = tentacle.NPC.lifeMax;
			NPC.life = tentacle.NPC.life;
			NPC.Hitbox = tentacle.GetDamageHitbox();

			NPC.dontTakeDamage = tentacle.State != 0;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			return false;
		}

		public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
			damage = (int)(damage * 1.25f);
		}

		public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage = (int)(damage * 1.25f);
		}

		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
		{
			if (crit)
				damage *= 2; //"But what about the crit damage system?" -- That is calculated in ModifyHitByX, and is thus already accounted for before this.

			if (Parent.NPC.life > Parent.NPC.lifeMax - NPC.lifeMax * 4)
				Parent.NPC.life -= damage;

			else if (Parent.NPC.life - damage < Parent.NPC.lifeMax - NPC.lifeMax * 4)
				Parent.NPC.life = Parent.NPC.lifeMax - NPC.lifeMax * 4;

			NPC.life = NPC.lifeMax;
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
		{
			if (crit)
				damage *= 2;

			if (Parent.NPC.life > Parent.NPC.lifeMax - NPC.lifeMax * 4)
				Parent.NPC.life -= damage;

			else if (Parent.NPC.life - damage < Parent.NPC.lifeMax - NPC.lifeMax * 4)
				Parent.NPC.life = Parent.NPC.lifeMax - NPC.lifeMax * 4;

			NPC.life = NPC.lifeMax;
		}
	}
}