using StarlightRiver.Content.Packets;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets
{
	public abstract class EvasionProjectile : ModProjectile
	{
		public ref float ParentIdentity => ref Projectile.ai[0];
		public EvasionShrineDummy Parent => DummySystem.dummies.FirstOrDefault(n => n is EvasionShrineDummy && (n as EvasionShrineDummy).identity == ParentIdentity) as EvasionShrineDummy;

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			Parent.lives--;

			if (Main.LocalPlayer.whoAmI == target.whoAmI)
			{
				PlayerHitPacket hitPacket = new PlayerHitPacket(Projectile.identity, target.whoAmI, info.Damage, Projectile.type);
				hitPacket.Send(-1, Main.LocalPlayer.whoAmI, false);

				if (Main.rand.NextBool(10000))
					Main.NewText("Skill issue.");
			}
		}
	}
}