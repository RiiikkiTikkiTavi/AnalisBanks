using BlazorApp1.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

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
		public async Task<byte[]> GenerateExcelAsync()
		{
			//Создаётся новый экземпляр BanksContext
			//await using гарантирует, что context будет автоматически освобождён
			await using var db = await dbFactory.CreateDbContextAsync();

			// формирование SQL-подобный LINQ-запрос
			var query = from fi in db.FormInfos
						join mr in db.MethodsResults on fi.IdInfo equals mr.IdInfo
						join arg in db.Arguments on mr.IdArg equals arg.IdArg
						join m in db.Methods on arg.IdMethods equals m.IdMethods
						select new
						{
							fi.Regnum,
							fi.Dt,
							MethodName = m.Name,
							MethodDescr = m.Descr,
							ArgumentName = arg.Name,
							ArgumentShortName = arg.ShortName,
							mr.Val
						};

			// выполнение запроса и сохранение в список
			var data = await query.ToListAsync();

			// Создаётся Excel-файл с именем Results
			using var workbook = new XLWorkbook();
			var worksheet = workbook.Worksheets.Add("Results");

			// заголовки таблицы excel
			worksheet.Cell(1, 1).Value = "Regnum";
			worksheet.Cell(1, 2).Value = "Date";
			worksheet.Cell(1, 3).Value = "Method Name";
			worksheet.Cell(1, 4).Value = "Method Description";
			worksheet.Cell(1, 5).Value = "Argument Name";
			worksheet.Cell(1, 6).Value = "Argument Short Name";
			worksheet.Cell(1, 7).Value = "Value";

			// запись данных
			for (int i = 0; i < data.Count; i++)
			{
				var row = data[i];
				worksheet.Cell(i + 2, 1).Value = row.Regnum;
				worksheet.Cell(i + 2, 2).Value = row.Dt?.ToString("dd.MM.yyyy");
				worksheet.Cell(i + 2, 3).Value = row.MethodName;
				worksheet.Cell(i + 2, 4).Value = row.MethodDescr;
				worksheet.Cell(i + 2, 5).Value = row.ArgumentName;
				worksheet.Cell(i + 2, 6).Value = row.ArgumentShortName;
				worksheet.Cell(i + 2, 7).Value = row.Val;
			}

			// Файл сохраняется в MemoryStream
			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			return stream.ToArray();
		}
	}
}
