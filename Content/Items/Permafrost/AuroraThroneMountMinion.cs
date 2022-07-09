using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;


namespace StarlightRiver.Content.Items.Permafrost
{
	internal class AuroraThroneMountMinion : ModProjectile
	{
        public override string Texture => AssetDirectory.SquidBoss + "Auroraling";

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            var target = Helpers.Helper.FindNearestNPC(Projectile.Center, true);

            if (target != null)
                Projectile.velocity += Vector2.Normalize(Projectile.Center - target.Center) * -0.15f;

            if (Projectile.velocity.LengthSquared() > 30) 
                Projectile.velocity *= 0.95f;

            if (Projectile.ai[0] % 15 == 0) 
                Projectile.velocity.Y -= 0.5f;

            Projectile.rotation = Projectile.velocity.X * 0.25f;
        }

		public override void Kill(int timeLeft)
		{
            foreach (NPC npc in Main.npc.Where(n => n.active && n.CanBeChasedBy(this, false) && Vector2.Distance(n.Center, Projectile.Center) < 120))
            {
                npc.StrikeNPC(Projectile.damage, Projectile.knockBack, Projectile.Center.X > npc.Center.X ? 1 : -1, false);
                npc.AddBuff(BuffType<AuroraThroneMountMinionDebuff>(), 300);
            }

            for (int k = 0; k < 20; k++)
            {
                Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Smoke>(), Main.rand.NextVector2Circular(5, 5), 150, new Color(80, 50, 50) * 0.5f, 1);

                var sparkOff = Main.rand.NextVector2Circular(3, 3);
                Dust.NewDustPerfect(Projectile.Center + sparkOff * 5, DustType<Dusts.Cinder>(), sparkOff, 0, new Color(255, 20, 20), 1);
            }

            Helpers.Helper.PlayPitched("SquidBoss/LightSplash", 0.6f, 1f, Projectile.Center);
            Helpers.Helper.PlayPitched("JellyBounce", 1f, 1f, Projectile.Center);
        }

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
            target.AddBuff(BuffType<AuroraThroneMountMinionDebuff>(), 300);
        }

		public override bool PreDraw(ref Color drawColor)
        {
            var spriteBatch = Main.spriteBatch;
            var frame = new Rectangle(26 * ((int)(Projectile.ai[0] / 5) % 3), 0, 26, 30);

            Texture2D tex = Request<Texture2D>(AssetDirectory.SquidBoss + "AuroralingGlow").Value;
            Texture2D tex2 = Request<Texture2D>(AssetDirectory.SquidBoss + "AuroralingGlow2").Value;

            float sin = 1 + (float)Math.Sin(Projectile.ai[0] / 10f);
            float cos = 1 + (float)Math.Cos(Projectile.ai[0] / 10f);
            Color color = new Color(1.2f + sin * 0.1f, 0.7f + sin * -0.25f, 0.25f) * 0.7f;

            spriteBatch.Draw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, frame, drawColor * 1.2f, Projectile.rotation, Projectile.Size / 2, 1, 0, 0);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, color * 0.8f, Projectile.rotation, Projectile.Size / 2, 1, 0, 0);
            spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation, Projectile.Size / 2, 1, 0, 0);

            Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);
            return false;
        }
    }

	public class AuroraThroneMountMinionDebuff : SmartBuff
	{
        public override string Texture => AssetDirectory.Debug;

        public AuroraThroneMountMinionDebuff() : base("Inked", "You take increased damage", true, false) { }

		public override void Load()
		{
            StarlightNPC.ModifyHitByProjectileEvent += takeExtraSummonDamage;
            StarlightPlayer.ModifyHitByNPCEvent += takeExtraDamage;
            StarlightPlayer.ModifyHitByProjectileEvent += takeExtraDamageProjectile;       
		}

		private void takeExtraSummonDamage(NPC NPC, Projectile Projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
            if (Inflicted(NPC) && Projectile.CountsAsClass(DamageClass.Summon))
                damage = (int)(damage * 1.25f);
		}

        private void takeExtraDamage(Player player, NPC NPC, ref int damage, ref bool crit)
        {
            if (Inflicted(player))
                damage = (int)(damage * 1.25f);         
        }

        private void takeExtraDamageProjectile(Player player, Projectile proj, ref int damage, ref bool crit)
        {
            if (Inflicted(player))
                damage = (int)(damage * 1.25f);
        }

		public override void Update(NPC npc, ref int buffIndex)
		{
            Dust.NewDust(npc.position, npc.width, npc.height, DustType<Dusts.Cinder>(), 0, 0, 0, new Color(255, 50, 50), 0.5f);
        }

		public override void Update(Player player, ref int buffIndex)
		{
            Dust.NewDust(player.position, player.width, player.height, DustType<Dusts.Cinder>(), 0, 0, 0, new Color(255, 50, 50), 0.5f);
        }
	}
}
