using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;

namespace Aldentea.BaramakiMutus.Data
{

	#region BaramakiQuestionクラス
	public class BaramakiQuestion : SweetMutus.Data.SweetQuestion, ICodedQuestion
	{

		#region *コンストラクタ(BaramakiQuestion)
		public BaramakiQuestion() : base()
		{

		}

		public BaramakiQuestion(GrandMutus.Data.ISong song) : base(song)
		{

		}

		protected BaramakiQuestion(SweetMutus.Data.SweetQuestion sweetQuestion)
		{
			ID = sweetQuestion.ID;
			Category = sweetQuestion.Category;
			No = sweetQuestion.No;
			Title = sweetQuestion.Title;
			Artist = sweetQuestion.Artist;
			FileName = sweetQuestion.FileName;
			SabiPos = sweetQuestion.SabiPos;
			PlayPos = sweetQuestion.PlayPos;
			StopPos = sweetQuestion.StopPos;
		}
		#endregion

		#region ICodedQuestion実装

		// Codeプロパティを追加する。

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

		#endregion

		#region XML入出力関連

		const string CODE_ATTRIBUTE = "code";

		// ↓newを使ったが、元クラスのメソッドをvirtualにするべきか？

		public new XElement GenerateElement(string songs_root, bool exporting = false)
		{
			var element = base.GenerateElement(songs_root, exporting);
			element.Add(new XAttribute(CODE_ATTRIBUTE, this.Code));
			return element;
		}


		#region *[static]XML要素からオブジェクトを生成(Generate)
		public new static BaramakiQuestion Generate(XElement questionElement, string songsRoot = null)
		{
			var question = new BaramakiQuestion(SweetMutus.Data.SweetQuestion.Generate(questionElement, songsRoot));

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
	#endregion

}
