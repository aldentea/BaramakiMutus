using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace Aldentea.BaramakiMutus
{
	using Data;

	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Aldentea.Wpf.Application.BasicWindow, INotifyPropertyChanged
	{


		#region *MyDocumentプロパティ
		protected BaramakiMutusDocument MyDocument
		{
			get { return (BaramakiMutusDocument)App.Current.Document; }
		}
		#endregion

		#region *MySettingsプロパティ
		Properties.Settings MySettings
		{
			get
			{
				return App.Current.MySettings;
			}
		}
		#endregion



		public MainWindow()
		{
			InitializeComponent();

			this.FileHistoryShortcutParent = menuItemHistory;

		}


		#region *ウィンドウ初期化時(MainWindow_Initialized)
		private void MainWindow_Initialized(object sender, EventArgs e)
		{
			// 窓の位置やサイズを復元。
			if (MySettings.MainWindowMaximized)
			{
				this.WindowState = System.Windows.WindowState.Maximized;
			}
			if (MySettings.MainWindowRect.Size != new Size(0, 0))
			{
				this.Left = MySettings.MainWindowRect.X;
				this.Top = MySettings.MainWindowRect.Y;
				this.Width = MySettings.MainWindowRect.Width;
				this.Height = MySettings.MainWindowRect.Height;
			}
			// 音量を復元。
			this.MySongPlayer.Volume = MySettings.SongPlayerVolume;
		}
		#endregion

		#region *ウィンドウクローズ時(MainWindow_Closed)
		private void MainWindow_Closed(object sender, EventArgs e)
		{
			// 窓の位置やサイズを保存。
			MySettings.MainWindowMaximized = this.WindowState == System.Windows.WindowState.Maximized;
			MySettings.MainWindowRect = new Rect(this.Left, this.Top, this.Width, this.Height);
			// 音量を保存。
			MySettings.SongPlayerVolume = this.MySongPlayer.Volume;

		}
		#endregion


		private void menuItemClose_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}


		#region *CurrentModeプロパティ
		public Mode CurrentMode
		{
			get
			{
				return _mode;
			}
			set
			{
				if (CurrentMode != value)
				{
					_mode = value;
					NotifyPropertyChanged("CurrentMode");
				}
			}
		}
		Mode _mode = Mode.Standby;
		public enum Mode
		{
			Standby,
			Waiting,
			Playing,
			Judged
		}
		#endregion

		#region *CurrentJudgementプロパティ
		Judgement? CurrentJudgement
		{
			get
			{
				return _judgement;
			}
			set
			{
				if (CurrentJudgement != value)
				{
					_judgement = value;
					//NotifyPropertyChanged("CurrentMode");
				}
			}

		}
		Judgement? _judgement = null;


		enum Judgement
		{
			/// <summary>
			/// 正解です。
			/// </summary>
			Correct,
			/// <summary>
			/// 不正解です。
			/// </summary>
			Mistake,
			/// <summary>
			/// ハズレです。
			/// </summary>
			Hazure
		}
		#endregion

		#region *CurrentPlayerプロパティ
		GrandMutus.Data.Player CurrentPlayer
		{
			get
			{
				return dataGridPlayers.SelectedItem as GrandMutus.Data.Player;
			}
		}
		private void dataGridPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (CurrentPlayer != null && CurrentJudgement.HasValue)
			{
				ProcessScore();
			}
		}
		#endregion
		
		// ※とりあえず．
		#region *CurrentQuestionプロパティ
		public ICodedQuestion CurrentQuestion
		{
			get
			{
				return _currentQuestion;
			}
			set
			{
				if (CurrentQuestion != value)
				{
					this._currentQuestion = value;
					NotifyPropertyChanged("CurrentQuestion");
				}
			}
		}
		ICodedQuestion _currentQuestion = null;
		#endregion
		




		#region 曲再生関連

		//protected MediaPlayer _mPlayer = new MediaPlayer();     // とりあえずprotectedにしておく．

		//bool songPlaying = false;

		#region *MySongPlayerプロパティ
		public GrandMutus.Base.SongPlayer MySongPlayer
		{
			get
			{
				return _songPlayer;
			}
		}
		GrandMutus.Base.SongPlayer _songPlayer = new GrandMutus.Base.SongPlayer();
		#endregion

		/*
		// 08/27/2013 by aldentea : IntroMutusからIntroRunperialにコピー．
		// 08/13/2012 by aldentea
		#region *Volumeプロパティ
		/// <summary>
		/// 音量を取得／設定します(最小0、最大1、線形スケール)。
		/// </summary>
		public double Volume
		{
			get
			{
				return MySongPlayer.Volume;
			}
			set
			{
				if (Volume != value)
				{
					MySongPlayer.Volume = value;
					NotifyPropertyChanged("Volume");
				}
			}
		}
		#endregion
	*/

		#region TogglePlayPauseコマンドハンドラ

		// 自前でSwitchPlayPauseコマンドを用意したけど，
		// System.Windows.Input.MediaCommands.TogglePlayPauseコマンドとまるっきり同じだったorz

		void SwitchPlayPause_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (MySongPlayer.IsActive)
			{
				MySongPlayer.TogglePlayPause();
			}
		}

		void SwitchPlayPause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = MySongPlayer.IsActive;
		}

		#endregion


		#endregion


		#region 編集関連

		#region SetCodesコマンドハンドラ

		private void SetCodes_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var dialog = new SetCodesDialog();
			dialog.DataContext = MyDocument.Questions;  // データバインディングは不要．
			dialog.ShowDialog();
		}

		private void SetCodes_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CurrentMode == Mode.Standby;
		}

		#endregion


		#endregion


		#region 進行関連


		#region StartGameコマンドハンドラ
		private void StartGame_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			CurrentMode = Mode.Waiting;
			textBoxQuestionCode.Focus();
		}

		void StartGame_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CurrentMode == Mode.Standby;
		}

		#endregion

		#region EndGameコマンドハンドラ

		private void EndGame_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (MessageBox.Show("終了していいですか？", "確認", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				if (MySongPlayer.IsActive)
				{
					MySongPlayer.Close();
				}
				CurrentMode = Mode.Standby;
			}

		}

		private void EndGame_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CurrentMode == Mode.Waiting;
		}

		#endregion
	

		#endregion


		#region 出題関連


		#region Startコマンドハンドラ

		private void Start_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			string code = (string)e.Parameter;
			// codeから問題を取得する．
			ICodedQuestion question = MyDocument.GetQuestionByCode(code);

			if (question == null)
			{
				// 例外を投げた方がいいのか？
				MessageBox.Show("問題コードが不適切です．もう一度よーく確かめよう！");
			}
			else if (question is HazureQuestion)
			{
				// ハズレ処理

				CurrentMode = Mode.Playing;
				CurrentQuestion = question;

				MyDocument.AddOrder(question.ID);

				// フォーカスをプレイヤーテーブルに移す．
				FocusOnPlayersTable();
			}
			else if (question is BaramakiQuestion)
			{
				// ※Logにorderを追加．
				// ※CurrentQuestionに設定．
				// ※再生開始．

				var b_question = (BaramakiQuestion)question;

				try
				{
					MySongPlayer.Open(b_question.FileName);
				}
				catch
				{
					MessageBox.Show("どういうわけかファイルを開けませんでした．ハズレ扱いにさせてください．");
					return;
				}
				CurrentMode = Mode.Playing;
				CurrentQuestion = question;

				MyDocument.AddOrder(question.ID);

				MySongPlayer.CurrentPosition = b_question.PlayPos;
				MySongPlayer.Play();

				// フォーカスをプレイヤーテーブルに移す．
				FocusOnPlayersTable();
			}
		}

		private void FocusOnPlayersTable()
		{
			dataGridPlayers.Focus();
			dataGridPlayers.SelectedIndex = 0;
			DataGridCellInfo cellInfo = new DataGridCellInfo(dataGridPlayers.Items[0], dataGridPlayers.Columns[0]);
			dataGridPlayers.CurrentCell = cellInfo;
			// http://gacken.com/wp/program/wpf/570/ から拝借．
			// 意外と面倒(^^;
		}

		private void Start_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CurrentMode == Mode.Waiting && !string.IsNullOrEmpty((string)e.Parameter);
		}

		#endregion

		#region Judgeコマンドハンドラ

		private void Judge_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			switch ((string)e.Parameter)
			{
				case "○":
					CurrentJudgement = CurrentQuestion is HazureQuestion ? Judgement.Hazure : Judgement.Correct;
					break;
				case "×":
					CurrentJudgement = CurrentQuestion is HazureQuestion ? Judgement.Hazure : Judgement.Mistake;
					break;
				default:
					CurrentJudgement = null;
					break;
			}


			if (CurrentPlayer != null && CurrentJudgement.HasValue)
			{
				// 得点処理．
				ProcessScore();

			}
			else
			{
				CurrentMode = Mode.Judged;
			}

			if (CurrentQuestion is BaramakiQuestion)
			{
				// サビ再生
				var question = (BaramakiQuestion)CurrentQuestion;
				MySongPlayer.CurrentPosition = question.SabiPos;
				MySongPlayer.Play();
			}
		}

		private void Judge_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			// 判定のやり直しを考慮．
			e.CanExecute = (CurrentMode == Mode.Playing) || (CurrentMode == Mode.Judged);
		}

		#endregion

		#region *得点の処理を行う(ProcessScore)
		protected void ProcessScore()
		{

			//categoryScoreCache = publishedCategories[category];

			switch (CurrentJudgement)
			{
				case Judgement.Correct:
					// 得点を加算．
					MyDocument.AddLog(CurrentPlayer.ID, "○", CurrentGain);
					if (CurrentPlayer.Score >= _kachinuke)
					{
						// ※勝ち抜け処理．
						MessageBox.Show(string.Format("{0} さんは勝ち抜けです！", CurrentPlayer.Name), "勝ち抜け！");
					}
					break;
				case Judgement.Mistake:
					MyDocument.AddLog(CurrentPlayer.ID, "×", 0);
					break;
				case Judgement.Hazure:
					MyDocument.AddLog(CurrentPlayer.ID, "■", 0);
					CurrentGain += 0.2M;
					MyDocument.AddLog("＋", CurrentGain);
					break;
				default:
					return;
			}


			EndQuestion();

		}
		#endregion

		/// <summary>
		/// 現在の1問正解時の得点を取得／設定します。
		/// </summary>
		public decimal CurrentGain
		{
			get
			{
				return _currentGain;
			}
			set
			{
				if (CurrentGain != value)
				{
					_currentGain = value;
					NotifyPropertyChanged("CurrentGain");
				}
			}
		}
		decimal _currentGain = 2;

		decimal _kachinuke = 10;

		/// <summary>
		/// 出題の終了処理をします．
		/// </summary>
		protected void EndQuestion()
		{
			//CurrentQuestion = null;
			CurrentJudgement = null;
			dataGridPlayers.SelectedItem = null;
			textBoxQuestionCode.Clear();
			textBoxQuestionCode.Focus();
			CurrentMode = Mode.Waiting;
		}

		#region UndoJudgementコマンドハンドラ

		//decimal categoryScoreCache = 0.0M;

		private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MyDocument.Undo();
			CurrentMode = Mode.Judged;
			FocusOnPlayersTable();
		}

		private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (CurrentMode == Mode.Waiting && MyDocument.CanUndo);
		}

		#endregion


		#region Throughコマンドハンドラ

		private void Through_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			EndQuestion();
		}

		private void Through_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CurrentMode == Mode.Playing || CurrentMode == Mode.Judged;
		}

		#endregion


		#endregion


		protected void NotifyPropertyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

	}
}
