using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using BoardKey = System.ValueTuple<string, int>;

namespace Assets
{
	public class Utility
	{
		public static string Save_QTable(Dictionary<BoardKey, float> data)
		{
			// Build up the string data.
			StringBuilder builder = new StringBuilder();
			foreach (var pair in data)
			{
				builder.Append(pair.Key).Append(":").Append(pair.Value).Append('\n');
			}
			string result = builder.ToString();
			// Remove the end comma.
			result = result.TrimEnd(' ');
			return result;
		}

		public static Dictionary<BoardKey, float> Load_Qtable(string value)
		{
			var result = new Dictionary<BoardKey, float>();
			//string value = File.ReadAllText(file);
			// Split the string.
			string[] tokens = value.Split(new string[] { ":", "\n" },
				StringSplitOptions.RemoveEmptyEntries);
			// Build up our dictionary from the string.
			for (int i = 0; i < tokens.Length; i += 2)
			{
				string dict_key = tokens[i];
				string dict_value = tokens[i + 1];


				var key_token = dict_key.Substring(1, dict_key.Length - 2);

				string[] key_tokens = key_token.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);


				BoardKey tempkey = new BoardKey(key_tokens[0], int.Parse(key_tokens[1]));

				result.Add(tempkey, float.Parse(dict_value));


			}
			return result;
		}
	}
}
