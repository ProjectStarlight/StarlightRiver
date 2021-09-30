using StarlightRiver.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.Items.Breacher
{
	[AutoloadEquip(EquipType.Head)]
    public class BreacherHead : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + "BreacherHead";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breacher Visor");
            Tooltip.SetDefault("Add stats later");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.value = 8000;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class BreacherChest : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + "BreacherChest";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breacher Chestplate");
            Tooltip.SetDefault("Add stats later");
        }

        public override void SetDefaults()
        {
            item.width = 34;
            item.height = 20;
            item.value = 6000;
        }


        public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == ModContent.ItemType<BreacherHead>() && legs.type == ModContent.ItemType<BreacherLegs>();

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "A breacher drone follows you \nDouble tap down to call an airstrike on a nearby enemy";
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SpotterDrone>()] < 1)
            {
                Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<SpotterDrone>(), (int)(30 * player.rangedDamage), 1.5f, player.whoAmI);
            }
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class BreacherLegs : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + "BreacherLegs";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breacher Leggings");
            Tooltip.SetDefault("Add stats later");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 20;
            item.value = 4000;
        }
    }

    public class SpotterDrone : ModProjectile
    {
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public int ScanTimer = 0;

        public bool CanScan = true;

        float timer = 0;

        const float attackRange = 800;

        private NPC target;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Granitech Drone");
            Main.projPet[projectile.type] = true;
            Main.projFrames[projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.netImportant = true;
            projectile.width = 20;
            projectile.height = 20;
            projectile.friendly = false;
            projectile.minion = true;
            projectile.minionSlots = 0;
            projectile.penetrate = -1;
            projectile.timeLeft = 216000;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            timer += 0.1f;
            if (ScanTimer <= 0)
                IdleMovement(player);
            else
                AttackMovement(player);
        }

        private Vector2 GetNoise() => new Vector2((float)Math.Cos(timer * 0.78f), (float)Math.Sin(timer * 1.06f)) * 0.1f;

        private void IdleMovement(Entity entity)
        {
            var noise = GetNoise();

            Vector2 targetPos = GetAnchorPoint(entity, new Vector2(-40, -28));
            Vector2 dir = targetPos - projectile.Center;

            float length = dir.Length();

            dir.Normalize();

            projectile.velocity *= 0.97f;
            projectile.velocity += dir * length * 0.01f;
            projectile.velocity += noise * Math.Max(0, (80 - length)) / 80;
            projectile.rotation = projectile.velocity.ToRotation();
        }

        private Vector2 GetAnchorPoint(Entity entity, Vector2 offset)
        {
            Vector2 v = entity.Center + new Vector2(0, offset.Y);
            v += new Vector2(offset.X, 0) * entity.direction;
            return v;
        }

        private void AttackMovement(Player player)
        {
            if (ScanTimer == 200)
            {
                NPC testtarget = Main.npc.Where(n => n.CanBeChasedBy(projectile, false) && Vector2.Distance(n.Center, projectile.Center) < attackRange).OrderBy(n => Vector2.Distance(n.Center, projectile.Center)).FirstOrDefault();

                if (testtarget == default)
                    IdleMovement(player);
                else
                {
                    target = testtarget;
                    ScanTimer--;
                }
            }
            else
            {
                if (!target.active || target == null)
                {
                    ScanTimer = 200;
                    return;
                }
                ScanTimer--;
                IdleMovement(target);

            }
        }
    }
}