using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System;
namespace Kerberos.Sots.Combat
{
	internal class VonNeumannChildSpawnLocations
	{
		private Ship m_VonNeumannMom;
		private Ship m_VonNeumannChild;
		private Vector3 m_DirectionFromMom;
		private float m_OffsetDistance;
		public Vector3 DirectionFromMom
		{
			get
			{
				return this.m_DirectionFromMom;
			}
		}
		public float OffsetDistance
		{
			get
			{
				return this.m_OffsetDistance;
			}
		}
		public Ship GetChildInSpace()
		{
			return this.m_VonNeumannChild;
		}
		public VonNeumannChildSpawnLocations(Ship mom, Vector3 dir, float offset)
		{
			this.m_DirectionFromMom = dir;
			this.m_OffsetDistance = offset;
			this.m_VonNeumannMom = mom;
			this.m_VonNeumannChild = null;
		}
		public bool CanSpawnAtLocation()
		{
			return this.m_VonNeumannChild == null;
		}
		public Vector3 GetSpawnLocation()
		{
			Matrix mat = Matrix.CreateRotationYPR(this.m_VonNeumannMom.Maneuvering.Rotation);
			mat.Position = this.m_VonNeumannMom.Maneuvering.Position;
			Vector3 vec = this.m_DirectionFromMom * this.m_OffsetDistance;
			return Vector3.Transform(vec, mat);
		}
		public void SpawnAtLocation(Ship child)
		{
			this.m_VonNeumannChild = child;
		}
		public void Clear()
		{
			this.m_VonNeumannChild = null;
		}
		public void Update()
		{
			if (this.m_VonNeumannChild == null)
			{
				return;
			}
			Vector3 spawnLocation = this.GetSpawnLocation();
			if ((this.m_VonNeumannChild.Position - spawnLocation).LengthSquared > 2500f)
			{
				this.Clear();
			}
		}
	}
}
