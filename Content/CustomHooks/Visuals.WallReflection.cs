using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
	class WallReflection : HookGroup
    {
        //Where Tiles are chosen for reflection, as well as appending these to the map
        public override SafetyLevel Safety => SafetyLevel.Safe;

        //Prpoerties to allow edit and continue :P
        private Vector2 ReflectionOffset => new Vector2(20, 0);
        private int TileSearchSize = 8;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On.Terraria.Projectile.Update += Projectile_Update;
            Main.OnPreDraw += Main_OnPreDraw;
        }

		public override void Unload()
		{
            Main.OnPreDraw -= Main_OnPreDraw;
        }

		private void Main_OnPreDraw(GameTime obj)
        {
            if (!Main.gameMenu)
            {
                TargetHost.GetMap("TileReflectableMap")?.DrawToBatchedTarget((spriteBatch) =>
                {
                    spriteBatch.Draw(PlayerTarget.Target, ReflectionOffset, Color.White);
                });

                TargetHost.GetMap("TileReflectionMap")?.DrawToBatchedTarget((spriteBatch) =>
                {
                    for (int i = -TileSearchSize; i < TileSearchSize; i++)
                        for (int j = -TileSearchSize; j < TileSearchSize; j++)
                        {
                            Point p = (Main.LocalPlayer.position / 16).ToPoint();
                            Point pij = new Point(p.X + i, p.Y + j);

                            if (WorldGen.InWorld(pij.X, pij.Y))
                            {
                                Tile tile = Framing.GetTileSafely(pij);
                                ushort type = tile.wall;

                                if (type == WallID.Glass
                                 || type == WallID.BlueStainedGlass
                                 || type == WallID.GreenStainedGlass
                                 || type == WallID.PurpleStainedGlass
                                 || type == WallID.YellowStainedGlass
                                 || type == WallID.RedStainedGlass)
                                {
                                    Vector2 pos = pij.ToVector2() * 16;
                                    Texture2D tex = Main.wallTexture[type];
                                    if (tex != null) spriteBatch.Draw(Main.wallTexture[type], pos - Main.screenPosition - new Vector2(8, 8), new Rectangle(tile.wallFrameX(), tile.wallFrameY(), 36, 36), Color.White);
                                }
                            }

                        }
                });
            }
        }

        private void Projectile_Update(On.Terraria.Projectile.orig_Update orig, Projectile self, int i)
        {
            TargetHost.GetMap("TileReflectableMap").DrawToBatchedTarget((spriteBatch) =>
            {
                Texture2D tex = Main.projectileTexture[self.type];
                if (tex != null && self.active) spriteBatch.Draw(tex, self.position - Main.screenPosition + ReflectionOffset, tex.Bounds, Color.White, self.rotation, Vector2.Zero, self.scale, SpriteEffects.None, 0f);
            });

            orig(self, i);
        }
    }
}