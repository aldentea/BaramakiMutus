﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using System.Xml;
using System.IO;

using Aldentea.SweetMutus.Data;
using GrandMutus.Data;
using Aldentea.Wpf.Document;

namespace Aldentea.BaramakiMutus.Data
{

	// SweetMutus.Data.SweetMutusGameDocumentをコピペしてみる。
	#region BaramakiMutusDocumentクラス
	public class BaramakiMutusDocument : Aldentea.Wpf.Document.DocumentWithOperationHistory, IMutusGameDocument
	{

		#region SweetMutusDocumentからのコピペ

		#region *Questionsプロパティ
		/// <summary>
		/// 問題のコレクションを取得します．
		/// </summary>
		public BaramakiQuestionsCollection Questions { get { return _questions; } }
		readonly BaramakiQuestionsCollection _questions;
		#endregion


		/*
		// (0.1.9)
		#region *Songsプロパティ
		/// <summary>
		/// Questionsプロパティと同じものを返しますが，ISongの集合として見たいとき(曲のインポートとか)に使います．
		/// ※プロパティの変更通知はされないかもしれません．
		/// </summary>
		public SweetQuestionsCollection Songs { get { return _questions; } }
		#endregion
		*/

		#region *WriterSettingsプロパティ
		public XmlWriterSettings WriterSettings
		{
			get { return _xmlWriterSettings; }
		}
		XmlWriterSettings _xmlWriterSettings;
		#endregion

		// (0.2.3)UndoCompletedイベントを拾うように改良．
		// (*0.3.3)Questions関連処理を追加。
		#region *コンストラクタ(SweetMutusDocument)
		public BaramakiMutusDocument()
		{
			// Questions関連処理
			_questions = new BaramakiQuestionsCollection(this);
			//_questions.QuestionsRemoved += Questions_QuestionsRemoved;
			_questions.ItemChanged += Songs_ItemChanged;
			_questions.RootDirectoryChanged += Questions_RootDirectoryChanged;
			/*
			//_questions.QuestionNoChanged += Questions_QuestionNoChanged;
			_questions.QuestionNoChangeCompleted += Questions_QuestionNoChanged;
			*/
			this.UndoCompleted += SweetMutusDocument_UndoCompleted;

			// カレントカテゴリ関連
			//this.Opened += SweetMutusDocument_Opened;

			// ログ関連
			//_logs.CollectionChanged += Logs_CollectionChanged;  // ※これだけでは、Orderが追加されたときにしかイベントが発生しない！？
			this.LogAdded += Logs_Changed;

			// XML出力関連処理
			_xmlWriterSettings = new XmlWriterSettings
			{
				Indent = true,
				NewLineHandling = NewLineHandling.Entitize
			};
		}

		#endregion

		#region *アイテム変更時(Songs_ItemChanged)
		// (0.1.4)QuestionNoChangedイベントを発生。
		// (0.1.2.1)QuestionCategoryChangedイベントを発生。
		// (0.2.0)Songs以外でも共通に使えるのではなかろうか？
		void Songs_ItemChanged(object sender, ItemEventArgs<IOperationCache> e) // Aldentea.Wpf.DocumentにもIOperationCacheがある．
		{
			var operationCache = (IOperationCache)e.Item;
			if (operationCache != null)
			{
				this.AddOperationHistory(operationCache);
				// ★ここに書くと，Undoのときにイベントが発生しないのでは...
				// →実際，発生しないので，その場合はSweetMutusDocument_UndoCompletedから同じ処理を呼び出す．
				NotifyOperation(operationCache);
			}
		}
		#endregion

		// (0.2.3)
		protected void NotifyOperation(IOperationCache operationCache)
		{
			/*
			if (operationCache is QuestionCategoryChangedCache)
			{
				this.QuestionCategoryChanged(this, EventArgs.Empty);
			}
			if (operationCache is QuestionNoChangedCache)
			{
				this.QuestionNoChanged(this, EventArgs.Empty);
			}
			*/
		}

		/// <summary>
		/// 問題のカテゴリ変更があったときに発生します。
		/// </summary>
		public event EventHandler QuestionCategoryChanged = delegate { };

