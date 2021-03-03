using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;

namespace StarlightRiver.Content.NPCs.Vitric
{
    internal class MagmitePassive : ModNPC
    {
        public ref float ActionState => ref npc.ai[0];
        public ref float ActionTimer => ref npc.ai[1];

        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/MagmitePassive";

        public override bool Autoload(ref string name)
        {
            mod.AddGore("StarlightRiver/Assets/NPCs/Vitric/MagmiteGore", new MagmiteGore());
            return true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Small Magmite");
        }

        public override void SetDefaults()
        {
            npc.width = 24;
            npc.height = 14;
            npc.damage = 0;
            npc.defense = 0;
            npc.lifeMax = 25;
            npc.aiStyle = -1;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }

        public override void AI()
        {
            var x = (int)(npc.Center.X / 16) + npc.direction; //check 1 tile infront of la cretura
            var y = (int)(npc.Center.Y / 16);
            var tile = Framing.GetTileSafely(x, y);
            var tileUp = Framing.GetTileSafely(x, y - 1);
            var tileFar = Framing.GetTileSafely(x + npc.direction * 2, y - 1);
            var tileUnder = Framing.GetTileSafely(x, y + 1);

            ActionTimer++;

            if (Main.rand.Next(10) == 0)
                Gore.NewGoreDirect(npc.Center, (Vector2.UnitY * -3).RotatedByRandom(0.2f), ModGore.GetGoreSlot("StarlightRiver/Assets/NPCs/Vitric/MagmiteGore"), Main.rand.NextFloat(0.5f, 0.8f));

            if (ActionState == 0)
            {
                if (npc.velocity.X == 0 && tile.slope() == 0 && !tile.halfBrick() && tile.collisionType == 1 && tileUp.collisionType == 0) //climb up small cliffs
                {
                    ActionState = 1;
                    npc.velocity *= 0;
                    ActionTimer = 0;
                    return;
                }

                npc.velocity.X += 0.05f * (Main.player[Main.myPlayer].Center.X > npc.Center.X ? 1 : -1);
                npc.velocity.X = Math.Min(npc.velocity.X, 1.5f);
                npc.velocity.X = Math.Max(npc.velocity.X, -1.5f);

                npc.direction = npc.velocity.X > 0 ? 1 : -1;
                npc.spriteDirection = npc.velocity.X > 0 ? 1 : -1;

                if(tileFar.collisionType == 1 && npc.velocity.Y == 0) //jump up big cliffs
                {
                    npc.velocity.Y -= 8;
                }

                if(tileUnder.collisionType == 0 && npc.velocity.Y == 0) //hop off edges
                {
                    npc.velocity.Y -= 4;
                }

                if (npc.velocity.Y != 0)
                {
                    npc.frame.X = 0;
                    npc.frame.Y = 0;
                }
                else
                {
                    npc.frame.X = 42;
                    npc.frame.Y = (int)((ActionTimer / 5) % 5) * 40;
                }
            }

            if(ActionState == 1)
            {
                if (ActionTimer == 60) 
                {
                    ActionState = 0;
                    ActionTimer = 0;
                    npc.position.Y -= 16;
                    npc.position.X += 26 * npc.direction;
                }

                npc.frame.X = 84;
                npc.frame.Y = (int)((ActionTimer / 60f) * 9) * 40;
            }

            npc.frame.Width = 42;
            npc.frame.Height = 40;
        }

        public override void NPCLoot()
        {
            for(int k = 0; k < 30; k++)
                Gore.NewGoreDirect(npc.Center, (Vector2.UnitY * Main.rand.NextFloat(-8, -1)).RotatedByRandom(0.5f), ModGore.GetGoreSlot("StarlightRiver/Assets/NPCs/Vitric/MagmiteGore"), Main.rand.NextFloat(0.5f, 0.8f));

            Main.PlaySound(SoundID.DD2_GoblinHurt, npc.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            var pos = npc.Center - Main.screenPosition + new Vector2(0, -8);

            if (ActionState == 1)
                pos += new Vector2(8 * npc.spriteDirection, -4);

            spriteBatch.Draw(GetTexture(Texture), pos, npc.frame, Color.White, 0, new Vector2(21, 20), 1, npc.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }

    internal class MagmiteGore : ModGore
    {
        public override void OnSpawn(Gore gore)
        {
            gore.timeLeft = 180;
            gore.sticky = true;
            gore.numFrames = 2;
            gore.behindTiles = true;
        }

        public override Color? GetAlpha(Gore gore, Color lightColor) => Color.White * (gore.scale < 0.5f ? gore.scale * 2 : 1);

        public override bool Update(Gore gore)
        {
            Lighting.AddLight(gore.position, new Vector3(0.1f, 0.1f, 0) * gore.scale);
            gore.scale *= 0.99f;

            if (gore.scale < 0.1f)
                gore.active = false;

            if (gore.velocity.Y == 0)
            {
                gore.velocity.X = 0;
                gore.rotation = 0;
                gore.frame = 1;
            }

            //gore.position += gore.velocity;

            if (gore.frame == 0) gore.velocity.Y += 0.5f;

            return true;
        }
    }
}