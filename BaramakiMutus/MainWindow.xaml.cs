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
			Correct,
			Mistake
		}
		#endregion

		#region *CurrentPlayerプロパティ
		/*
		GrandMutus.Data.Player CurrentPlayer
		{
			get
			{
				return dataGridPlayers.SelectedItem as Player;
			}
		}
		*/
		private void dataGridPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			/*
			if (CurrentPlayer != null && CurrentJudgement.HasValue)
			{
				ProcessScore();
			}
			*/
		}
		#endregion
		/*
				// ※とりあえず．
				#region *CurrentQuestionプロパティ
				public BaramakiIntroQuestion CurrentQuestion
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
				BaramakiIntroQuestion _currentQuestion = null;
				#endregion
			*/




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



		protected void NotifyPropertyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

	}
}
