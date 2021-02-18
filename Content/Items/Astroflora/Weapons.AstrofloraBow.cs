using Terraria;
using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using StarlightRiver.Helpers;
using Terraria.Graphics.Effects;
using System;

namespace StarlightRiver.Content.Items.Astroflora
{
    public class AstrofloraBow : ModItem
    {
        public override string Texture => AssetDirectory.Astroflora + "AstrofloraBow";

        private List<NPC> locks;

        private const int maxLocks = 3;

        private const string SoundPath = "Sounds/Custom/Astroflora/";

        public bool CursorShouldBeRed { get; private set; }

        private int counter;

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("TBA");
        }

        public override void SetDefaults()
        {
            // Balance requiered on all stats (I have no idea what point in progression this is).
            item.damage = 200;
            item.ranged = true;

            item.useTime = 30;
            item.useAnimation = 30;

            item.shootSpeed = 1;
            item.shoot = ProjectileID.PurificationPowder; // Dummy since Shoot hook changes the result.
            item.useAmmo = AmmoID.Arrow;

            item.noMelee = true;

            item.useStyle = ItemUseStyleID.HoldingOut;
            item.UseSound = SoundID.Item5;

            item.rare = ItemRarityID.Blue;
            item.value = Item.sellPrice(0, 1, 0, 0);
        }

