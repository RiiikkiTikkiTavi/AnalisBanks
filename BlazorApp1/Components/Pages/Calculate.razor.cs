using Microsoft.AspNetCore.Components;

namespace BlazorApp1.Components.Pages
{
	public partial class Calculate : ComponentBase
	{

		private string time = "";
		private string val = "";

		private int input = 0;
		private double result = 0;
		private string operation = "";
		private double firstNumber;
		private string display = "";

		public string Time { get => time; set => time = value; }
		public string Val { get => val; set => val = value; }
		public string Display { get => display; set => display = value; }
		public int Input { get => input; set => input = value; }

		public void ShowTime()
		{
			Time = DateTime.Now.ToString("D");

		}

		public void SetOperation(string op)
		{

			operation = op;
			Display += operation;
		}

		public void SetInput(int num)
		{
			Display += num.ToString();
			Input = 0;

		}
		public void Calcul()
		{
			if (/*!string.IsNullOrEmpty(Input) && */!string.IsNullOrEmpty(operation))
			{
				double secondNumber = 0;// = double.Parse(Input);
				switch (operation)
				{
					case "+": result = firstNumber + secondNumber; break;
					case "-": result = firstNumber - secondNumber; break;
					case "*": result = firstNumber * secondNumber; break;
					case "/": result = secondNumber != 0 ? firstNumber / secondNumber : 0; break;

				}
				//Input = result.ToString();
				operation = "";
			}
		}

		public void Clear()
		{
			Input = 0;
			result = 0;
			operation = "";
			Display = "";
		}


	

}
}
