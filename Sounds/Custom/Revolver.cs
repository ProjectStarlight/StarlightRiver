using Microsoft.Xna.Framework.Audio;
using Terraria.ModLoader;

namespace StarlightRiver.Sounds.Custom
{
	public class Revolver : ModSound
	{
        public override SoundEffectInstance PlaySound(ref SoundEffectInstance soundInstance, float volume, float pan, SoundType type)
        {
            soundInstance = sound.CreateInstance();
            return soundInstance;
        }
    }
}