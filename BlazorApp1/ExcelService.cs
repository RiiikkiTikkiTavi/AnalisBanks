using BlazorApp1.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlazorApp1
{
	public class ExcelService
	{
		private readonly IDbContextFactory<BanksContext> dbFactory;

		//конструктор с передачей фабрики контекста базы данных
		public ExcelService(IDbContextFactory<BanksContext> dbFactory)
		{
			this.dbFactory = dbFactory;
		}

		// возвращает массив байтов, содержащий Excel-фай
		public async Task<byte[]> GenerateExcelAsync(DateOnly dt, int regnum, int methodId)
		{
			//Создаётся новый экземпляр BanksContext
			//await using гарантирует, что context будет автоматически освобождён
			await using var db = await dbFactory.CreateDbContextAsync();

			// формирование SQL-подобный LINQ-запрос
			var query = from fi in db.FormInfos
						where fi.Dt == dt && fi.Regnum == regnum
						join bnk in db.Banks on fi.Regnum equals bnk.Regnum
						join mr in db.MethodsResults on fi.IdInfo equals mr.IdInfo
						join arg in db.Arguments on mr.IdArg equals arg.IdArg
						where arg.IdMethods == methodId
						join m in db.Methods on arg.IdMethods equals m.IdMethods
						select new
						{
							fi.Regnum,
							fi.Dt,
							bnk.Name,
							MethodName = m.Name,
							MethodDescr = m.Descr,
							ArgumentName = arg.Name,
							ArgumentShortName = arg.ShortName,
							mr.Val
						};

			// выполнение запроса и сохранение в список
			var data = await query.ToListAsync();

			// Используем первую строку для заголовков
			var first = data.First(); 

			// Создаётся Excel-файл с именем Results
			using var workbook = new XLWorkbook();
			var worksheet = workbook.Worksheets.Add("Results");

			// Заголовок — название метода
			worksheet.Cell(1, 1).Value = first.MethodName;
			worksheet.Range(1, 1, 1, 3).Merge().Style.Font.SetBold().Font.FontSize = 16;

			// Описание метода
			worksheet.Cell(2, 1).Value = first.MethodDescr;
			worksheet.Range(2, 1, 2, 7).Merge().Style.Font.FontSize = 12;

			// Информация: Имя (Name) и Дата (Dt)
			worksheet.Cell(3, 1).Value = first.Name;
			worksheet.Cell(3, 7).Value = first.Dt?.ToDateTime(TimeOnly.MinValue).ToString("dd.MM.yyyy");

			// Заголовки таблицы
			worksheet.Cell(5, 1).Value = "Показатель";
			worksheet.Cell(5, 2).Value = "Краткое имя";
			worksheet.Cell(5, 3).Value = "Значение";


			// запись данных
			for (int i = 0; i < data.Count; i++)
			{ 
					var row = data[i];
					worksheet.Cell(i + 6, 1).Value = row.ArgumentName;
					worksheet.Cell(i + 6, 2).Value = row.ArgumentShortName;
					worksheet.Cell(i + 6, 3).Value = row.Val;
			}

			// Автоширина
			worksheet.Columns().AdjustToContents();

			// Файл сохраняется в MemoryStream
			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			return stream.ToArray();
		}
	}
}
