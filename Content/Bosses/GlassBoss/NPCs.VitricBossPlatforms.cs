using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    internal class VitricBossPlatformUp : MovingPlatform
    {
        public const int MaxHeight = 880;
        public override string Texture => AssetDirectory.GlassBoss + "VitricBossPlatform";

        public VitricBackdropLeft parent;
        public Vector2 storedCenter;

        public override bool CheckActive() => false;

        public override void SafeSetDefaults()
        {
            npc.width = 220;
            npc.height = 16;
            npc.noTileCollide = true;
            npc.dontCountMe = true;
        }

        public override void SafeAI()
        {
            /*AI fields:
             * 0: state
             * 1: rise time left
             * 2: acceleration delay
             */

            if (npc.ai[0] == 0)
                if (npc.ai[1] > 0)
                {
                    npc.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Risetime;
                    npc.ai[1]--;
                }
                else npc.velocity.Y = 0;

            if (npc.ai[0] == 1)
            {
                npc.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Scrolltime * (1f / parent.npc.ai[3]);
                if (npc.position.Y <= StarlightWorld.VitricBiome.Y * 16 + 16 * 16)
                    npc.position.Y += MaxHeight;

                npc.visualOffset = Vector2.One.RotatedByRandom(6.28f) * parent.shake * 0.5f;
            }

            if (storedCenter == Vector2.Zero && npc.velocity.Y == 0)
                storedCenter = npc.Center;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            var tex = ModContent.GetTexture(Texture);
            spriteBatch.Draw(tex, npc.Center + npc.visualOffset + Vector2.UnitY * 4 - Main.screenPosition, null, drawColor, 0, tex.Size() / 2, 1, 0, 0);
            return false;
        }
    }

    internal class VitricBossPlatformDown : VitricBossPlatformUp
    {
        public override void SafeAI()
        {
            /*AI fields:
             * 0: state
             * 1: rise time left
             * 2: acceleration delay
             */

            if (npc.ai[0] == 0)
                if (npc.ai[1] > 0)
                {
                    npc.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Risetime;
                    npc.ai[1]--;
                }
                else npc.velocity.Y = 0;

            if (npc.ai[0] == 1)
            {
                npc.velocity.Y = (float)MaxHeight / VitricBackdropLeft.Scrolltime * (1f / parent.npc.ai[3]);
                if (npc.position.Y >= StarlightWorld.VitricBiome.Y * 16 + 16 * 16 + MaxHeight)
                    npc.position.Y -= MaxHeight;

                npc.visualOffset = Vector2.One.RotatedByRandom(6.28f) * parent.shake * 0.5f;
            }

            if (storedCenter == Vector2.Zero && npc.velocity.Y == 0)
                storedCenter = npc.Center;

        }
    }

    internal class VitricBossPlatformUpSmall : VitricBossPlatformUp
    {
        public override string Texture => AssetDirectory.GlassBoss + "VitricBossPlatformSmall";

        public override void SafeSetDefaults()
        {
            npc.width = 100;
            npc.height = 16;
            npc.noTileCollide = true;
            npc.dontCountMe = true;
        }
    }

    internal class VitricBossPlatformDownSmall : VitricBossPlatformDown
    {
        public override string Texture => AssetDirectory.GlassBoss + "VitricBossPlatformSmall";

        public override void SafeSetDefaults()
        {
            npc.width = 100;
            npc.height = 16;
            npc.noTileCollide = true;
            npc.dontCountMe = true;
        }
    }
}