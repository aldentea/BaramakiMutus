using System.Windows.Input;

namespace Aldentea.BaramakiMutus
{

	#region [static]Commandsクラス
	public static class Commands
	{
		/// <summary>
		/// 問題コードを設定するコマンドです．
		/// 実行すると，それ用のダイアログを開きます．
		/// </summary>
		public static RoutedCommand SetCodesCommand = new RoutedCommand();

		/// <summary>
		/// ゲームを開始するコマンドです．
		/// </summary>
		public static RoutedCommand StartGameCommand = new RoutedCommand();

		/// <summary>
		/// ゲームを開始するコマンドです．
		/// </summary>
		public static RoutedCommand EndGameCommand = new RoutedCommand();

		/// <summary>
		/// 出題を開始するコマンドです．
		/// 問題のコードをパラメータに与えます．
		/// </summary>
		public static RoutedCommand StartCommand = new RoutedCommand();

		/// <summary>
		/// 判定を行うコマンドです．'○'か'×'かをパラメータに与えます．
		/// </summary>
		public static RoutedCommand JudgeCommand = new RoutedCommand();

		public static RoutedCommand UndoJudgementCommand = new RoutedCommand();

		/// <summary>
		/// 「スルー」扱いにするコマンドです．この形式では，よほどのことがない限り使わないはずです
		/// (該当の曲ファイルは存在するけど音声が入っていなかったとき，とか？)．
		/// </summary>
		public static RoutedCommand ThroughCommand = new RoutedCommand();

	}
	#endregion

}
