using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using System;
using System.Collections.Generic;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Palestone
{
    public class PalestoneHammer : ModItem
    {
        public override string Texture => AssetDirectory.PalestoneItem + "PalestoneHammer";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palecrusher");
        }

        public override void SetDefaults()
        {
            item.channel = true;
            item.damage = 12;
            item.width = 24;
            item.height = 24;
            item.useTime = 320;
            item.useAnimation = 320;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.melee = true;
            item.noMelee = true;
            item.knockBack = 8;
            item.useTurn = false;
            item.value = Item.sellPrice(0, 1, 42, 0);
            item.rare = 0;
            item.autoReuse = false;
            item.shoot = mod.ProjectileType("PalecrusherProj");
            item.shootSpeed = 6f;
            item.noUseGraphic = true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);
    }
     class PalecrusherProj : ClubProj
     {
        public override string Texture => AssetDirectory.PalestoneItem + "PalecrusherProj";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pale Crusher");
            Main.projFrames[projectile.type] = 2;
        }

        int[] targets = new int[3];

        public override void Smash(Vector2 position)
        {
            Player player = Main.player[projectile.owner];
            for (int k = 0; k <= 100; k++)
            {
                Dust.NewDustPerfect(projectile.oldPosition + new Vector2(projectile.width / 2, projectile.height / 2), DustType<Content.Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * projectile.ai[0] / 10f);
            }
            for (int k = 0; k < 3; k++)
            {
                int range = 40;
                int target = -1;
                float lowestDist = float.MaxValue;
                for (int i = 0; i < 200; ++i)
                {
                    bool match = false;
                    NPC npc = Main.npc[i];
                    foreach (int j in targets)
                    {
                        if (j == i)
                        {
                            match = true;
                            break;
                        }
                    }
                    if (npc.active && npc.CanBeChasedBy(projectile) && !npc.friendly && !match && !npc.noGravity)
                    {
                        float dist = projectile.Distance(npc.Center);
                        if (dist / 16 < range)
                        {
                            if (dist < lowestDist)
                            {
                                lowestDist = dist;

                                target = npc.whoAmI;
                                projectile.netUpdate = true;
                            }
                        }
                    }
                }
                targets[k] = target;
                if (targets[k] != -1)
                {
                    NPC npc = Main.npc[targets[k]];
                    Projectile.NewProjectile(npc.Center.X, npc.Center.Y - 32, 0, 0, ModContent.ProjectileType<PalePillar>(), projectile.damage / 3, projectile.knockBack / 2, projectile.owner);
                }
            }
            for (int j = 0; j < 3; j++)
            {
                if (targets[j] != -1)
                {
                    NPC npc = Main.npc[targets[j]];
                    Projectile.NewProjectile(npc.Center.X, npc.Center.Y - 32, 0, 0, ModContent.ProjectileType<PalePillar>(), projectile.damage / 3, projectile.knockBack / 2, projectile.owner);
                }
            }
        }
        public PalecrusherProj() : base(52, 16, 40, -1, 48, 4, 8, 2.1f, 18f) { }
     }

    public class PalePillar : ModProjectile
    {
        public override string Texture => AssetDirectory.PalestoneItem + "PalePillar";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pale Pillar");
        }

        public override void SetDefaults()
        {
            projectile.hide = true;
            projectile.hostile = false;
            projectile.width = 30;
            projectile.height = 5;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.damage = 1;
            projectile.penetrate = -1;
            projectile.alpha = 0;
            projectile.timeLeft = 600;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
        }

        int phase = 0; //0 = invis, 1 = raising, 2 = raised
        public override bool PreAI()
        {
            projectile.velocity.X = 0;
            switch (phase)
            {
                case 0:
                    projectile.velocity.Y = 24;
                    projectile.alpha = 255;
                    break;
                case 1:
                    projectile.velocity.Y = -6;
                    projectile.alpha = 0;
                    if (projectile.timeLeft < 70)
                    {
                        phase = 2;
                    }
                    break;
                case 2:
                    projectile.friendly = false;
                    projectile.velocity.Y = 0;
                    break;
            }
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.Y != projectile.velocity.Y && phase == 0)
            {
                projectile.velocity.Y = -6;
                phase = 1;
                projectile.timeLeft = 80;
                projectile.alpha = 0;
                projectile.tileCollide = false;
                projectile.position.Y += 20;
                projectile.friendly = true;
            }
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            fallThrough = false;
            return true;
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!target.noGravity)
            {
                target.velocity.Y = -10;
            }
        }

    }
}