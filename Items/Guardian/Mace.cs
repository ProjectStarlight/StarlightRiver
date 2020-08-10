using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Guardian
{
    internal class Mace : GuardianWeapon
    {
        private int Timer { get; set; }

        public Mace(int HPcost, int lifesteal, int healrad, int heal) : base(HPcost, lifesteal, healrad, heal)
        {
        }

        public override void HoldItem(Player player)
        {
            Timer++; //the timing ticker
            if (Timer > item.useTime * 2) Timer = -(item.useTime * 2);
            //Main.NewText(Timer);
        }

        public virtual void SpawnProjectile(int type, Player player) //virtual to allow custom timing gimicks on individual maces
        {
            int damage = (item.damage / 2) + (Math.Abs(Timer) <= item.useTime * 0.5f ? item.damage / 2 : item.damage / 4); //calculate damage based on timing
            int i = Projectile.NewProjectile(player.Center + Vector2.Normalize(Main.MouseWorld - player.Center) * 48, Vector2.Zero, type, damage, item.knockBack, player.whoAmI); //spawns the mace swing
            if (Main.projectile[i].modProjectile != null)
            {
                (Main.projectile[i].modProjectile as MaceProjectile).LifeSteal = (int)(LifeSteal * (damage / (float)item.damage)); //sets the healing of the projectile to the appropriate fraction based on timing
                (Main.projectile[i].modProjectile as MaceProjectile).HealRadius = HealRadius;
                (Main.projectile[i].modProjectile as MaceProjectile).Heal = (int)(HealAmount * (damage / (float)item.damage));
            }
        }
    }

    internal class MaceProjectile : ModProjectile
    {
        public int LifeSteal { get; set; }
        public int HealRadius { get; set; }
        public int Heal { get; set; }

        public bool HealedTeam = false;

        public virtual void SafeSetDefaults()
        {
        }

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();
            projectile.aiStyle = 0;
            projectile.friendly = true;
            projectile.penetrate = -1;
        }

        public virtual void SafeAI()
        {
        }

        public sealed override void AI()
        {
            projectile.position += Main.player[projectile.owner].velocity;
        }

        public virtual void SafeOnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
        }

        public sealed override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Player player = Main.player[projectile.owner];

            player.statLife += LifeSteal;
            player.HealEffect(LifeSteal);
            if (!HealedTeam)
            {
                foreach (Player ally in Main.player.Where(ally => Vector2.Distance(player.Center, ally.Center) <= HealRadius && ally != player))
                {
                    ally.statLife += Heal;
                    ally.HealEffect(Heal);
                }
                for (int k = 0; k <= 100; k++)
                {
                    Dust.NewDustPerfect(player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(HealRadius), DustType<Dusts.Starlight>(), Vector2.Zero, 0, default, Main.rand.NextFloat(0.6f));
                }
                for (int k = 0; k <= HealRadius * 6.28f; k += 2)
                {
                    Dust.NewDustPerfect(player.Center + Vector2.One.RotatedBy(k / (HealRadius * 6.28f) * 6.28f) * HealRadius, DustType<Dusts.Starlight>(), Vector2.Zero, 0, default, 0.8f);
                }
                Main.PlaySound(SoundID.NPCHit5.WithPitchVariance(0.6f), player.Center);
                HealedTeam = true;
            }
            SafeOnHitNPC(target, damage, knockback, crit);
        }
    }
}