using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    internal class BossPit : DummyTile
    {
        public override int DummyType => ProjectileType<BossPitDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 11, 4, DustID.Stone, SoundID.Tink, false, new Color(50, 50, 50));
        }

        public override void SafeNearbyEffects(int i, int j, bool closer)
        {
            Main.tileSolid[Type] = Dummy.ai[1] == 0;
        }
    }

    internal class BossPitDummy : Dummy, IDrawAdditive
    {
        public BossPitDummy() : base(TileType<BossPit>(), 11 * 16, 4 * 16) { }

        public override void Update()
        {
            if (projectile.ai[1] == 1) //opening
            {
                if (projectile.ai[0] < 88) projectile.ai[0] += 4;
            }

            if (projectile.ai[1] == 2) //closing
            {
                projectile.ai[0] -= 4;
                if (projectile.ai[0] <= 0) projectile.ai[1] = 0;
            }
            if (projectile.ai[1] == 1 && !Main.npc.Any(n => n.active && n.type == NPCType<Bosses.OvergrowBoss.OvergrowBoss>())) projectile.ai[1] = 2;

            Lighting.AddLight(projectile.position + new Vector2(88, 0), new Vector3(1, 1, 0.4f) * (projectile.ai[0] / 88f));
            if (projectile.ai[0] > 0)
            {
                Dust.NewDustPerfect(new Vector2(projectile.position.X + (88 - projectile.ai[0] + Main.rand.NextFloat(projectile.ai[0] * 2)), projectile.position.Y + 56), DustType<Content.Dusts.GoldWithMovement>(), new Vector2(0, Main.rand.NextFloat(-3, -1)));
            }

            //lightning
            if (projectile.ai[0] == 88 && Main.rand.Next(8) == 0)
            {
                DrawHelper.DrawElectricity(projectile.position + new Vector2(Main.rand.Next(176), 60), projectile.position + new Vector2(Main.rand.Next(2) == 0 ? 0 : 176, 0), DustType<Dusts.GoldNoMovement>(), 0.5f);
            }

            if (projectile.ai[1] != 0)
            {
                //collision
                foreach (Player player in Main.player.Where(p => p.Hitbox.Intersects(new Rectangle((int)projectile.position.X, (int)projectile.position.Y + 30, 176, 32))))
                {
                    player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " got cooked extra crispy..."), 120, 0);
                    player.velocity.Y -= 30;
                }

                //flail zapping
                foreach (NPC flail in Main.npc.Where(p => p.active && p.type == NPCType<Bosses.OvergrowBoss.OvergrowBossFlail>()
                && p.Hitbox.Intersects(new Rectangle((int)projectile.position.X, (int)projectile.position.Y, 176, 32)) && p.life <= 1 && p.ai[2] == 0))
                {
                    flail.ai[2] = 1; //tells the flail it is zapped
                    flail.ai[1] = 0; //resets the flail's timer
                    flail.velocity *= 0; //freezes the flail
                }
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 pos = projectile.position - Main.screenPosition;
            Texture2D tex1 = GetTexture("StarlightRiver/Assets/Tiles/Overgrow/PitCover");

            spriteBatch.Draw(tex1, pos + new Vector2(88 + projectile.ai[0], 0), tex1.Frame(), lightColor);
            spriteBatch.Draw(tex1, pos + new Vector2(-projectile.ai[0], 0), tex1.Frame(), lightColor, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Vector2 pos = projectile.position - Main.screenPosition;
            Texture2D tex0 = GetTexture("StarlightRiver/Assets/Tiles/Overgrow/PitGlowBig");
            Rectangle rect = new Rectangle((int)pos.X + 88 - (int)projectile.ai[0], (int)pos.Y - 52, (int)projectile.ai[0] * 2, 116);

            spriteBatch.Draw(tex0, rect, tex0.Frame(), new Color(255, 255, 120) * (projectile.ai[0] / 88f));
        }
    }
}