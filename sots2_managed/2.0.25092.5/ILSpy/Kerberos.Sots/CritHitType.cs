using System;
namespace Kerberos.Sots
{
	public enum CritHitType
	{
		None,
		CrewDeath,
		CoolantSystem,
		ReactorCoolantLeak,
		AmmoConveyor,
		Scanners,
		ECM,
		ReactorShielding,
		TurretFailure,
		ModuleFailure,
		MagazineExplosion,
		CapacitorBreach,
		TurretMalfunction,
		SupplyHit,
		TurretHydrolics,
		RotThrusters,
		MainThrusters,
		HelmControls,
		LifeSupport,
		ShieldGenerator,
		TwoTimesDamage,
		ThreeTimesDamage,
		FiveTimesDamage,
		TenTimesDamage,
		TwentyTimesDamage,
		MaxCritHits
	}
}