		/// <summary>
		/// 問題のNo変更があったときに発生します。
		/// </summary>
		public event EventHandler QuestionNoChanged = delegate { };

		/*
				private void Logs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
				{
					switch (e.Action)
					{
						case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
							IEnumerable<Player> players =
								e.NewItems.Cast<Log>()
									.Where(log => log.PlayerID.HasValue)
									.Select(log => Players.Get(log.PlayerID.Value))
									.Distinct();
							foreach (var player in players)
							{
								UpdateScore(player);
							}
							break;
					}
				}
				*/
		private void Logs_Changed(object sender, LogEventArgs e)
		{
			if (e.PlayerID.HasValue)
			{
				UpdateScore(Players.Get(e.PlayerID.Value));
			}
		}


		protected void UpdateScore(Player player)
		{
			player.Score = Logs.AllLog.Where(log => log.PlayerID == player.ID && log.Code == "○").Sum(log => log.Value);
		}


		// (0.2.3)
		private void SweetMutusDocument_UndoCompleted(object sender, UndoCompletedEventArgs e)
		{
			NotifyOperation(e.OperationCache);
		}

		#region 曲関連(すべてコメントアウト)

		/*

		// Songオブジェクトはここで(のみ)作るようにする？

		/// <summary>
		/// (*0.3.4)現時点では未使用！
		/// </summary>
		List<string> _addedSongFiles = new List<string>();

		// (0.2.2.1)カテゴリを設定できるように改良。
		// (0.2.0)曲を追加しない場合にnullを返すように変更。
		#region *曲を追加(AddQuestions)
		/// <summary>
		/// このメソッドが直接呼び出されることは想定していません．
		/// 呼び出し元でAddSongsActionに設定されるActionの中で呼び出して下さい(ややこしい...)．
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public SweetQuestion AddQuestion(string fileName, string category = null)
		{
			SweetQuestion question = new SweetQuestion { FileName = fileName, Category = string.IsNullOrEmpty(category) ? string.Empty : category };
			if (LoadInformation(question))
			{
				return this.AddQuestion(question);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// questionがnullの場合は、何もせずにnullを返します。
		/// </summary>
		/// <param name="question"></param>
		/// <returns></returns>
		private SweetQuestion AddQuestion(SweetQuestion question)
		{
			if (question == null)
			{
				return null;
			}
			try
			{
				_questions.Add(question); // この後の処理でSongDuplicateExceptionが発生する。
				_addedSongFiles.Add(question.FileName);
				//SongAdded(this, new ItemEventArgs<Song> { Item = song });
				return question;
			}
			catch (SongDuplicateException)
			{
				// この時点ではsongが_songsに追加された状態になっているので、ここで削除する。
				_questions.Remove(question);
				return null;
			}
		}

		// (0.2.2.1)カテゴリを設定できるように改良。
		// (0.2.0)追加された問題がない場合は、操作履歴に追加しないように修正。
		public void AddQuestions(IEnumerable<string> fileNames, string category = null)
		{
			IList<SweetQuestion> added_questions;

			if (AddQuestionsAction == null)
			{
				// 同期的に実行．
				added_questions = new List<SweetQuestion>();
				foreach (var fileName in fileNames)
				{
					var question = AddQuestion(fileName, category);
					if (question != null)
					{ added_questions.Add(question); }
				}
			}
			else
			{
				// 通常は呼び出し元に制御を渡して，UIを表示する．
				added_questions = AddQuestionsAction.Invoke(fileNames);
			}
			if (added_questions.Count > 0)
			{
				AddOperationHistory(new SweetQuestionsAddedCache(this, added_questions.ToArray()));
			}
		}

		#endregion

		public Func<IEnumerable<string>, IList<SweetQuestion>> AddQuestionsAction = null;

		// (0.3.0)
		#region *問題を削除(RemoveQuestions)
		//public void RemoveSongs(IEnumerable<string> fileNames)
		//{
		//	RemoveSongs(fileNames.Select(fileName => _songs.FirstOrDefault(s => s.FileName == fileName)).Where(s => s != null));
		//}

		// (0.2.1)バグを修正(列挙体にたいする削除操作)。
		// (0.0.6.3)UIから削除する場合も，このメソッドを経由することにしたので，OperationCacheの追加はここで行う．
		// (0.3.1)OperationCacheの追加はQuestionsRemovedイベントハンドラで行うことにする
		// (曲の削除はUIから直接行われることが想定されるため)．
		// (0.3.0)
		public void RemoveQuestions(IEnumerable<SweetQuestion> questions)
		{
			var removed_questions = new List<SweetQuestion>();
			foreach (var question in questions.ToArray())
			{

				if (_questions.Remove(question))
				{
					removed_questions.Add(question);
				}
			}
			AddOperationHistory(new SweetQuestionsRemovedCache(this, removed_questions));
		}
		#endregion

		// (0.3.1)
		//void Questions_QuestionsRemoved(object sender, ItemEventArgs<IEnumerable<SweetQuestion>> e)
		//{
		//	AddOperationHistory(new SweetQuestionsRemovedCache(this, e.Item));
		//}

		// (0.2.0)staticを解除。boolを返すように変更。
		// (0.1.7)再生開始位置もロードするように変更．
		// HyperMutusからのパクリ．古いメソッドだけど，とりあえずそのまま使う．
		// 場所も未定．とりあえずstatic化してここに置いておく．
		#region *ファイルからメタデータを読み込み(LoadInformation)
		/// <summary>
		/// songのFileNameプロパティで指定されたファイルからメタデータを読み込みます．
		/// </summary>
		bool LoadInformation(SweetQuestion question)
		{
			SPP.Aldente.IID3Tag tag;
			try
			{
				tag = SPP.Aldente.AldenteMP3TagAccessor.ReadFile(question.FileName);
			}
			catch (IOException ex)
			{
				// 通知したい。
				if (Confirm(string.Format(
					"読み込みに失敗しました。 - {0} \n曲情報を読み込まずに曲ファイルを追加しますか？", ex.Message)))
				{
					tag = null;
				}
				else
				{
					return false;
				}
			}
			catch (ApplicationException)
			{
				tag = null;
			}
			question.Title = tag == null ? string.Empty : tag.Title;
			question.Artist = tag == null ? string.Empty : tag.Artist;
			question.SabiPos = tag == null ? TimeSpan.Zero : TimeSpan.FromSeconds(Convert.ToDouble(tag.SabiPos));
			question.PlayPos = tag == null ? TimeSpan.Zero : TimeSpan.FromSeconds(Convert.ToDouble(tag.StartPos));
			return true;
		}
		#endregion

		*/
		#endregion


