using System;
using System.Data;
using Microsoft.AspNetCore.Components;


namespace BlazorApp1.Components.Pages
{
	public partial class FormsData : ComponentBase
	{	

		// можно добавить фильтрацию-поиск по колонкам
		// https://blazor.syncfusion.com/documentation/datagrid/filtering

		private int regnum = 1000;
		private string bankName;
		//private DateTime onDate = DateTime.Today;
		private DateTime onDate = new DateTime(2025, 1, 1); // год - месяц - день;
		DataSet ds;
		private bool isLoading = false;
		private bool showNulls = false;
		private bool ShowToast = false;
		private string ToastMessage = "";

		private int selectedForm = 135; // ID формы по умолчанию
		private List<int> formNumber = new() { 123, 135, 101 }; // Пример списка форм

		private CreditOrgInfoClient _creditService;

		public int Regnum { get => regnum; set => regnum = value; }
		public string BankName { get => bankName; set => bankName = value; }
		public DateTime OnDate { get => onDate; set => onDate = value; }
		public DataSet Ds { get => ds; set => ds = value; }
		public bool IsLoading { get => isLoading; set => isLoading = value; }
		public bool ShowNulls { get => showNulls; set => showNulls = value; }
		public int SelectedForm { get => selectedForm; set => selectedForm = value; }
		public List<int> FormNumber { get => formNumber; set => formNumber = value; }

		[Inject]
		public CreditOrgInfoClient CreditService { get => _creditService; set => _creditService = value; }
		

		private async Task LoadData2()
		{

			BankName = await CreditService.GetBankNameByRegnum(Regnum);
			Console.WriteLine($"Name: {BankName}");

		}

		// загрузка данных в зависимости от выбранной формы, банка и даты
		public async Task LoadData()
		{

			IsLoading = true; // Показываем "Загрузка данных..."
			StateHasChanged(); // Принудительно обновляем UI
			switch (SelectedForm)
			{
				case 123:
					Ds = await CreditService.GetData123(Regnum, OnDate);
					Console.WriteLine("form 123");
					break;
				case 135:
					Ds = await CreditService.GetData135(Regnum, OnDate);
					Console.WriteLine("form 135");
					break;
				case 101:
					Ds = await CreditService.GetData101(Regnum, OnDate);
					Console.WriteLine("form 101");
					break;

			}

			IsLoading = false; // Данные загружены, скрываем "Загрузка данных..."
			StateHasChanged(); // Обновляем UI снова

		}

		public async Task SaveData()
		{

			IsLoading = true; // Показываем "Загрузка данных..."
			StateHasChanged(); // Принудительно обновляем UI
			switch (SelectedForm)
			{
				case 123:
					await CreditService.LoadData123(Regnum, OnDate);
					ToastMessage = "Данные 123 формы загружены";
					Console.WriteLine("load 123");
					break;
				case 135:
					await CreditService.LoadData135(Regnum, OnDate);
					ToastMessage = "Данные 135 формы загружены";
					Console.WriteLine("load 135");
					break;
				case 101:
					await CreditService.LoadData101(Regnum, OnDate);
					ToastMessage = "Данные 101 формы загружены";
					Console.WriteLine("load 101");
					break;

			}

			IsLoading = false; // Данные загружены, скрываем "Загрузка данных..."
			ShowToast = true;
			StateHasChanged();

			// Автоматически скрыть через 3 секунды
			await Task.Delay(3000);
			ShowToast = false;
			
			StateHasChanged(); // Обновляем UI снова

		}

		// проверить содержит ли строка null-значения
		public bool ContainsNull(DataRow row)
		{
			return row.ItemArray.Any(item => item == DBNull.Value || item == null);
		}
	}
}
