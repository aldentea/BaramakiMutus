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
	/// <summary>
	/// SetCodesDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class SetCodesDialog : Window
	{
		// DataContextとして，Questionsプロパティを設定してください．

		public SetCodesDialog()
		{
			InitializeComponent();
		}

		private void buttonClose_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

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
	}
}