		#region 問題関連(追加削除系はコメントアウト)

		// (0.1.4)
		#region *IDから問題を取得(FindQuestion)
		/// <summary>
		/// 与えられたIDに対応する問題を返します(内部でSingleメソッドを使っています)．
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ICodedQuestion FindQuestion(int id)
		{
			return _questions.Single(q => q.ID == id);
		}
		#endregion

		// (0.0.3)
		/// <summary>
		/// コードから問題を取得します。該当する問題がない場合は、nullを返します。
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public ICodedQuestion GetQuestionByCode(string code)
		{
			return Questions.SingleOrDefault(q => q.Code == code);
		}

		/*

		// (0.3.3)
		/// <summary>
		/// カテゴリと出題順から、問題を取得します。
		/// 該当するものがない場合はnullを返します。
		/// </summary>
		/// <param name="category"></param>
		/// <param name="no"></param>
		/// <returns></returns>
		public SweetQuestion GetQuestion(string category, int no)
		{
			return _questions.FirstOrDefault(q => { return q.Category == category && q.No == no; });
		}


		// (*0.4.5.1)
		#region *Questionの番号変更時(Questions_NoChanged)
		void Questions_QuestionNoChanged(object sender, ValueChangedEventArgs<int?> e)
		{
			var question = (SweetQuestion)sender;
			AddOperationHistory(new QuestionNoChangedCache(question, e.PreviousValue, e.CurrentValue));
		}
		#endregion


*/

