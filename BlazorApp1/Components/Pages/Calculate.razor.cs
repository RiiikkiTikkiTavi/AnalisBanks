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

                int n = newValue.Length;

                if (string.IsNullOrEmpty(display)){
                    // оператор не может быть первым
                    if (operators.Contains(newValue[0])) return;

                    // точка не может быть первой
                    if(newValue[0] == '.') return;

                    // Если всё ок — присваиваем
                    display = newValue;

                }
                else
                {
                    // не может быть два оператора подряд
                    if (operators.Contains(display[^1]) && operators.Contains(newValue[^1]))
                        return;

                    // не может быть оператора после точки
                    if (display.EndsWith(".") && operators.Contains(newValue[^1]))
                        return;

                    // точка не может быть после точки и оператора
                    if ((newValue[^1] == '.') && (display.EndsWith(".") || operators.Contains(display[^1])))
                        return;

                    // после параметров может быть только оператор
                    int lastOpenBrace = display.LastIndexOf('{');
                    int lastCloseBrace = display.LastIndexOf('}');
                    if (lastOpenBrace != -1 && lastCloseBrace > lastOpenBrace && lastCloseBrace == display.Length - 1)
                    {
                        // Параметр завершён, можно вводить только оператор
                        if (!operators.Contains(newValue[^1]))
                            return;
                    }



                    // Если всё ок — присваиваем
                    display = newValue;


                }

/*
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
               // display = newValue;

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
