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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Aldentea.BaramakiMutus
{

	#region SetCodesDialogクラス
	/// <summary>
	/// SetCodesDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class SetCodesDialog : Window
	{
		// DataContextとして，Questionsプロパティを設定してください．

		#region *コンストラクタ(SetCodesDialog)
		public SetCodesDialog()
		{
			InitializeComponent();
		}
		#endregion

		#region イベントハンドラ

		#region *[閉じる]ボタンクリック時(buttonClose_Click)
		private void buttonClose_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		#endregion

		#region *textBoxCodeでのキー押下時(textBoxCode_KeyDown)
		private void textBoxCode_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Return:
					dataGridQuestions.Focus();
					var cell = new DataGridCellInfo(dataGridQuestions.SelectedItem, dataGridQuestions.Columns[1]);
					dataGridQuestions.CurrentCell = cell;
					e.Handled = true;
					break;
			}
		}
		#endregion

		#region *dataGridQuestionsでのキー押下時(dataGridQuestions_KeyDown)
		private void dataGridQuestions_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.F2:
					textBoxCode.Focus();
					e.Handled = true;
					break;
			}

		}
		#endregion

		#endregion

	}
	#endregion

}
