using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Items.Underground
{
	public class GloomGel : QuickMaterial
	{
		public override string Texture => "StarlightRiver/Assets/Items/Underground/GloomGel";

		public GloomGel() : base("Gloom Gel", "", 9999, 20, 1) { }
	}
}