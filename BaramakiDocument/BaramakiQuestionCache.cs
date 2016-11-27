using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aldentea.Wpf.Document;

namespace Aldentea.BaramakiMutus.Data
{
	#region QuestionCodeChangedCacheクラス
	public class QuestionCodeChangedCache : GrandMutus.Data.PropertyChangedCache<string>
	{
		ICodedQuestion _question;

		public QuestionCodeChangedCache(ICodedQuestion question, string from, string to)
			: base(from, to)
		{
			this._question = question;
		}

		// DoとかReverseで実行するときにはOperationCacheを新規作成したくないわけだが...

		// →考えられる方法は2つ．
		// 1つは，通常のプロパティのsetterで値をセットするんだけど，キャッシュの作成を抑止する．
		// もう1つは，キャッシュを作成せずに値をセットする別の機構(internalメソッドか？)を用意する．
		// 並列実行の対応も気になりますが…


		//public override void Do()
		//{
		//	_song.Title = _currentValue;
		//}

		public override void Reverse()
		{
			_question.Code = _previousValue;
		}

		// そもそもoperationCache.Reverse(); だけでアンドゥできる仕組みだったのに，
		// 実装側のコードが複雑になってしまっては意味がないのではないか？

		public override bool CanCancelWith(IOperationCache other)
		{
			var other_cache = other as QuestionCodeChangedCache;
			if (other_cache == null)
			{ return false; }
			else
			{
				return other_cache._question == this._question &&
					other_cache._previousValue == this._currentValue &&
					other_cache._currentValue == this._previousValue;
			}
		}

		//public override IOperationCache GetInverse()
		//{
		//	return new SongTitleChangedCache(this._song, this._currentValue, this._previousValue);
		//}

	}
	#endregion
}
