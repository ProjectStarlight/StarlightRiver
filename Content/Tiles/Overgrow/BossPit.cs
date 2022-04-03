using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

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
            Main.tileSolid[Type] = Dummy(i, j).ai[1] == 0;
        }
    }

    internal class BossPitDummy : Dummy, IDrawAdditive
    {
        public BossPitDummy() : base(TileType<BossPit>(), 11 * 16, 4 * 16) { }

        public override void Update()
        {
            if (Projectile.ai[1] == 1) //opening
            {
                if (Projectile.ai[0] < 88) Projectile.ai[0] += 4;
            }

            if (Projectile.ai[1] == 2) //closing
            {
                Projectile.ai[0] -= 4;
                if (Projectile.ai[0] <= 0) Projectile.ai[1] = 0;
            }
            if (Projectile.ai[1] == 1 && !Main.npc.Any(n => n.active && n.type == NPCType<Bosses.OvergrowBoss.OvergrowBoss>())) Projectile.ai[1] = 2;

            Lighting.AddLight(Projectile.position + new Vector2(88, 0), new Vector3(1, 1, 0.4f) * (Projectile.ai[0] / 88f));
            if (Projectile.ai[0] > 0)
            {
                Dust.NewDustPerfect(new Vector2(Projectile.position.X + (88 - Projectile.ai[0] + Main.rand.NextFloat(Projectile.ai[0] * 2)), Projectile.position.Y + 56), DustType<Content.Dusts.GoldWithMovement>(), new Vector2(0, Main.rand.NextFloat(-3, -1)));
            }

            //lightning
            if (Projectile.ai[0] == 88 && Main.rand.Next(8) == 0)
            {
                DrawHelper.DrawElectricity(Projectile.position + new Vector2(Main.rand.Next(176), 60), Projectile.position + new Vector2(Main.rand.Next(2) == 0 ? 0 : 176, 0), DustType<Dusts.GoldNoMovement>(), 0.5f);
            }

            if (Projectile.ai[1] != 0)
            {
                //collision
                foreach (Player Player in Main.player.Where(p => p.Hitbox.Intersects(new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y + 30, 176, 32))))
                {
                    Player.Hurt(PlayerDeathReason.ByCustomReason(Player.name + " got cooked extra crispy..."), 120, 0);
                    Player.velocity.Y -= 30;
                }

                //flail zapping
                foreach (NPC flail in Main.npc.Where(p => p.active && p.type == NPCType<Bosses.OvergrowBoss.OvergrowBossFlail>()
                && p.Hitbox.Intersects(new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, 176, 32)) && p.life <= 1 && p.ai[2] == 0))
                {
                    flail.ai[2] = 1; //tells the flail it is zapped
                    flail.ai[1] = 0; //resets the flail's timer
                    flail.velocity *= 0; //freezes the flail
                }
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 pos = Projectile.position - Main.screenPosition;
            Texture2D tex1 = Request<Texture2D>("StarlightRiver/Assets/Tiles/Overgrow/PitCover").Value;

            spriteBatch.Draw(tex1, pos + new Vector2(88 + Projectile.ai[0], 0), tex1.Frame(), lightColor);
            spriteBatch.Draw(tex1, pos + new Vector2(-Projectile.ai[0], 0), tex1.Frame(), lightColor, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Vector2 pos = Projectile.position - Main.screenPosition;
            Texture2D tex0 = Request<Texture2D>("StarlightRiver/Assets/Tiles/Overgrow/PitGlowBig").Value;
            Rectangle rect = new Rectangle((int)pos.X + 88 - (int)Projectile.ai[0], (int)pos.Y - 52, (int)Projectile.ai[0] * 2, 116);

            spriteBatch.Draw(tex0, rect, tex0.Frame(), new Color(255, 255, 120) * (Projectile.ai[0] / 88f));
        }
    }
}