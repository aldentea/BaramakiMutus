using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;

namespace Aldentea.BaramakiMutus.Data
{
	public class HazureQuestion : GrandMutus.Data.QuestionBase<BaramakiQuestionsCollection>, ICodedQuestion
	{

		#region *Codeプロパティ
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
		#endregion

		#region *Answerプロパティ
		public override string Answer
		{
			get
			{
				return "*ハズレ*";
			}
		}
		#endregion

		// ここまででビルドは通る。


		#region XML入出力関連

		public const string ELEMENT_NAME = "hazure";
		const string CODE_ATTRIBUTE = "code";
		const string ID_ATTRIBUTE = "id";	// これ親クラスでprotected指定していいのでは？

		public XElement GenerateElement()
		{
			return new XElement(ELEMENT_NAME,
				new XAttribute(ID_ATTRIBUTE, this.ID),
				new XAttribute(CODE_ATTRIBUTE, Code)
			);
		}


		#region *[static]XML要素からオブジェクトを生成(Generate)
		public static HazureQuestion Generate(XElement questionElement)
		{
			// 要素名は呼び出し元でチェックすることとし、ここではチェックしない。
			var question = new HazureQuestion();

			var id_attribute = questionElement.Attribute(ID_ATTRIBUTE);
			if (id_attribute != null)
			{
				question.ID = (int)id_attribute;
			}

			var code_attribute = questionElement.Attribute(CODE_ATTRIBUTE);
			if (code_attribute != null)
			{
				question.Code = code_attribute.Value;
			}
			return question;
		}
		#endregion


		#endregion


	}

}
