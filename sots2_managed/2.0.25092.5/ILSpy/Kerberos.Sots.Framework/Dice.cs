using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;
namespace Kerberos.Sots.Framework
{
	public class Dice
	{
		private struct Die
		{
			public int Count;
			public int Sides;
			public override string ToString()
			{
				return string.Format("{0}D{1}", this.Count, this.Sides);
			}
		}
		private static readonly Random Random = new Random();
		private List<Dice.Die> _dice;
		private int _constant;
		[XmlAttribute("Value")]
		public string Value
		{
			get
			{
				return this.ToString();
			}
			set
			{
				Dice.Parse(this, value);
			}
		}
		[XmlIgnore]
		public bool IsZero
		{
			get
			{
				return this._constant == 0 && (this._dice == null || this._dice.Count <= 0);
			}
		}
		public int AverageRoll
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this._dice.Count; i++)
				{
					num += this._dice[i].Count * (this._dice[i].Sides + 1) / 2;
				}
				return num + this._constant;
			}
		}
		public Dice()
		{
		}
		public Dice(string value) : this()
		{
			Dice.Parse(this, value);
		}
		private void Clear()
		{
			if (this._dice != null)
			{
				this._dice.Clear();
			}
			this._constant = 0;
		}
		public int Roll()
		{
			return this.Roll(Dice.Random);
		}
		public int Roll(Random random)
		{
			return this.Roll(random, null);
		}
		public int Roll(Random random, string results)
		{
			if (results != null)
			{
				results = string.Empty;
			}
			int num = 0;
			if (this._dice != null)
			{
				for (int i = 0; i < this._dice.Count; i++)
				{
					for (int j = 0; j < this._dice[i].Count; j++)
					{
						int num2 = random.NextInclusive(1, this._dice[i].Sides);
						if (results != null)
						{
							if (!string.IsNullOrEmpty(results))
							{
								results += " + ";
							}
							results += string.Format("{0}(D{1}", num2, this._dice[i].Sides);
						}
						num += num2;
					}
				}
			}
			if (this._constant != 0)
			{
				num += this._constant;
				if (results != null)
				{
					if (!string.IsNullOrEmpty(results))
					{
						results += " + ";
					}
					results += this._constant;
				}
			}
			if (results != null && num != this._constant)
			{
				results.Insert(0, string.Format("{0}=", num));
			}
			return num;
		}
		public override string ToString()
		{
			string text = string.Empty;
			if (this._dice != null)
			{
				for (int i = 0; i < this._dice.Count; i++)
				{
					if (!string.IsNullOrEmpty(text))
					{
						text += " + ";
					}
					text += this._dice[i];
				}
			}
			if (this._constant != 0)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += " + ";
				}
				text += this._constant;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = "0";
			}
			return text;
		}
		private static void AddDice(List<Dice.Die> dice, int count, int sides)
		{
			for (int i = 0; i < dice.Count; i++)
			{
				if (dice[i].Sides == sides)
				{
					dice[i] = new Dice.Die
					{
						Count = dice[i].Count + sides,
						Sides = dice[i].Sides
					};
					return;
				}
			}
			dice.Add(new Dice.Die
			{
				Count = count,
				Sides = sides
			});
		}
		private static void Parse(Dice output, string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				output.Clear();
				return;
			}
			try
			{
				for (int i = 0; i < value.Length; i++)
				{
					char c = value[i];
					if (!char.IsDigit(c) && !char.IsWhiteSpace(c) && c != 'D' && c != 'd' && c != '+' && c != '-')
					{
						throw new ArgumentException(string.Format("Invalid character {0}", c));
					}
				}
				List<Dice.Die> dice = new List<Dice.Die>();
				int num = 0;
				string[] array = value.Split(new char[]
				{
					'+'
				});
				string[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					string text = array2[j];
					string[] array3 = text.Split(new char[]
					{
						'D',
						'd'
					});
					if (array3.Length != 1 || array3.Length != 2)
					{
						if (array3.Length == 1)
						{
							num += int.Parse(array3[0], NumberFormatInfo.InvariantInfo);
						}
						else
						{
							if (array3.Length != 2)
							{
								throw new ArgumentException("Invalid format.  Should be '3D6' or '17'");
							}
							int count = int.Parse(array3[0], NumberFormatInfo.InvariantInfo);
							int num2 = int.Parse(array3[1], NumberFormatInfo.InvariantInfo);
							if (num2 < 2)
							{
								throw new ArgumentException("Dice need at last 2 sides.");
							}
							Dice.AddDice(dice, count, num2);
						}
					}
				}
				output._dice = dice;
				output._constant = num;
			}
			catch (Exception innerException)
			{
				string message = string.Format("'{0}' is not a valid string for dice.", value);
				throw new ArgumentException(message, innerException);
			}
		}
	}
}
