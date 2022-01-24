using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
	
	public class EnumData
	{

		public enum EBoardResult
		{
			Draw = 0,
			O_Win = 1,
			X_Win = -1,
			Continue = 2
		}

		public enum EBoardState
		{
			O = 1,
			X = -1,
			E = 0,
		}



	}
}
