using NetEasy;
using StarlightRiver.Content.Archaeology;
using System;
using System.Linq;
using Terraria.DataStructures;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace StarlightRiver.Content.Packets
{
	/// <summary>
	/// Sent from server to clients
	/// notifies them to remove the tile entity and update the texturepath on the projectile
	/// sort of a large packet that could get split for bad performace but these shouldn't be super time sensitive nor frequent
	/// </summary>
	[Serializable]
	public class ArtifactSpawnPacket : Module
	{
		private readonly short x;
		private readonly short y;
		private readonly int artifactId;
		private readonly int projectileIdentity;
		private readonly string texturePath;

		public ArtifactSpawnPacket(int artifactId, short x, short y, int projectileIdentity, string texturePath)
		{
			this.x = x;
			this.y = y;
			this.artifactId = artifactId;
			this.projectileIdentity = projectileIdentity;
			this.texturePath = texturePath;
		}

		protected override void Receive()
		{
			if (TileEntity.ByID.ContainsKey(artifactId))
			{
				Artifact artifactEntity = TileEntity.ByID[artifactId] as Artifact;

				if (artifactEntity != null)
				{
					artifactEntity.Kill(x, y);
					ArtifactManager.artifacts.Remove(artifactEntity);
				}
			}

			ModContent.GetInstance<ArchaeologyMapLayer>().CalculateDrawables();

			Projectile proj = Main.projectile.FirstOrDefault(n => n.identity == projectileIdentity);

			if (proj is not null && proj.ModProjectile is not null)
			{
				ArtifactItemProj artifactProj = proj.ModProjectile as ArtifactItemProj;
				artifactProj.itemTexture = texturePath;
			}
		}
	}
}