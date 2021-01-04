using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Lavas
{
    public abstract class LavaStyle : ModWaterStyle
    {
        public sealed override bool Autoload(ref string name, ref string texture, ref string blockTexture)
        {
            LavaLoader.lavas.Add(this);
            return SafeAutoload(ref name, ref texture, ref blockTexture);
        }

        public virtual bool SafeAutoload(ref string name, ref string texture, ref  string blockTexture)
        {
            return true;
        }

        public virtual void DrawEffects()
        {

        }

        public string blockTexture;

        public sealed override bool ChooseWaterStyle() => false;

        public virtual bool ChooseLavaStyle => false;
    }
}
