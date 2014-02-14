using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace Kerberos.Sots.Framework
{
	public static class CsvOperations
	{
		public const char DefaultDelimiter = ',';
		public const char DefaultQuoteChar = '"';
		public static string[] SplitLine(string line, char quoteChar, char delimiter)
		{
			return CsvOperations.ParseQuotedStrings(line, quoteChar, delimiter, 0).ToArray<string>();
		}
		public static IEnumerable<string[]> Read(IFileSystem fileSystem, string filename, char quoteChar, char delimiter, int firstColumn, int maxColumns)
		{
			IEnumerable<string[]> result;
			using (Stream stream = fileSystem.CreateStream(filename))
			{
				using (StreamReader streamReader = new StreamReader(stream))
				{
					result = CsvOperations.Read(streamReader, quoteChar, delimiter, firstColumn, maxColumns);
				}
			}
			return result;
		}
		public static IEnumerable<string[]> Read(string filename, char quoteChar, char delimiter, int firstColumn, int maxColumns)
		{
			using (StreamReader streamReader = new StreamReader(filename))
			{
				foreach (string[] current in CsvOperations.Read(streamReader, quoteChar, delimiter, firstColumn, maxColumns))
				{
					yield return current;
				}
			}
			yield break;
		}
		public static IEnumerable<string[]> Read(string filename, char quoteChar, char delimiter, int firstColumn, int maxColumns, Encoding enc)
		{
			using (StreamReader streamReader = new StreamReader(filename, enc))
			{
				foreach (string[] current in CsvOperations.Read(streamReader, quoteChar, delimiter, firstColumn, maxColumns))
				{
					yield return current;
				}
			}
			yield break;
		}
		public static IEnumerable<string[]> Read(StreamReader csvStream, char quoteChar, char delimiter, int firstColumn, int maxColumns)
		{
			string toParse = string.Empty;
			int num = 0;
			List<string[]> list = new List<string[]>();
			while ((toParse = csvStream.ReadLine()) != null)
			{
				num++;
				list.Add(CsvOperations.ParseQuotedStrings(toParse, quoteChar, delimiter, num).Skip(firstColumn).Take(maxColumns).ToArray<string>());
			}
			return list;
		}
		public static string ToCSV(string[] textlines)
		{
			return CsvOperations.ToCSV(textlines, '"', ',');
		}
		public static string ToCSV(string[] textlines, char quoteChar, char delimiter)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < textlines.Length; i++)
			{
				string value = textlines[i];
				stringBuilder.Append(CsvOperations.QuoteStr(value, quoteChar));
				stringBuilder.Append(delimiter);
			}
			stringBuilder = stringBuilder.Remove(stringBuilder.Length - 1, 1);
			return stringBuilder.ToString();
		}
		private static string ExtractQuotedStr(string value, char quotedChar)
		{
			string text = string.Empty;
			int num = 0;
			if (string.IsNullOrWhiteSpace(value) || value[0] != quotedChar)
			{
				return value;
			}
			for (int i = 1; i < value.Length - 1; i++)
			{
				if (value[i] == quotedChar)
				{
					num++;
					if (num == 2)
					{
						text += quotedChar;
						num = 0;
					}
				}
				else
				{
					text += value[i];
				}
			}
			return text;
		}
		private static string QuoteStr(string value, char quoteChar)
		{
			if (value.Contains(" ") || value.Contains(quoteChar) || value.Contains(Environment.NewLine))
			{
				string arg = quoteChar.ToString();
				for (int i = 0; i < value.Length; i++)
				{
					if (value[i] == quoteChar)
					{
						arg += quoteChar;
					}
					arg += value[i];
				}
				return arg + quoteChar;
			}
			return value;
		}
		private static IEnumerable<string> ParseQuotedStrings(string toParse, char quoteChar, char delimiter, int line)
		{
			string text = string.Empty;
			bool flag = false;
			for (int i = 0; i < toParse.Length; i++)
			{
				if (toParse[i] != quoteChar & toParse[i] != delimiter)
				{
					text += toParse[i];
				}
				else
				{
					if (toParse[i] == delimiter)
					{
						if (!flag)
						{
							yield return CsvOperations.ExtractQuotedStr(text, quoteChar);
							text = string.Empty;
						}
						else
						{
							text += toParse[i];
						}
					}
					else
					{
						if (toParse[i] == quoteChar && i != 0 && i - 1 > 0 && toParse[i - 1] == delimiter)
						{
							flag = true;
							text += toParse[i];
						}
						else
						{
							if (toParse[i] == quoteChar & i == toParse.Length - 1)
							{
								text += toParse[i];
							}
							else
							{
								if (toParse[i] == quoteChar & toParse[i + 1] == delimiter)
								{
									flag = false;
									text += toParse[i];
								}
								else
								{
									if (toParse[i] == quoteChar)
									{
										text += toParse[i];
									}
									else
									{
										if (toParse[i] == delimiter)
										{
											text += toParse[i];
										}
									}
								}
							}
						}
					}
				}
			}
			yield return CsvOperations.ExtractQuotedStr(text, quoteChar);
			text = string.Empty;
			yield break;
		}
	}
}
