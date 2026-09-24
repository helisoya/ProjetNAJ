using ANF.Persistent;
using UnityEngine;


namespace ANF.Utils
{
	/// <summary>
	/// General ANF Utilities
	/// </summary>
	public class ANFUtils
	{

		/// <summary>
		/// Checks the result of the operation
		/// </summary>
		/// <param name="left">Left value</param>
		/// <param name="right">Right value</param>
		/// <param name="oper">The Operator</param>
		/// <returns>The result</returns>
		public static bool IsCheckOkay(int left, int right, string oper)
		{
			switch (oper)
			{
				case "==":
					return left == right;
				case ">":
					return left > right;
				case "<":
					return left < right;
				case ">=":
					return left >= right;
				case "<=":
					return left <= right;
				case "!=":
					return left != right;
			}

			return false;
		}

		/// <summary>
		/// Implementation for the if content check
		/// </summary>
		/// <param name="line">The current line to parse</param>
		/// <param name="container">The variable container</param>
		/// <param name="result">The out result</param>
		/// <returns>True if no problem was encountered</returns>
		public static bool CheckIfContentImpl(string line, PlayerVariableContainer container, out bool result)
		{
			result = true;
			bool isAnd = true;
			bool tmpResult;

			string[] operators = new string[] { "==", "<=", ">=", "<", ">", "!=" };

			string[] split = line.Split("&");
			if (split.Length == 1)
			{
				result = false;
				isAnd = false;
				split = line.Split("|");
			}

			foreach (string part in split)
			{
				foreach (string oper in operators)
				{
					if (part.Contains(oper))
					{
						string[] parametersSplit = part.Split(oper);
						if (parametersSplit.Length != 2)
						{
							result = false;
							return false;
						}

						string left = parametersSplit[0].Replace(" ", "");
						string right = parametersSplit[1].Replace(" ", "");

						int valueLeft;
						int valueRight;

						if (container == null || !container.GetVariable(left, out valueLeft))
						{
							if (!int.TryParse(left, out valueLeft))
							{
								result = false;
								return false;
							}
						}

						if (container == null || !container.GetVariable(right, out valueRight))
						{
							if (!int.TryParse(right, out valueRight))
							{
								result = false;
								return false;
							}
						}

						tmpResult = IsCheckOkay(valueLeft, valueRight, oper);

						if (!tmpResult && isAnd)
						{
							result = false;
							break;
						}
						else if (tmpResult && !isAnd)
						{
							result = true;
							break;
						}

						break;
					}
				}
			}
			return true;
		}
	}
}