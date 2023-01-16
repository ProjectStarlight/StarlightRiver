using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Lavas
{
	public abstract class LavaStyle : ModWaterStyle
    {
        public string texturePath;

		public override string Texture => texturePath;

		public override void Load()
		{
            string name = "";
            string texture = "";
            string blockTexture = "";
            var value = SafeAutoload(ref name, ref texture, ref blockTexture);

            LavaLoader.lavas?.Add(this);
            texturePath = texture;
        }

        public virtual bool SafeAutoload(ref string name, ref string texture, ref string blockTexture) => true;

        public virtual bool DrawEffects(int x, int y) => false;

        public virtual void DrawBlockEffects(int x, int y, Tile up, Tile left, Tile right, Tile down) { }

        public string blockTexture;

        public virtual bool ChooseLavaStyle() => false;
    }
}
