using Google.Protobuf.WellKnownTypes;
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

        public string Display
        {
            get => display;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    display = "";
                    return;
                }

                char[] operators = ['+', '-', '*', '/', '%'];

                string newValue = value.Trim();

                // Если первый символ — оператор, не устанавливаем
                if (string.IsNullOrEmpty(display) && operators.Contains(newValue[0]))
                    return;

                // Запрещаем начинать с точки
                if (string.IsNullOrEmpty(display) && newValue[0] == '.')
                    return;

                // Проверка на два оператора подряд
                if (!string.IsNullOrEmpty(display) &&
                    operators.Contains(display[^1]) &&
                    operators.Contains(newValue[^1]))
                    return;

                // Запрещаем точку сразу после оператора или двойную точку
                if (newValue[^1] == '.')
                {
                    if (display.EndsWith(".") || (!string.IsNullOrEmpty(display) && operators.Contains(display[^1])))
                        return;
                }
                /*
                // Запрещаем оператор сразу после точки
                if (display.EndsWith(".") && operators.Contains(newValue[0]))
                    return;

                */

                // Запрещаем точку, если в числе она уже была

                // Если всё ок — присваиваем
                display = newValue;

            }
        }





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
