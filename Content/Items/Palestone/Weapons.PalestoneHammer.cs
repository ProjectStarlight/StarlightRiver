using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
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
            Item.channel = true;
            Item.damage = 12;
            Item.width = 24;
            Item.height = 24;
            Item.useTime = 320;
            Item.useAnimation = 320;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.melee = true;
            Item.noMelee = true;
            Item.knockBack = 8;
            Item.useTurn = false;
            Item.value = Item.sellPrice(0, 1, 42, 0);
            Item.rare = 0;
            Item.autoReuse = false;
            Item.shoot = Mod.ProjectileType("PalecrusherProj");
            Item.shootSpeed = 6f;
            Item.noUseGraphic = true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);
    }
     class PalecrusherProj : ClubProj
     {
        public override string Texture => AssetDirectory.PalestoneItem + "PalecrusherProj";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pale Crusher");
            Main.projFrames[Projectile.type] = 2;
        }

        int[] targets = new int[3];

        public override void Smash(Vector2 position)
        {
            Player Player = Main.player[Projectile.owner];
            for (int k = 0; k <= 100; k++)
            {
                Dust.NewDustPerfect(Projectile.oldPosition + new Vector2(Projectile.width / 2, Projectile.height / 2), DustType<Content.Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * Projectile.ai[0] / 10f);
            }
            for (int k = 0; k < 3; k++)
            {
                int range = 40;
                int target = -1;
                float lowestDist = float.MaxValue;
                for (int i = 0; i < 200; ++i)
                {
                    bool match = false;
                    NPC NPC = Main.npc[i];
                    foreach (int j in targets)
                    {
                        if (j == i)
                        {
                            match = true;
                            break;
                        }
                    }
                    if (NPC.active && NPC.CanBeChasedBy(Projectile) && !NPC.friendly && !match && !NPC.noGravity)
                    {
                        float dist = Projectile.Distance(NPC.Center);
                        if (dist / 16 < range)
                        {
                            if (dist < lowestDist)
                            {
                                lowestDist = dist;

                                target = NPC.whoAmI;
                                Projectile.netUpdate = true;
                            }
                        }
                    }
                }
                targets[k] = target;
                if (targets[k] != -1)
                {
                    NPC NPC = Main.npc[targets[k]];
                    Projectile.NewProjectile(NPC.Center.X, NPC.Center.Y - 32, 0, 0, ModContent.ProjectileType<PalePillar>(), Projectile.damage / 3, Projectile.knockBack / 2, Projectile.owner);
                }
            }
            for (int j = 0; j < 3; j++)
            {
                if (targets[j] != -1)
                {
                    NPC NPC = Main.npc[targets[j]];
                    Projectile.NewProjectile(NPC.Center.X, NPC.Center.Y - 32, 0, 0, ModContent.ProjectileType<PalePillar>(), Projectile.damage / 3, Projectile.knockBack / 2, Projectile.owner);
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
            Projectile.hide = true;
            Projectile.hostile = false;
            Projectile.width = 30;
            Projectile.height = 5;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.damage = 1;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        int phase = 0; //0 = invis, 1 = raising, 2 = raised
        public override bool PreAI()
        {
            Projectile.velocity.X = 0;
            switch (phase)
            {
                case 0:
                    Projectile.velocity.Y = 24;
                    Projectile.alpha = 255;
                    break;
                case 1:
                    Projectile.velocity.Y = -6;
                    Projectile.alpha = 0;
                    if (Projectile.timeLeft < 70)
                    {
                        phase = 2;
                    }
                    break;
                case 2:
                    Projectile.friendly = false;
                    Projectile.velocity.Y = 0;
                    break;
            }
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.Y != Projectile.velocity.Y && phase == 0)
            {
                Projectile.velocity.Y = -6;
                phase = 1;
                Projectile.timeLeft = 80;
                Projectile.alpha = 0;
                Projectile.tileCollide = false;
                Projectile.position.Y += 20;
                Projectile.friendly = true;
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