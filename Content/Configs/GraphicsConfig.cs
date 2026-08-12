using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace StarlightRiver.Content.Configs
{
	public class GraphicsConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Range(0, 1)]
		[Slider]
		[DefaultValue(1f)]
		public float ScreenshakeMult = 1;

		[Range(5, 30)]
		[DrawTicks]
		[Slider]
		[DefaultValue(5f)]
		public int LightingPollRate = 5;

		[DefaultValue(true)]
		public bool ParticlesActive = true;

		public ReflectionSubConfig ReflectionConfig = new();
	}

	public class ReflectionSubConfig
	{
		[DefaultValue(true)]
		public bool ReflectionsOn = true;

		[DefaultValue(true)]
		public bool PlayerReflectionsOn = true;

		[DefaultValue(true)]
		public bool NpcReflectionsOn = true;

		[DefaultValue(true)]
		public bool ProjReflectionsOn = true;

		[DefaultValue(true)]
		public bool DustReflectionsOn = true;

		public override bool Equals(object obj)
		{
			if (obj is ReflectionSubConfig other)
			{
				return other.ReflectionsOn == ReflectionsOn
						&& other.PlayerReflectionsOn == PlayerReflectionsOn
						&& other.NpcReflectionsOn == NpcReflectionsOn
						&& other.ProjReflectionsOn == ProjReflectionsOn
						&& other.DustReflectionsOn == DustReflectionsOn;
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Incase someone disables all the individual reflection components without disabling the entire system, we still want the extra optimization of disabling the entire system
		/// so this checks each indiviudal reflection component and returns true if any are reflecting while the system is on
		/// </summary>
		/// <returns></returns>
		public bool isReflectingAnything()
		{
			return ReflectionsOn && (PlayerReflectionsOn || NpcReflectionsOn || ProjReflectionsOn || DustReflectionsOn);
		}

		public override int GetHashCode()
		{
			return new { ReflectionsOn, PlayerReflectionsOn, NpcReflectionsOn, ProjReflectionsOn, DustReflectionsOn }.GetHashCode();
		}
	}
}