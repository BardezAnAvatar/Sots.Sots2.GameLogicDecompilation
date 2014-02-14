using System;
namespace Kerberos.Sots.ShipFramework
{
	public class ObtainShipClassTypes
	{
		public static RealShipClasses GetRealShipClass(ShipClass legacyShipClass, BattleRiderTypes battleRiderType, string filename = "")
		{
			switch (legacyShipClass)
			{
			case ShipClass.Cruiser:
				if (battleRiderType != BattleRiderTypes.battlerider)
				{
					return RealShipClasses.Cruiser;
				}
				return RealShipClasses.BattleCruiser;
			case ShipClass.Dreadnought:
				if (battleRiderType != BattleRiderTypes.battlerider)
				{
					return RealShipClasses.Dreadnought;
				}
				return RealShipClasses.BattleShip;
			case ShipClass.Leviathan:
				return RealShipClasses.Leviathan;
			case ShipClass.BattleRider:
				switch (battleRiderType)
				{
				case BattleRiderTypes.Unspecified:
				case BattleRiderTypes.nodefighter:
				case BattleRiderTypes.patrol:
				case BattleRiderTypes.scout:
				case BattleRiderTypes.spinal:
				case BattleRiderTypes.escort:
				case BattleRiderTypes.interceptor:
				case BattleRiderTypes.torpedo:
				case BattleRiderTypes.battlerider:
					return RealShipClasses.BattleRider;
				case BattleRiderTypes.boardingpod:
					return RealShipClasses.BoardingPod;
				case BattleRiderTypes.drone:
					return RealShipClasses.Drone;
				case BattleRiderTypes.escapepod:
					return RealShipClasses.EscapePod;
				case BattleRiderTypes.assaultshuttle:
					return RealShipClasses.AssaultShuttle;
				case BattleRiderTypes.biomissile:
					return RealShipClasses.Biomissile;
				default:
					throw new ArgumentOutOfRangeException("battleRiderType");
				}
				break;
			case ShipClass.Station:
				if (filename.Contains("drone"))
				{
					return RealShipClasses.Platform;
				}
				return RealShipClasses.Station;
			default:
				throw new ArgumentOutOfRangeException("legacyShipClass");
			}
		}
		public static BattleRiderTypes GetBattleRiderTypeByName(ShipClass shipClass, string name)
		{
			BattleRiderTypes result = BattleRiderTypes.Unspecified;
			if (shipClass == ShipClass.BattleRider)
			{
				if (name.Contains("drone"))
				{
					result = BattleRiderTypes.drone;
				}
				else
				{
					if (name.Contains("assaultshuttle") || name.Contains("assault_shuttle"))
					{
						result = BattleRiderTypes.assaultshuttle;
					}
					else
					{
						if (name.Contains("boardingpod") || name.Contains("boarding_pod"))
						{
							result = BattleRiderTypes.boardingpod;
						}
						else
						{
							if (name.Contains("escapepod") || name.Contains("escape_pod"))
							{
								result = BattleRiderTypes.escapepod;
							}
							else
							{
								if (name.Contains("patrol"))
								{
									result = BattleRiderTypes.patrol;
								}
								else
								{
									if (name.Contains("scout"))
									{
										result = BattleRiderTypes.scout;
									}
									else
									{
										if (name.Contains("spinal"))
										{
											result = BattleRiderTypes.spinal;
										}
										else
										{
											if (name.Contains("escort"))
											{
												result = BattleRiderTypes.escort;
											}
											else
											{
												if (name.Contains("interceptor"))
												{
													result = BattleRiderTypes.interceptor;
												}
												else
												{
													if (name.Contains("biomissile") || name.Contains("bio_missile"))
													{
														result = BattleRiderTypes.biomissile;
													}
													else
													{
														if (name.Contains("torpedo"))
														{
															result = BattleRiderTypes.torpedo;
														}
														else
														{
															if (name.Contains("nodefighter"))
															{
																result = BattleRiderTypes.nodefighter;
															}
															else
															{
																result = BattleRiderTypes.battlerider;
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			else
			{
				if (name.ToLower().Contains("bb_") || name.ToLower().Contains("bc_"))
				{
					result = BattleRiderTypes.battlerider;
				}
			}
			return result;
		}
	}
}
