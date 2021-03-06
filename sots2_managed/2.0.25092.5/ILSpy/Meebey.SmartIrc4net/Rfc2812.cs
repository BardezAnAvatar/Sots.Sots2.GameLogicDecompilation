using System;
using System.Text.RegularExpressions;
namespace Meebey.SmartIrc4net
{
	public sealed class Rfc2812
	{
		private static Regex _NicknameRegex = new Regex("^[A-Za-z\\[\\]\\\\`_^{|}][A-Za-z0-9\\[\\]\\\\`_\\-^{|}]+$", RegexOptions.Compiled);
		private Rfc2812()
		{
		}
		public static bool IsValidNickname(string nickname)
		{
			return nickname != null && nickname.Length > 0 && Rfc2812._NicknameRegex.Match(nickname).Success;
		}
		public static string Pass(string password)
		{
			return "PASS " + password;
		}
		public static string Nick(string nickname)
		{
			return "NICK " + nickname;
		}
		public static string User(string username, int usermode, string realname)
		{
			return string.Concat(new string[]
			{
				"USER ",
				username,
				" ",
				usermode.ToString(),
				" * :",
				realname
			});
		}
		public static string Oper(string name, string password)
		{
			return "OPER " + name + " " + password;
		}
		public static string Privmsg(string destination, string message)
		{
			return "PRIVMSG " + destination + " :" + message;
		}
		public static string Notice(string destination, string message)
		{
			return "NOTICE " + destination + " :" + message;
		}
		public static string Join(string channel)
		{
			return "JOIN " + channel;
		}
		public static string Join(string[] channels)
		{
			string str = string.Join(",", channels);
			return "JOIN " + str;
		}
		public static string Join(string channel, string key)
		{
			return "JOIN " + channel + " " + key;
		}
		public static string Join(string[] channels, string[] keys)
		{
			string str = string.Join(",", channels);
			string str2 = string.Join(",", keys);
			return "JOIN " + str + " " + str2;
		}
		public static string Part(string channel)
		{
			return "PART " + channel;
		}
		public static string Part(string[] channels)
		{
			string str = string.Join(",", channels);
			return "PART " + str;
		}
		public static string Part(string channel, string partmessage)
		{
			return "PART " + channel + " :" + partmessage;
		}
		public static string Part(string[] channels, string partmessage)
		{
			string str = string.Join(",", channels);
			return "PART " + str + " :" + partmessage;
		}
		public static string Kick(string channel, string nickname)
		{
			return "KICK " + channel + " " + nickname;
		}
		public static string Kick(string channel, string nickname, string comment)
		{
			return string.Concat(new string[]
			{
				"KICK ",
				channel,
				" ",
				nickname,
				" :",
				comment
			});
		}
		public static string Kick(string[] channels, string nickname)
		{
			string str = string.Join(",", channels);
			return "KICK " + str + " " + nickname;
		}
		public static string Kick(string[] channels, string nickname, string comment)
		{
			string text = string.Join(",", channels);
			return string.Concat(new string[]
			{
				"KICK ",
				text,
				" ",
				nickname,
				" :",
				comment
			});
		}
		public static string Kick(string channel, string[] nicknames)
		{
			string str = string.Join(",", nicknames);
			return "KICK " + channel + " " + str;
		}
		public static string Kick(string channel, string[] nicknames, string comment)
		{
			string text = string.Join(",", nicknames);
			return string.Concat(new string[]
			{
				"KICK ",
				channel,
				" ",
				text,
				" :",
				comment
			});
		}
		public static string Kick(string[] channels, string[] nicknames)
		{
			string str = string.Join(",", channels);
			string str2 = string.Join(",", nicknames);
			return "KICK " + str + " " + str2;
		}
		public static string Kick(string[] channels, string[] nicknames, string comment)
		{
			string text = string.Join(",", channels);
			string text2 = string.Join(",", nicknames);
			return string.Concat(new string[]
			{
				"KICK ",
				text,
				" ",
				text2,
				" :",
				comment
			});
		}
		public static string Motd()
		{
			return "MOTD";
		}
		public static string Motd(string target)
		{
			return "MOTD " + target;
		}
		[Obsolete("use Lusers() method instead")]
		public static string Luser()
		{
			return Rfc2812.Lusers();
		}
		public static string Lusers()
		{
			return "LUSERS";
		}
		[Obsolete("use Lusers(string) method instead")]
		public static string Luser(string mask)
		{
			return Rfc2812.Lusers(mask);
		}
		public static string Lusers(string mask)
		{
			return "LUSER " + mask;
		}
		[Obsolete("use Lusers(string, string) method instead")]
		public static string Luser(string mask, string target)
		{
			return Rfc2812.Lusers(mask, target);
		}
		public static string Lusers(string mask, string target)
		{
			return "LUSER " + mask + " " + target;
		}
		public static string Version()
		{
			return "VERSION";
		}
		public static string Version(string target)
		{
			return "VERSION " + target;
		}
		public static string Stats()
		{
			return "STATS";
		}
		public static string Stats(string query)
		{
			return "STATS " + query;
		}
		public static string Stats(string query, string target)
		{
			return "STATS " + query + " " + target;
		}
		public static string Links()
		{
			return "LINKS";
		}
		public static string Links(string servermask)
		{
			return "LINKS " + servermask;
		}
		public static string Links(string remoteserver, string servermask)
		{
			return "LINKS " + remoteserver + " " + servermask;
		}
		public static string Time()
		{
			return "TIME";
		}
		public static string Time(string target)
		{
			return "TIME " + target;
		}
		public static string Connect(string targetserver, string port)
		{
			return "CONNECT " + targetserver + " " + port;
		}
		public static string Connect(string targetserver, string port, string remoteserver)
		{
			return string.Concat(new string[]
			{
				"CONNECT ",
				targetserver,
				" ",
				port,
				" ",
				remoteserver
			});
		}
		public static string Trace()
		{
			return "TRACE";
		}
		public static string Trace(string target)
		{
			return "TRACE " + target;
		}
		public static string Admin()
		{
			return "ADMIN";
		}
		public static string Admin(string target)
		{
			return "ADMIN " + target;
		}
		public static string Info()
		{
			return "INFO";
		}
		public static string Info(string target)
		{
			return "INFO " + target;
		}
		public static string Servlist()
		{
			return "SERVLIST";
		}
		public static string Servlist(string mask)
		{
			return "SERVLIST " + mask;
		}
		public static string Servlist(string mask, string type)
		{
			return "SERVLIST " + mask + " " + type;
		}
		public static string Squery(string servicename, string servicetext)
		{
			return "SQUERY " + servicename + " :" + servicetext;
		}
		public static string List()
		{
			return "LIST";
		}
		public static string List(string channel)
		{
			return "LIST " + channel;
		}
		public static string List(string[] channels)
		{
			string str = string.Join(",", channels);
			return "LIST " + str;
		}
		public static string List(string channel, string target)
		{
			return "LIST " + channel + " " + target;
		}
		public static string List(string[] channels, string target)
		{
			string str = string.Join(",", channels);
			return "LIST " + str + " " + target;
		}
		public static string Names()
		{
			return "NAMES";
		}
		public static string Names(string channel)
		{
			return "NAMES " + channel;
		}
		public static string Names(string[] channels)
		{
			string str = string.Join(",", channels);
			return "NAMES " + str;
		}
		public static string Names(string channel, string target)
		{
			return "NAMES " + channel + " " + target;
		}
		public static string Names(string[] channels, string target)
		{
			string str = string.Join(",", channels);
			return "NAMES " + str + " " + target;
		}
		public static string Topic(string channel)
		{
			return "TOPIC " + channel;
		}
		public static string Topic(string channel, string newtopic)
		{
			return "TOPIC " + channel + " :" + newtopic;
		}
		public static string Mode(string target)
		{
			return "MODE " + target;
		}
		public static string Mode(string target, string newmode)
		{
			return "MODE " + target + " " + newmode;
		}
		public static string Service(string nickname, string distribution, string info)
		{
			return string.Concat(new string[]
			{
				"SERVICE ",
				nickname,
				" * ",
				distribution,
				" * * :",
				info
			});
		}
		public static string Invite(string nickname, string channel)
		{
			return "INVITE " + nickname + " " + channel;
		}
		public static string Who()
		{
			return "WHO";
		}
		public static string Who(string mask)
		{
			return "WHO " + mask;
		}
		public static string Who(string mask, bool ircop)
		{
			if (ircop)
			{
				return "WHO " + mask + " o";
			}
			return "WHO " + mask;
		}
		public static string Whois(string mask)
		{
			return "WHOIS " + mask;
		}
		public static string Whois(string[] masks)
		{
			string str = string.Join(",", masks);
			return "WHOIS " + str;
		}
		public static string Whois(string target, string mask)
		{
			return "WHOIS " + target + " " + mask;
		}
		public static string Whois(string target, string[] masks)
		{
			string str = string.Join(",", masks);
			return "WHOIS " + target + " " + str;
		}
		public static string Whowas(string nickname)
		{
			return "WHOWAS " + nickname;
		}
		public static string Whowas(string[] nicknames)
		{
			string str = string.Join(",", nicknames);
			return "WHOWAS " + str;
		}
		public static string Whowas(string nickname, string count)
		{
			return string.Concat(new string[]
			{
				"WHOWAS ",
				nickname,
				" ",
				count,
				" "
			});
		}
		public static string Whowas(string[] nicknames, string count)
		{
			string text = string.Join(",", nicknames);
			return string.Concat(new string[]
			{
				"WHOWAS ",
				text,
				" ",
				count,
				" "
			});
		}
		public static string Whowas(string nickname, string count, string target)
		{
			return string.Concat(new string[]
			{
				"WHOWAS ",
				nickname,
				" ",
				count,
				" ",
				target
			});
		}
		public static string Whowas(string[] nicknames, string count, string target)
		{
			string text = string.Join(",", nicknames);
			return string.Concat(new string[]
			{
				"WHOWAS ",
				text,
				" ",
				count,
				" ",
				target
			});
		}
		public static string Kill(string nickname, string comment)
		{
			return "KILL " + nickname + " :" + comment;
		}
		public static string Ping(string server)
		{
			return "PING " + server;
		}
		public static string Ping(string server, string server2)
		{
			return "PING " + server + " " + server2;
		}
		public static string Pong(string server)
		{
			return "PONG " + server;
		}
		public static string Pong(string server, string server2)
		{
			return "PONG " + server + " " + server2;
		}
		public static string Error(string errormessage)
		{
			return "ERROR :" + errormessage;
		}
		public static string Away()
		{
			return "AWAY";
		}
		public static string Away(string awaytext)
		{
			return "AWAY :" + awaytext;
		}
		public static string Rehash()
		{
			return "REHASH";
		}
		public static string Die()
		{
			return "DIE";
		}
		public static string Restart()
		{
			return "RESTART";
		}
		public static string Summon(string user)
		{
			return "SUMMON " + user;
		}
		public static string Summon(string user, string target)
		{
			return "SUMMON " + user + " " + target;
		}
		public static string Summon(string user, string target, string channel)
		{
			return string.Concat(new string[]
			{
				"SUMMON ",
				user,
				" ",
				target,
				" ",
				channel
			});
		}
		public static string Users()
		{
			return "USERS";
		}
		public static string Users(string target)
		{
			return "USERS " + target;
		}
		public static string Wallops(string wallopstext)
		{
			return "WALLOPS :" + wallopstext;
		}
		public static string Userhost(string nickname)
		{
			return "USERHOST " + nickname;
		}
		public static string Userhost(string[] nicknames)
		{
			string str = string.Join(" ", nicknames);
			return "USERHOST " + str;
		}
		public static string Ison(string nickname)
		{
			return "ISON " + nickname;
		}
		public static string Ison(string[] nicknames)
		{
			string str = string.Join(" ", nicknames);
			return "ISON " + str;
		}
		public static string Quit()
		{
			return "QUIT";
		}
		public static string Quit(string quitmessage)
		{
			return "QUIT :" + quitmessage;
		}
		public static string Squit(string server, string comment)
		{
			return "SQUIT " + server + " :" + comment;
		}
	}
}
