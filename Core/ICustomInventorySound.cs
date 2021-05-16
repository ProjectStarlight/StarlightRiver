using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core
{
	interface ICustomInventorySound
	{
		SoundEffectInstance InventorySound(float pitch);
	}
}
