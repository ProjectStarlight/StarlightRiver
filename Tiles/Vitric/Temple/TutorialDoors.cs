using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Terraria.ModLoader.ModContent;
using Terraria;
using Terraria.ID;
using StarlightRiver.Projectiles.Dummies;
using StarlightRiver.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Tiles.Vitric.Temple
{
    class TutorialDoor1 : DummyTile
    {
        public override int DummyType => ProjectileType<TutorialDoor1Dummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }
    }

    class TutorialDoor1Dummy : Dummy
    {
        public TutorialDoor1Dummy() : base(16, 16 * 7, TileType<TutorialDoor1>()) { }
    }

    class TutorialDoor2 : DummyTile
    {
        public override int DummyType => ProjectileType<TutorialDoor2Dummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 2, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(100, 200, 255), false, true);
        }
    }

    class TutorialDoor2Dummy : Dummy
    {
        public TutorialDoor2Dummy() : base(16 * 2, 16 * 7, TileType<TutorialDoor2>()) { }

        public override void Update()
        {
            base.Update();
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(GetTexture("StarlightRiver/Tiles/Vitric/Temple/TutorialDoor2"), projectile.position, lightColor);
            spriteBatch.Draw(GetTexture("StarlightRiver/Tiles/Vitric/Temple/TutorialDoor2Glow"), projectile.position, Color.White * (float)Math.Sin(StarlightWorld.rottime));
        }
    }
}
