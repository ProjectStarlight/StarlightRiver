using Microsoft.Xna.Framework;
using Terraria;


namespace StarlightRiver.Core.Loaders
{
	class PrimLoader : ILoadable
    {
        public float Priority { get => 1.09f; }

        public void Load()
        {
            if (Main.dedServ)
                return;

            StarlightRiver.primitives = new PrimTrailManager();
            StarlightRiver.primitives.LoadContent(Main.graphics.GraphicsDevice);

            On.Terraria.Main.DrawProjectiles += Main_DrawProjectiles;
            Main.OnPreDraw += Main_OnPreDraw;
        }

        private void Main_DrawProjectiles(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
        {
            StarlightRiver.primitives.DrawTarget(Main.spriteBatch);
            orig(self);
        }

        private void Main_OnPreDraw(GameTime obj)
        {
            if (Main.spriteBatch != null && StarlightRiver.primitives != null)
                StarlightRiver.primitives.DrawTrails(Main.spriteBatch, Main.graphics.GraphicsDevice);
        }
        public void Unload()
        {
            On.Terraria.Main.DrawProjectiles -= Main_DrawProjectiles;
            Main.OnPreDraw -= Main_OnPreDraw;
            StarlightRiver.primitives = null;
        }
    }
}
