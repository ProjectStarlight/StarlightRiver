using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Lavas
{
	public abstract class LavaStyle : ModWaterStyle
    {
        public string texturePath;

        public sealed override bool Autoload(ref string name, ref string texture, ref string blockTexture)
        {
            var value = SafeAutoload(ref name, ref texture, ref blockTexture);

            LavaLoader.lavas?.Add(this);
            texturePath = texture;
            return value;
        }

        public virtual bool SafeAutoload(ref string name, ref string texture, ref string blockTexture) => true;

        public virtual bool DrawEffects(int x, int y) => false;

        public virtual void DrawBlockEffects(int x, int y, Tile up, Tile left, Tile right, Tile down) { }

        public string blockTexture;

        public sealed override bool ChooseWaterStyle() => false;

        public virtual bool ChooseLavaStyle() => false;
    }
}