		/*

		// (*0.4.1)
		#region *問題を追加(AddQuestions)
		/// <summary>
		/// 問題削除をアンドゥしたときに使うことを想定しています。
		/// (0.1.8からは，インポートの時にも使っています．)
		/// </summary>
		/// <param name="questions"></param>
		public void AddQuestions(IEnumerable<SweetQuestion> questions)
		{
			// ここは同期実行でいいでしょう。
			var added_questions = new List<SweetQuestion>();
			foreach (var question in questions)
			{
				var added_song = AddQuestion(question);
				if (added_song != null)
				{ added_questions.Add(added_song); }
			}
			AddOperationHistory(new SweetQuestionsAddedCache(this, added_questions.ToArray()));
		}
		#endregion

		// (0.2.2)category引数を追加。
		// (0.1.8)
		#region *曲をインポート(ImportSongs)
		public void ImportSongs(IEnumerable<GrandMutus.Data.ISong> songs, string category = null)
		{
			this.AddQuestions(songs.Select(song => new SweetQuestion(song) { Category = string.IsNullOrEmpty(category) ? string.Empty : category }));
		}
		#endregion

		*/

		// (*0.4.4)
		#region *曲のルートディレクトリ変更時(Questions_RootDirectoryChanged)
		void Questions_RootDirectoryChanged(object sender, ValueChangedEventArgs<string> e)
		{
			this.AddOperationHistory(new SongsRootDirectoryChangedCache(this.Questions, e.PreviousValue, e.CurrentValue));
		}
		#endregion

		#endregion


		#region ファイル入出力関連

		// <mutus version="3.0">
		//   <sweet>
		//     <questions>
		//        <question ... >

		public const string ROOT_ELEMENT_NAME = "mutus";
		const string VERSION_ATTERIBUTE = "version";
		const string SWEET_ELEMENT_NAME = "sweet";

		// (0.3.0)sweet要素を生成する部分を，GenerateSweetMutusElementに分離．
		// (0.0.1)エクスポートの場合に対応したつもりです．
		#region *XMLを生成(GenerateXML)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="destination_directory">出力されるXMLファイルのディレクトリのフルパスを与えます．</param>
		/// <param name="exported_songs_root">エクスポートするときは，その曲を格納するディレクトリの名前を与えます．
		/// そうでなければnullを与えます．</param>
		/// <returns></returns>
		public XDocument GenerateXml(string destination_directory, string exported_songs_root = null)
		{
			XDocument xdoc = new XDocument(new XElement(ROOT_ELEMENT_NAME, new XAttribute(VERSION_ATTERIBUTE, "3.0")));
			XElement sweet = GenerateSweetMutusElement(destination_directory, exported_songs_root);
			xdoc.Root.Add(sweet);
			return xdoc;
		}
		#endregion

		// (0.3.0)
		#region *[virtual]sweet要素を生成(GenerateSweetMutusElement)
		/// <summary>
		/// sweet要素を生成します．
		/// </summary>
		/// <param name="destination_directory"></param>
		/// <param name="exported_songs_root"></param>
		/// <returns></returns>
		protected virtual XElement GenerateSweetMutusElement(string destination_directory, string exported_songs_root = null)
		{
			XElement sweet = new XElement(SWEET_ELEMENT_NAME);
			sweet.Add(Questions.GenerateElement(destination_directory, exported_songs_root));
			// ログを追加．
			if (Logs.Count > 0)
			{
				sweet.Add(Logs.GenerateElement());
			}
			// プレイヤーを追加。
			if (Players.Count > 0)
			{
				sweet.Add(Players.GenerateElement());
			}
			return sweet;
		}
		#endregion

