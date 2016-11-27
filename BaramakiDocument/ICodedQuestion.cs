using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aldentea.BaramakiMutus.Data
{
	public interface ICodedQuestion : GrandMutus.Data.IQuestionBase
	{
		string Code { get; set; }

		#region *Codeプロパティの実装例
		/*
		public string Code
		{
			get
			{
				return _code;
			}
			set
			{
				if (Code != value)
				{
					NotifyPropertyChanging("Code");
					this._code = value;
					NotifyPropertyChanged("Code");
				}
			}
		}
		string _code = string.Empty;
		*/
		#endregion

	}
}
