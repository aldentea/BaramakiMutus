using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Threading.Tasks;
using System.Globalization;

namespace Aldentea.BaramakiMutus
{
	using Data;


	#region MaskAnswerConverterクラス
	public class MaskAnswerConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values[0] is BaramakiQuestion)
			{
				var question = (BaramakiQuestion)values[0];
				var mode = (MainWindow.Mode)values[1];

				if (mode == MainWindow.Mode.Judged || mode == MainWindow.Mode.Waiting)
				{
					switch ((string)parameter)
					{
						case "title":
							return question.Title;
						case "artist":
							return question.Artist;
						default:
							return question.Title;
					}
				}
			}
			else if (values[0] is HazureQuestion)
			{
				var mode = (MainWindow.Mode)values[1];
				if (mode == MainWindow.Mode.Playing || mode == MainWindow.Mode.Judged || mode == MainWindow.Mode.Waiting)
				{
					return "*ハズレ*";
				}

			}
			return "？？？";
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	#endregion


	#region CodedQuestionConverterクラス
	public class CodedQuestionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is BaramakiQuestion)
			{
				var question = (BaramakiQuestion)value;
				return $"{question.Title} / {question.Artist}";
			}
			else if (value is HazureQuestion)
			{
				return "*ハズレ*";
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	#endregion

}
