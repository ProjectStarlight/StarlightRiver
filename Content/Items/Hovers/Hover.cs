using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Hovers
{
	internal abstract class Hover : ModItem
	{
		readonly string texture;

		public override string Texture => texture;

		public Hover(string texture) : base()
		{
			this.texture = texture;
		}

		public override void SetStaticDefaults()
		{
			ItemID.Sets.Deprecated[Type] = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Clear();
			tooltips.Add(new(Mod, "SLRHoverInfo", "This should not apper! report to https://github.com/ProjectStarlight/StarlightRiver/issues"));
		}
	}

	internal class GenericHover : Hover
	{
		public GenericHover() : base("StarlightRiver/Assets/Misc/Exclaim") { }
	}

	internal class WindsHover : Hover
	{
		public WindsHover() : base("StarlightRiver/Assets/Abilities/ForbiddenWinds") { }
	}

	internal class FaeHover : Hover
	{
		public FaeHover() : base("StarlightRiver/Assets/Abilities/Faeflame") { }
	}
}