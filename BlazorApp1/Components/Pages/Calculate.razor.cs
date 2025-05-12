using MathNet.Symbolics;
using Microsoft.AspNetCore.Components;

namespace BlazorApp1.Components.Pages
{
	public partial class Calculate : ComponentBase
	{


		private string input = "";
		private double result = 0;
		private string display;

		public string Input { get => input; set => input = value; }
		public string Display { get => display; set => display = value; }

		
		


		// метод расчета выражений
		public void Calc()
		{
			var expr = Infix.ParseOrThrow(Input);

			// Создаем словарь значений переменных
			var variables = new Dictionary<string, FloatingPoint> { { ("x"), 1 } };

			// Вычисляем выражение с подстановкой значений
			result = Evaluate.Evaluate(variables, expr).RealValue;

			Console.WriteLine($"Result: {Display}");
		}

		// очистка полей
		public void Clear()
		{
			Display = "";
			result = 0;
		}
	}
}