		/*

		// (0.3.0) ログとかは未対応．
		// (0.1.1)
		#region *HyperMutusのXMLを生成(GenerateMtuXML)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="destination_directory">出力されるXMLファイルのディレクトリのフルパスを与えます．</param>
		/// <param name="exported_songs_root">エクスポートするときは，その曲を格納するディレクトリの名前を与えます．
		/// そうでなければnullを与えます．</param>
		/// <returns></returns>
		public XDocument GenerateMtuXml(string destination_directory, string exported_songs_root = null)
		{
			XDocument xdoc = new XDocument(new XElement(ROOT_ELEMENT_NAME, new XAttribute(VERSION_ATTERIBUTE, "3.0")));

			xdoc.Root.Add(Questions.GenerateQuestionsElement());
			xdoc.Root.Add(Questions.GenerateSongsElement(destination_directory, exported_songs_root));

			return xdoc;
		}
		#endregion

		*/

		#endregion


		// (0.1.10).mtqファイルのエクスポートに対応．
		#region *エクスポート時にファイルを保存(SaveExport)
		/// <summary>
		/// エクスポート時にファイルを保存します．
		/// 曲ファイルのコピーは予め済ませておいて下さい．
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="songs_root"></param>
		public void SaveExport(string destination, string songs_root)
		{
			/*
			switch (Path.GetExtension(destination))
			{
				case ".mtq":
					// case ".mtu":
					using (XmlWriter writer = XmlWriter.Create(destination, this.WriterSettings))
					{
						GenerateMtuXml(Path.GetDirectoryName(destination), songs_root).WriteTo(writer);
					}
					break;
				default:
					using (XmlWriter writer = XmlWriter.Create(destination, this.WriterSettings))
					{
						GenerateXml(Path.GetDirectoryName(destination), songs_root).WriteTo(writer);
					}
					break;
			}
			*/
			using (XmlWriter writer = XmlWriter.Create(destination, this.WriterSettings))
			{
				GenerateXml(Path.GetDirectoryName(destination), songs_root).WriteTo(writer);
			}
		}
		#endregion


		#region DocumentBase実装

		// (0.1.0)基底クラスのメソッド名の変更を反映．
		protected override void InitializeDocument()
		{
			base.InitializeDocument();
			Questions.Initialize();
			Logs.Initialize();  // SweetMutusGameDocumentよりコピペ。
			Players.Initialize();
		}

		// (0.1.10)HyperMutusのファイルに対応...したつもり．
		// (0.1.3)mutus2のファイルに対応？
		// (*0.4.0.1)Songs.RootDirectoryの設定を追加。
		#region *[override]ファイルからロード(LoadDocument)
		protected override bool LoadDocument(string fileName)
		{
			using (XmlReader reader = XmlReader.Create(fileName))
			{
				var xdoc = XDocument.Load(reader);
				var root = xdoc.Root;

				// ☆ここから下は，継承先でオーバーライドできるようにしておきましょう．
				if (root.Name == ROOT_ELEMENT_NAME)
				{
					decimal? version = (decimal?)root.Attribute(VERSION_ATTERIBUTE);
					if (version.HasValue)
					{
						if (version >= 3.0M)
						{
							var result = TryLoadSweetMutusDocument(root, Path.GetDirectoryName(fileName));
							if (result.HasValue)
							{
								return result.Value;
							}
							/*
							else
							{
								result = TryLoadGrandMutusDocument(root, fileName);
								if (result.HasValue)
								{
									return result.Value;
								}
							}
							*/
						}
						/*
						else if (version >= 2.0M)
						{
							// mutus2のファイル？
							var songs = root.Element("songs");
							if (Confirm("mutus2のファイルを読み込みます．保存するときにSweetMutusの形式に変換することになります(情報の一部が失われることがあります)．\n"
								+ "処理を続行しますか？"))
							{
								NowLoading = true;
								try
								{
									this.Questions.LoadMutus2SongsElement(songs, Path.GetDirectoryName(fileName));
									this.IsConverted = true;
								}
								finally
								{
									NowLoading = false;
								}
								return true;
							}
							else
							{
								return false;
							}
						}
			*/
					}
				}

			}
			return false;
		}
		#endregion

		// (0.3.0)LoadElementsメソッドを分離．
		bool? TryLoadSweetMutusDocument(XElement root, string fileDirectory)
		{
			var sweet = root.Element(SWEET_ELEMENT_NAME);
			if (sweet != null)
			{
				// SweetMutusDocument！
				NowLoading = true;
				try
				{
					LoadElements(sweet, fileDirectory);
				}
				finally
				{
					NowLoading = false;
				}
				return true;
			}
			else
			{
				return null;
			}
		}

