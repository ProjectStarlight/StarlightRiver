using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.StarJuice
{
	internal class BountyKnife : StarjuiceStoringItem
    {
        public BountyKnife() : base(2500) { }

        public override string Texture => "StarlightRiver/Assets/Items/Starjuice/BountyKnife";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hunters Dagger");
            Tooltip.SetDefault("Infuse a beast with starlight\nInfused enemies become powerful and gain abilities\nSlain enemies drop crystals");
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.rare = ItemRarityID.Orange;
            Item.shoot = ProjectileType<BountyKnifeProjectile>();
            Item.shootSpeed = 2;
            Item.damage = 1;
        }

        public override bool Shoot(Player Player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (charge == maxCharge)
            {
                charge = 0;
                return true;
            }
            return false;
        }
    }

    internal class BountyKnifeProjectile : ModProjectile     
    {
        public override string Texture => "StarlightRiver/Assets/Items/Starjuice/BountyKnifeProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 1000;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 5;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bounty Knife");
        }

        public override void AI()
        {
            Dust.NewDustPerfect(Projectile.Center, DustType<Content.Dusts.Starlight>(), Vector2.Zero);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!target.boss && !target.dontTakeDamage && !target.immortal && !target.friendly)
            {
                /*if (target.aiStyle == 1) NPC.NewNPC((int)target.position.X, (int)target.position.Y, ModContent.NPCType<NPCs.Beasts.SlimeBeast>());
                else if (!target.noGravity && !target.noTileCollide) NPC.NewNPC((int)target.position.X, (int)target.position.Y, ModContent.NPCType<NPCs.Beasts.GroundBeast>());
                else if (target.noGravity && !target.noTileCollide) NPC.NewNPC((int)target.position.X, (int)target.position.Y, ModContent.NPCType<NPCs.Beasts.AirBeast>());
                else if (target.noGravity && target.noTileCollide) NPC.NewNPC((int)target.position.X, (int)target.position.Y, ModContent.NPCType<NPCs.Beasts.PhaseBeast>());
                else NPC.NewNPC((int)target.position.X, (int)target.position.Y, ModContent.NPCType<NPCs.Beasts.FailsafeBeast>()); //probably stupid rare*/
            }
        }
    }
}