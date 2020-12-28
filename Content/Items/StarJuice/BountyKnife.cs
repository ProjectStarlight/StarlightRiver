using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using Terraria.ModLoader;

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
            item.width = 16;
            item.height = 16;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useAnimation = 10;
            item.useTime = 10;
            item.rare = ItemRarityID.Orange;
            item.shoot = ProjectileType<BountyKnifeProjectile>();
            item.shootSpeed = 2;
            item.damage = 1;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
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
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 1000;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.extraUpdates = 5;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bounty Knife");
        }

        public override void AI()
        {
            Dust.NewDustPerfect(projectile.Center, DustType<Content.Dusts.Starlight>(), Vector2.Zero);
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