		// (0.3.0)
		protected virtual void LoadElements(XElement sweet, string fileDirectory)
		{
			this.Questions.LoadElement(sweet.Element(SweetQuestionsCollection.ELEMENT_NAME), fileDirectory);
			var logsElement = sweet.Element(LogsCollection.ELEMENT_NAME);
			if (logsElement != null)
			{
				Logs.LoadElement(logsElement);
			}
			var playersElement = sweet.Element(PlayersCollection.ELEMENT_NAME);
			if (playersElement != null)
			{
				Players.LoadElement(playersElement);
			}
		}

		#region GrandMutusドキュメント用(割愛)
		/*
		bool? TryLoadGrandMutusDocument(XElement root, string fileName)
		{
			var questions_element = root.Element(GrandMutus.Data.QuestionsCollection.ELEMENT_NAME);
			if (questions_element == null)
			{
				// 他のドキュメント形式を試してみる余地がある？(そんなのあるかどうかわからんけど)
				// ということでnullを返す．
				return null;
			}
			else
			{
				if (questions_element.Elements().All(elem => elem.Name == GrandMutus.Data.IntroQuestion.ELEMENT_NAME))
				{
					return LoadGrandMutusDocument(root, fileName);
				}
				else
				{
					// intro以外の問題を含むと厄介なので，rejectする．

					// ※ユーザに通知したい！
					return false;
				}
			}
		}

		bool LoadGrandMutusDocument(XElement root, string fileName)
		{
			// rootからMutusDocumentというメソッドはないんだっけ？
			MutusDocument doc = new MutusDocument();

			doc.LoadGrandMutusDocument(root, fileName); // 0.6.4.2以降！

			NowLoading = true;
			try
			{
				foreach (var q in doc.Questions)
				{
					IntroQuestion question = (IntroQuestion)q;
					var sweet_question = new SweetQuestion(question.Song);
					sweet_question.PlayPos = question.PlayPos;
					// intro_question側で未実装．
					//sweet_question.StopPos = question.StopPos;
					sweet_question.Category = question.Category;
					sweet_question.No = question.No;

					this.AddQuestion(sweet_question);
				}
			}
			finally { NowLoading = false; }
			return true;
		}
		*/
		#endregion

		#region *[override]ファイルに保存(SaveDocument)
		protected override bool SaveDocument(string destination)
		{
			/*
			// 拡張子に応じてフォーマットを決める。
			var ext = Path.GetExtension(destination); // extには"."を含む。

			
			switch (ext)
			{
				case ".mtu":
				case ".mtq":
					return SaveMtqDocument(destination);
				default:
					return SaveSmtDocument(destination);
			}
			*/
			return SaveSmtDocument(destination);
		}
		#endregion

		#region 保存関連メソッド

		bool SaveSmtDocument(string destination)
		{
			using (XmlWriter writer = XmlWriter.Create(destination, this.WriterSettings))
			{
				GenerateXml(Path.GetDirectoryName(destination)).WriteTo(writer);
			}
			// 基本的にtrueを返せばよろしい．
			// falseを返すべきなのは，保存する前にキャンセルした時とかかな？
			return true;
		}
		/*
		bool SaveMtqDocument(string destination)
		{
			using (XmlWriter writer = XmlWriter.Create(destination, this.WriterSettings))
			{
				GenerateMtuXml(Path.GetDirectoryName(destination)).WriteTo(writer);
			}
			return true;
		}
		*/
		#endregion

		#endregion

		#endregion

		#region SweetMutusGameDocumentからのコピペ

		#region *Logsプロパティ
		public LogsCollection Logs
		{
			get
			{
				return _logs;
			}
		}

		LogsCollection _logs = new LogsCollection();
		#endregion


		public event EventHandler<OrderEventArgs> OrderAdded = delegate { };
		public event EventHandler<OrderEventArgs> OrderRemoved = delegate { };