        public override void HoldItem(Player player)
        {
            if (CursorShouldBeRed && --counter <= 0)
            {
                counter = 0;
                CursorShouldBeRed = false;
            }

            locks = locks ?? new List<NPC>();

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (locks.Contains(npc) && (!npc.CanBeChasedBy() || (npc.CanBeChasedBy() && !npc.GetGlobalNPC<AstrofloraLocksGlobalNPC>().Locked)))
                {
                    locks.Remove(npc);
                }

                Rectangle generousHitbox = npc.Hitbox;
                generousHitbox.Inflate(npc.Hitbox.Width / 3, npc.Hitbox.Height / 3);

                if (npc.CanBeChasedBy() && !npc.GetGlobalNPC<AstrofloraLocksGlobalNPC>().Locked && locks.Count < maxLocks && !locks.Contains(npc) && generousHitbox.Contains(Main.MouseWorld.ToPoint()))
                {
                    Say("Target Locked!", player);

                    locks.Add(npc);

                    npc.GetGlobalNPC<AstrofloraLocksGlobalNPC>().Locked = true;

                    // TODO: Play some kind of lock-on sound effect?
                }
            }
        }

        private void Say(string text, Player player)
        {
            // Main.fontCombatText[0] is just the variant used when dramatic == false.
            Vector2 textSize = Main.fontCombatText[0].MeasureString(text);

            Rectangle textRectangle = new Rectangle((int)player.MountedCenter.X, (int)(player.MountedCenter.Y + player.height), (int)textSize.X, (int)textSize.Y);

            CombatText.NewText(textRectangle, Main.cursorColor, text);
        }

        public override bool CanUseItem(Player player)
        {
            if (locks.Count > 0)
            {
                return true;
            }
            else
            {
                player.GetModPlayer<StarlightPlayer>().Shake = 10;

                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, $"{SoundPath}Failure"), player.Center);

                Say("No Locks!", player);

                CursorShouldBeRed = true;
                counter = 30;

                return false;
            }
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            for (int i = 0; i < maxLocks; i++)
            {
                int index;

                if (locks.Count == 0)
                {
                    index = -1;
                }
                else
                {
                    // Dictates which lock the projectile will go for. If three locks, it's one for each, else any excess projectiles target a random lock.
                    index = i > locks.Count - 1 ? Main.rand.Next(locks).whoAmI : locks[i].whoAmI;
                }

                Vector2 shotOffset = Vector2.Normalize(new Vector2(speedX, speedY)) * 32;

                Projectile.NewProjectile(position + shotOffset, new Vector2(speedX, speedY).RotatedBy((i - 1) * (MathHelper.PiOver4 / 2)) * 24, ModContent.ProjectileType<AstrofloraBolt>(), damage, knockBack, player.whoAmI, index);
            }

            locks.Clear();

            return false;
        }
    }

    public class AstrofloraBolt : ModProjectile, IDrawPrimitive
    {
        private const int oldPositionCacheLength = 120;

        private const int trailMaxWidth = 8;

        public override string Texture => AssetDirectory.Invisible;

        private Primitives primitives;

        private List<Vector2> cache;

        private int TargetNPCIndex
        {
            get => (int)projectile.ai[0];
            set => projectile.ai[0] = value;
        }

        private bool HitATarget
        {
            get => (int)projectile.ai[1] == 1;
            set => projectile.ai[1] = value ? 1 : 0;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.Homing[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;

            projectile.damage = 100;
            projectile.knockBack = 8;

            projectile.friendly = true;

            projectile.timeLeft = 300;

            projectile.tileCollide = false;

            projectile.penetrate = -1;
        }

        public override void AI()
        {
            // Sync its target.
            projectile.netUpdate = true;

            ManageCaches();

            PopulateVertexBuffer();

            if (projectile.timeLeft < 30)
            {
                projectile.alpha += 8;
            }

            if (!HitATarget)
            {
                projectile.velocity.Y = Math.Min(projectile.velocity.Y + 0.1f, 10);

                if (TargetNPCIndex == -1)
                {
                    return;
                }

                NPC target = Main.npc[TargetNPCIndex];

                if (!target.CanBeChasedBy())
                {
                    // Stop homing if the target NPC is no longer a valid target.
                    TargetNPCIndex = -1;

                    return;
                }

                Homing(target);
            }
        }

        private void Homing(NPC target)
        {
            Vector2 move = target.Center - projectile.Center;

            AdjustMagnitude(ref move);

            projectile.velocity = (10 * projectile.velocity + move) / 11f;

            AdjustMagnitude(ref projectile.velocity);
        }

        private void AdjustMagnitude(ref Vector2 vector)
        {
            float adjustment = 24;

            float magnitude = vector.Length();

            if (magnitude > adjustment)
            {
                vector *= adjustment / magnitude;
            }
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < oldPositionCacheLength; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            cache.Add(projectile.Center);

            while (cache.Count > oldPositionCacheLength)
            {
                cache.RemoveAt(0);
            }
        }

        private void PopulateVertexBuffer()
        {
            if (cache.Count < 3 || (primitives?.IsDisposed ?? false))
            {
                return;
            }

            Color color = new Color(31, 250, 131) * (1 - (projectile.alpha / 255f));

            List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
            List<short> indices = new List<short>();

            // We can omit 2 cache positions because the beginning and end are not counted, and then for each other cache position there are 4 vertices (plus 3 for the tip).
            int maxVertexCount = 3 + ((oldPositionCacheLength - 2) * 4);

            // We can omit 2 cache positions because the beginning and end are not counted, and then for each other cache position there are 6 indices (2 triangles, and 3 more indices for the tip).
            int maxIndexCount = 3 + ((oldPositionCacheLength - 2) * 6);

            primitives = primitives ?? new Primitives(Main.instance.GraphicsDevice, maxVertexCount, maxIndexCount);

            short currentVertex = 0;

            for (int i = 1; i < cache.Count - 1; i++)
            {
                // 1 at the start, 0 at the end of trail.
                float amountAlongTrail = ((float)(i - 1) / oldPositionCacheLength);

                float width = amountAlongTrail * trailMaxWidth;

                Vector2 previousCenter = cache[i - 1];
                Vector2 currentCenter = cache[i];
                Vector2 nextCenter = cache[i + 1];

                Vector2 prevToCur = (currentCenter - previousCenter).SafeNormalize(Vector2.Zero);
                Vector2 curToNext = (nextCenter - currentCenter).SafeNormalize(Vector2.Zero);

                /* Previous point indicated by prev, Current indicated by cur.
                 * We want to generate 2 triangles - ACD and ABD.
                 *   A -------- B
                 *   |          |
                 *  prev.      cur.
                 *   |          |
                 *   C -------- D
                */

                // Rotation is always anticlockwise so this is facing towards A from prev.
                Vector2 prevToA = prevToCur.RotatedBy(MathHelper.PiOver2);

                // And this is facing towards B from cur.
                Vector2 curToB = curToNext.RotatedBy(MathHelper.PiOver2);

                Vector3 a = (previousCenter + (prevToA * width)).Vec3();
                Vector3 b = (currentCenter + (curToB * width)).Vec3();
                Vector3 c = (previousCenter - (prevToA * width)).Vec3();
                Vector3 d = (currentCenter - (curToB * width)).Vec3();

                float currentUVX = (float)i / oldPositionCacheLength;
                float previousUVX = amountAlongTrail;

                vertices.AddRange(new VertexPositionColorTexture[]
                {
                    new VertexPositionColorTexture(a, color, new Vector2(previousUVX, 0)),
                    new VertexPositionColorTexture(b, color, new Vector2(currentUVX, 0)),
                    new VertexPositionColorTexture(c, color, new Vector2(previousUVX, 1)),
                    new VertexPositionColorTexture(d, color, new Vector2(currentUVX, 1))
                });

                indices.AddRange(new short[]
                {
                    // A, C, D.
                    currentVertex, (short)(currentVertex + 2), (short)(currentVertex + 3),

                    // D, B, A.
                    (short)(currentVertex + 3), (short)(currentVertex + 1), currentVertex
                });

                currentVertex += 4;
            }

            /* Now we need to make the tip of the trail. It'll look something like this:
             *     C
             *    / \
             *   /   \
             *  /     \
             * A ----- B
             * Forming a pointed triangular tip.
            */

            Vector2 center = cache[cache.Count - 2];

            Vector2 tipC = center + (projectile.velocity.SafeNormalize(Vector2.Zero) * trailMaxWidth * 4);
            Vector2 centerToTip = (tipC - center).SafeNormalize(Vector2.Zero);
            Vector2 centerToA = centerToTip.RotatedBy(MathHelper.PiOver2);

            float tipWidth = ((float)cache.Count / oldPositionCacheLength) * trailMaxWidth;

            Vector2 tipA = center + (centerToA * tipWidth);
            Vector2 tipB = center - (centerToA * tipWidth);

            if (HitATarget)
            {
                tipA = tipB = tipC = projectile.Center;
            }

            vertices.AddRange(new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(tipA.Vec3(), color, Vector2.UnitX),
                new VertexPositionColorTexture(tipB.Vec3(), color, Vector2.One),
                new VertexPositionColorTexture(tipC.Vec3(), color, Vector2.One)
            });

            indices.AddRange(new short[]
            {
                // A, B, C.
                currentVertex, (short)(currentVertex + 1), (short)(currentVertex + 2),
            });

            primitives.SetVertices(vertices.ToArray());
            primitives.SetIndices(indices.ToArray());
        }

        public void DrawPrimitives()
        {
            if (primitives != null && !primitives.IsDisposed)
            {
                Effect effect = Filters.Scene["AstrofloraPrimitives"].GetShader().Shader;

                Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
                Matrix view = Main.GameViewMatrix.ZoomMatrix;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["transformMatrix"].SetValue(world * view * projection);

                //effect.Parameters["noise"].SetValue(ModContent.GetTexture("Terraria/Misc/Perlin"));

                primitives.Render(effect);
            }
        }

        public override bool? CanHitNPC(NPC target)
            => TargetNPCIndex != -1 && !HitATarget && Main.npc[TargetNPCIndex] == target;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) 
        {
            target.GetGlobalNPC<AstrofloraLocksGlobalNPC>().Locked = false;

            projectile.timeLeft = 30;

            HitATarget = true;

            // This is hacky, but it lets the projectile keep its rotation without having to make an extra variable to cache it after it hits a target and "stops".
            projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * 0.0001f;
        } 

        public override void Kill(int timeLeft)
        {
            primitives?.Dispose();

            if (TargetNPCIndex > -1)
            {
                NPC npc = Main.npc[TargetNPCIndex];

                if (npc.active)
                {
                    npc.GetGlobalNPC<AstrofloraLocksGlobalNPC>().Locked = false;
                }
            }
        }
    }

    public class AstrofloraLocksGlobalNPC : GlobalNPC 
    {
        public override bool InstancePerEntity => true;

        public const int MaxLockDuration = 5 * 60; // 5 seconds, subject to change (1 second feels a bit short).

        public bool Locked
        {
            get => locked;
            set
            {
                if (value)
                {
                    remainingLockDuration = MaxLockDuration;
                }

                locked = value;
            }
        }

        private bool locked;

        public int remainingLockDuration;

        public override bool PreAI(NPC npc)
        {
            if (--remainingLockDuration <= 0)
            {
                Locked = false;
                remainingLockDuration = 0;
            }

            return base.PreAI(npc);
        }
    }
}