		/// <summary>
		/// ログが追加された時に発生します。
		/// </summary>
		public event EventHandler<LogEventArgs> LogAdded = delegate { };

		/// <summary>
		/// ログが削除されたときに発生します。
		/// </summary>
		public event EventHandler<LogEventArgs> LogRemoved = delegate { };


		#region *IsRehearsalプロパティ
		/// <summary>
		/// リハーサルモードであるかどうかの値を取得／設定します。
		/// リハーサルモードの場合は、出題関連の操作についてダーティフラグが立ちません。
		/// </summary>
		public bool IsRehearsal
		{
			get
			{
				return this._isRehearsal;
			}
			set
			{
				if (this.IsRehearsal != value)
				{
					this._isRehearsal = value;
					NotifyPropertyChanged("IsRehearsal");
				}
			}
		}
		bool _isRehearsal = true;
		#endregion



		#region ログ関連

		// (0.3.2)リハーサルモードを実装。
		// (0.3.0)
		#region *出題順を追加(AddOrder)
		public void AddOrder(int? questionID)
		{
			if (questionID.HasValue)
			{
				Logs.AddOrder(questionID.Value);
				if (!IsRehearsal)
				{
					AddOperationHistory(new AddOrderCache(this, questionID));
				}
			}
			else
			{
				Logs.AddFirstOrder();
			}
			this.OrderAdded(this, new OrderEventArgs(questionID));
		}
		#endregion

		// (0.3.0)
		//public bool AddFirstOrder()
		//{
		//	return Logs.AddFirstOrder();
		//}

		// (0.3.2)リハーサルモードを実装。
		// (0.3.0)
		public void RemoveOrder()
		{
			int? questionID = Logs.RemoveOrder();
			if (!IsRehearsal)
			{
				AddOperationHistory(new RemoveOrderCache(this, questionID));
			}
			this.OrderRemoved(this, new OrderEventArgs(null));
		}

		// (0.0.5)LogAddedイベントを発生するように変更。
		public void AddLog(string code, decimal value)
		{
			this.AddLog(null, code, value);
		}

		// (0.0.5)LogAddedイベントを発生するように変更。
		public void AddLog(int? playerID, string code, decimal value)
		{
			Logs.AddLog(playerID, code, value);
			if (!this.IsRehearsal)
			{
				AddOperationHistory(new AddLogCache(this, Logs.CurrentOrder.ID, code, value, playerID));
			}
			LogAdded(this, new LogEventArgs(code, value, playerID));
		}

		// (0.0.5)アンドゥ用？
		public void AddLog(Log log, int order_id)
		{
			Logs.First(o => o.ID == order_id).Add(log);
		}

		public void RemoveLog(int log_id)
		{
			var order = Logs.First(o => o.Any(lg => lg.ID == log_id));
			var log = order.First(lg => lg.ID == log_id);
			RemoveLog(log, order);
		}

		public void RemoveLog(int order_id, string code, decimal value, int? player_id)
		{
			var order = Logs.First(o => o.ID == order_id);
			var log = order.First(lg => lg.Code == code && lg.Value == value && lg.PlayerID == player_id);
			RemoveLog(log, order);
		}

		protected void RemoveLog(Log log, Order order)
		{
			order.Remove(log);
			if (!this.IsRehearsal)
			{
				AddOperationHistory(new RemoveLogCache(this, order.ID, log));
			}
			LogRemoved(this, new LogEventArgs(log.Code, log.Value, log.PlayerID, order.QuestionID));

		}


		#endregion


		// (0.0.8) TSV形式に対応。
		// (0.0.7) とりあえずaldentea形式のみ実装。
		public void ExportLog(StreamWriter writer, GameLogFormat format = GameLogFormat.TSV)
		{
			switch(format)
			{
				case GameLogFormat.TSV:
					ExportLogTSV(writer);
					return;
				case GameLogFormat.Aldentea:
					ExportLogAldentea(writer);
					return;
			}
		}

		// (0.0.8)
		protected virtual void ExportLogAldentea(StreamWriter writer)
		{

			Dictionary<int, decimal> score_table = new Dictionary<int, decimal>();
			foreach (var player in Players)
			{
				score_table.Add(player.ID, 0);
			}

			foreach (var order in this.Logs.Where(o => o.QuestionID.HasValue))
			{
				var q = this.Questions.Get(order.QuestionID.Value); 
				if (q is BaramakiQuestion)
				{
					var question = (BaramakiQuestion)q;
					var log = order.First();
					var player = Players.Get(log.PlayerID.Value);

					string q_string = $"{order.ID}. {player.Name}[{q.Code}]{question.Title}／{question.Artist}／";
					switch (log.Code)
					{
						case "○":
							score_table[player.ID] += log.Value;
							writer.WriteLine($"{q_string}○、{player.Name}＋{log.Value}→{score_table[player.ID]}");
							break;
						case "×":
							writer.WriteLine($"{q_string}×");
							break;
					}
				}
				else if (q is HazureQuestion)
				{
					var log = order.First();
					var rate = order.First(l => l.Code == "＋").Value;
					var player = Players.Get(log.PlayerID.Value);
					writer.WriteLine($"{order.ID}. {player.Name}[{q.Code}]*ハズレ*／(レート→{rate})");
				}

			}

		}

		// (0.0.8)
		protected virtual void ExportLogTSV(StreamWriter writer)
		{
			Dictionary<int, decimal> score_table = new Dictionary<int, decimal>();
			foreach (var player in Players)
			{
				score_table.Add(player.ID, 0);
			}

			foreach (var order in this.Logs.Where(o => o.QuestionID.HasValue))
			{
				var q = this.Questions.Get(order.QuestionID.Value);
				if (q is BaramakiQuestion)
				{
					var question = (BaramakiQuestion)q;
					var log = order.First();
					var player = Players.Get(log.PlayerID.Value);

					string q_string = $"{order.ID}	{player.Name}	{q.Code}	{question.Title}	{question.Artist}";
					switch (log.Code)
					{
						case "○":
							score_table[player.ID] += log.Value;
							writer.WriteLine($"{q_string}	○	{player.Name}	{log.Value}	{score_table[player.ID]}");
							break;
						case "×":
							writer.WriteLine($"{q_string}	×");
							break;
					}
				}
				else if (q is HazureQuestion)
				{
					var log = order.First();
					var rate = order.First(l => l.Code == "＋").Value;
					var player = Players.Get(log.PlayerID.Value);
					writer.WriteLine($"{order.ID}	{player.Name}	{q.Code}	*ハズレ*		レート	{rate}");
				}

			}

		}


		#endregion

		#region Player関連

		#region *Playersプロパティ
		public PlayersCollection Players
		{
			get
			{
				return _players;
			}
		}

		PlayersCollection _players = new PlayersCollection();

		// InitializeDocumentメソッドで初期化する。

		#endregion


		public event EventHandler<PlayerEventArgs> PlayerAdded = delegate { };
		public event EventHandler<PlayerEventArgs> PlayerRemoved = delegate { };


		public void AddPlayer(string name)
		{
			// nameの検証はPlayers.AddPlayerで行う。
			// 失敗した場合はArgumentExceptionを返す。
			Players.AddPlayer(name);
			if (!IsRehearsal)
			{
				AddOperationHistory(new AddPlayerCache(this, name));
			}
			this.PlayerAdded(this, new PlayerEventArgs());
		}

		public void RemovePlayer(string name)
		{
			// 失敗した場合はArgumentExceptionを返す。
			Players.RemovePlayer(name);
			if (!IsRehearsal)
			{
				AddOperationHistory(new RemovePlayerCache(this, name));
			}
			this.PlayerRemoved(this, new PlayerEventArgs());

		}

		#endregion


	}
	#endregion

	// (0.0.8)
	#region GameLogFormat列挙体
	public enum GameLogFormat
	{
		/// <summary>
		/// タブ区切りのテキストです。
		/// </summary>
		TSV,
		/// <summary>
		/// aldentea方式です。
		/// </summary>
		Aldentea
	}
	#endregion

}
