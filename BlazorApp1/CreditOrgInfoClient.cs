/* Класс для работы с загрузкой данных из ЦБ*/

using BlazorApp1.Models;
using global::CregitInfoWS;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
//using static Google.Protobuf.Collections.MapField<TKey, TValue>;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlazorApp1
{
	public class CreditOrgInfoClient
	{
		private readonly CreditOrgInfoSoap _client;


		/*		[Inject]
				private IDbContextFactory<BanksContext> dbFactory { get; set; } = default!;

				public CreditOrgInfoClient()
				{
					var binding = new BasicHttpBinding();
					var endpoint = new EndpointAddress("http://www.cbr.ru/CreditInfoWebServ/CreditOrgInfo.asmx");

					var factory = new ChannelFactory<CreditOrgInfoSoap>(binding, endpoint);
					_client = factory.CreateChannel();
				}

				*/
		
		private readonly IDbContextFactory<BanksContext> dbFactory;
		public CreditOrgInfoClient(CreditOrgInfoSoap client, IDbContextFactory<BanksContext> dbFactory)
		{
			_client = client;
			this.dbFactory = dbFactory;
		}

		private bool rewrite = false;

		//вспомогательный метод - вывод dataset в консоль
		private void ShowDataSet(DataSet dataSet)
		{
			foreach (DataTable table in dataSet.Tables)
			{
				Console.WriteLine($"Таблица: {table.TableName}");

				// Вывод заголовков колонок
				Console.WriteLine(string.Join(" | ", table.Columns.Cast<DataColumn>().Select(col => col.ColumnName)));

				// Разделитель
				Console.WriteLine(new string('-', 50));

				// Вывод всех строк
				foreach (DataRow row in table.Rows)
				{
					Console.WriteLine(string.Join(" | ", row.ItemArray));
				}

				Console.WriteLine(new string('=', 50)); // Разделитель между таблицами
			}
		}


		// получение наименования банка по рег. номеру
		public async Task<string> GetBankNameByRegnum(int regnum)
		{
			double internal_code = await _client.RegNumToIntCodeAsync(regnum);
			if (internal_code == -1) return "";
			XmlNode xmlResponse = await _client.CreditInfoByIntCodeXMLAsync(internal_code);

			if (xmlResponse == null) return "";
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlResponse.OuterXml);

			XmlNode? orgNameNode = xmlDoc.SelectSingleNode("//OrgName");
			return orgNameNode?.InnerText ?? "";
		}

		// получение данных по форме 123 по рег. номеру банка и дате
		public async Task<DataSet> GetData123(int regnum, DateTime dt)
		{
			var response = await _client.Data123FormFullAsync(regnum, dt);

			if (response == null || response.Nodes == null || response.Nodes.Count == 0)
				return null; // Если данные пустые

			// Оборачиваем полученные XML-элементы в общий корневой узел
			XDocument xmlDoc = new XDocument(
				new XDeclaration("1.0", "utf-8", null), // Добавляем заголовок XML
				new XElement("Root", response.Nodes)
			);

			DataSet dataSet = new DataSet();

			using (XmlReader xmlReader = xmlDoc.CreateReader())
			{
				dataSet.ReadXml(xmlReader); // Читаем XML
			}

			ShowDataSet(dataSet);

			return dataSet;
		}

		private async Task LoadData(
			int regnum,
			DateTime dt,
			string codeColumn,
			string valueColumn,
			Func<Task<DataSet>> getDataFunc)
		{
			// Получаем контекст базы данных
			await using var db = dbFactory?.CreateDbContext();
			if (db == null)
			{
				Console.WriteLine("Ошибка: dbFactory == null");
				return;
			}

			// Загружаем данные формы
			var dataSet = await getDataFunc();
			if (dataSet == null) return;

			// Получаем идентификатор расчета (id_info)
			int id_info = await CreateLoad(regnum, dt);

			var dataRecords = new List<DataNor>();        // Список новых записей для сохранения
			var validTnors = new HashSet<int>();          // Список id_tnor, найденных по кодам шаблонов

			// Обход всех таблиц и строк внутри DataSet
			foreach (var table in dataSet.Tables.Cast<DataTable>())
			{
				foreach (DataRow row in table.Rows)
				{
					var code = row[codeColumn]?.ToString();
					if (string.IsNullOrWhiteSpace(code)) continue;

					// Ищем соответствующий шаблон в базе
					var id_tnor = await db.TemplatesNors
										  .Where(t => t.Code == code)
										  .Select(t => (int?)t.IdTnor)
										  .FirstOrDefaultAsync();

					// Пропускаем, если шаблон не найден
					if (id_tnor == null) continue;

					// Пробуем распарсить значение
					if (!decimal.TryParse(row[valueColumn]?.ToString(), out var val)) continue;

					// Добавляем id_tnor в набор для возможного удаления старых записей
					validTnors.Add(id_tnor.Value);

					// Добавляем запись в список
					dataRecords.Add(new DataNor
					{
						IdInfo = id_info,
						IdTnor = id_tnor.Value,
						Val = val
					});
				}
			}

			// Удаление старых данных, если задан rewrite = true
			if (rewrite && validTnors.Any())
			{
				var toDelete = await db.DataNors
									   .Where(d => d.IdInfo == id_info && validTnors.Contains(d.IdTnor ?? -1))
									   .ToListAsync();

				if (toDelete.Any())
				{
					db.DataNors.RemoveRange(toDelete);
					await db.SaveChangesAsync();
				}
			}

			// Сохранение новых данных
			if (dataRecords.Any())
			{
				db.DataNors.AddRange(dataRecords);
				await db.SaveChangesAsync();
			}
		}
		public async Task LoadData123(int regnum, DateTime dt)
		{
			await LoadData(
				regnum,
				dt,
				codeColumn: "CODE",
				valueColumn: "VALUE",
				getDataFunc: () => GetData123(regnum, dt)
			);
		}

		public async Task LoadData135(int regnum, DateTime dt)
		{
			await LoadData(
				regnum,
				dt,
				codeColumn: "C3",
				valueColumn: "V3",
				getDataFunc: () => GetData135(regnum, dt)
			);
		}



		// получение данных по форме 135 по рег. номеру банка и дате
		public async Task<DataSet> GetData135(int regnum, DateTime dt)
		{
			// список колонок, которые необходимо убрать при выводе
			List<string> columnsToDelete = new List<string> { "V3_2", "V3_3" };

			var response = await _client.Data135FormFullAsync(regnum, dt);

			if (response == null || response.Nodes == null || response.Nodes.Count == 0)
				return null; // Если данные пустые

			// Оборачиваем полученные XML-элементы в общий корневой узел
			XDocument xmlDoc = new XDocument(
				new XDeclaration("1.0", "utf-8", null), // Добавляем заголовок XML
				new XElement("Root", response.Nodes)
			);

			DataSet dataSet = new DataSet();

			using (XmlReader xmlReader = xmlDoc.CreateReader())
			{
				dataSet.ReadXml(xmlReader); // Читаем XML
			}

			// убираем пустую инфу, делаем корректный вывод
			foreach (DataTable table in dataSet.Tables.Cast<DataTable>().ToList())
			{
				if (table.Rows.Count == 0) // Проверяем, пустая ли таблица
				{
					dataSet.Tables.Remove(table); // Удаляем таблицу
				}
				else // если таблица не пуста, удаляем лишние столбцы
				{
					foreach (var columnName in columnsToDelete)
					{
						if (table.Columns.Contains(columnName)) // Проверяем, есть ли столбец
						{
							table.Columns.Remove(columnName);
						}
					}
				}
			}

			ShowDataSet(dataSet);

			return dataSet;
		}

		// загрузка данных по 135 форме в базу данных
		/*
		public async Task LoadData135(int regnum, DateTime dt)
		{
			await using var db = dbFactory?.CreateDbContext(); 
			if (db == null)
			{
				Console.WriteLine("Ошибка: dbFactory == null");
				return;
			}
			// загрузить 135 форму в dataset
			var dataSet = await GetData135(regnum, dt);
			// объявление списка записей для сохранения в базу
			var data135Records = new List<DataNor>();
			// если данные есть
			if (dataSet == null) return;

			// получить id расчета
			int id_info = await CreateLoad(regnum, dt);

			foreach (DataTable table in dataSet.Tables)
			{
				foreach (DataRow row in table.Rows)
				{
					var id_tnor = await db.TemplatesNors
										 .Where(t => t.Code == row["C3"])
										 .Select(t => (int?)t.IdTnor)
										 .FirstOrDefaultAsync();

					// Пробуем распарсить значение
					if (!decimal.TryParse(row["V3"]?.ToString(), out var val))
						continue; // Пропустить, если значение V3 отсутствует или не число

					data135Records.Add(new DataNor
					{
						IdInfo = id_info,
						IdTnor = id_tnor,
						Val = val
					});
				}
			}



			db.DataNors.AddRange(data135Records);
			await db.SaveChangesAsync();
		}
		*/

		// получение данных по форме 101 по рег. номеру банка и дате
		public async Task<DataSet> GetData101(int regnum, DateTime dt)
		{
			// список колонок, которые необходимо оставить при выводе
			List<string> columnsToKeep = new List<string> { "pln", "ap", "numsc", "vitg", "iitg", "priz", "Column1" };

			var response = await _client.Data101FNewAsync(regnum, dt);

			if (response == null || response.Nodes == null || response.Nodes.Count == 0)
				return null; // Если данные пустые

			// Оборачиваем полученные XML-элементы в общий корневой узел
			XDocument xmlDoc = new XDocument(
				new XDeclaration("1.0", "utf-8", null), // Добавляем заголовок XML
				new XElement("Root", response.Nodes)
			);

			DataSet dataSet = new DataSet();

			using (XmlReader xmlReader = xmlDoc.CreateReader())
			{
				dataSet.ReadXml(xmlReader); // Читаем XML
			}

			// Удаляем таблицу F1011, если она есть
			if (dataSet.Tables.Contains("F1011"))
			{
				dataSet.Tables.Remove("F1011");
			}

			// убираем пустую инфу, делаем корректный вывод
			foreach (DataTable table in dataSet.Tables)
			{
				var columnsToRemove = table.Columns.Cast<DataColumn>() // преобразование table.Columns в IEnumerable<DataColumn>, чтобы применить LINQ.
										.Select(c => c.ColumnName)  // получение списка имен столбцов
										.Except(columnsToKeep)  // оставляет только те элементы, которые не входят в columnsToKeep
										.ToList();

				foreach (string columnName in columnsToRemove)
				{
					table.Columns.Remove(columnName); // удаление лишних колонок
				}
			}

			ShowDataSet(dataSet);

			return dataSet;
        }

		// загрузка данных по 101 форме в базу данных
		public async Task LoadData101(int regnum, DateTime dt)
		{
			await using var db = dbFactory?.CreateDbContext(); 
			if (db == null)
			{
				Console.WriteLine("Ошибка: dbFactory == null");
				return;
			}
			// загрузить 101 форму в dataset
			var dataSet = await GetData101(regnum, dt);
			// объявление списка записей для сохранения в базу
            var data101Records = new List<Data101>();
            // если данные есть
            if (dataSet == null) return;

            // получить словарь соответсвий (ap, numsc) - id_t101 шаблона 101 формы
            var id_t101s = await FindTemplateId101(dataSet);
			// получить id расчета
			int id_info = await CreateLoad(regnum, dt);

			// удалить старые данные
			if (rewrite)
			{
				var existingRecords = await db.Data101s
											  .Where(d => d.IdInfo == id_info)
											  .ToListAsync();
				db.Data101s.RemoveRange(existingRecords);
				await db.SaveChangesAsync();
			}

			foreach (DataTable table in dataSet.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    string? ap = row["ap"]?.ToString();
                    string? numsc = row["numsc"]?.ToString();
                    string? pln = row["pln"]?.ToString();

                    if (string.IsNullOrWhiteSpace(ap) || string.IsNullOrWhiteSpace(numsc))
                        continue;

                    // попытка найти значение по ключу в словаре id_t101s
                    if (!id_t101s.TryGetValue((ap, numsc, pln), out int id_t101))
                        continue; // если шаблон не найден — пропускаем

                    // Парсим значения
                    decimal.TryParse(row["vitg"]?.ToString(), out var vint);
                    decimal.TryParse(row["iitg"]?.ToString(), out var iitg);

                    // список объектов типа Data101, которые хотим добавить в базу
                    // добавляем новый объект в список, чтобы потом все записи массово сохранить в базу.
                    data101Records.Add(new Data101
                    {
                        IdInfo = id_info,
                        IdT101 = id_t101,
                        Vint = vint,
                        Iitg = iitg
                    });
                }
            }

            db.Data101s.AddRange(data101Records);
            await db.SaveChangesAsync();
        }


		// получить id шаблона 101 формы по счету и его типу (А/П)
		// возвращает словарь соответсвий (ap, numsc, pln) - id_t101
		public async Task<Dictionary<(string ap, string numsc, string pln), int>> FindTemplateId101(DataSet dataSet)
		{
			// Сбор уникальных троек (ap, numsc, pln) из DataSet
			var keys = new HashSet<(string ap, string numsc, string pln)>();

			await using var db = dbFactory.CreateDbContext();

			foreach (DataTable table in dataSet.Tables)
			{
				foreach (DataRow row in table.Rows)
				{
					string? ap = row["ap"]?.ToString();
					string? numsc = row["numsc"]?.ToString();
					string? pln = row["pln"]?.ToString();

					if (!string.IsNullOrWhiteSpace(ap) &&
						!string.IsNullOrWhiteSpace(numsc) &&
						!string.IsNullOrWhiteSpace(pln))
					{
						keys.Add((ap, numsc, pln));
					}
				}
			}

			// Загружаем шаблоны из БД
			var allTemplates = await db.Templates101s.ToListAsync();

			// Фильтруем по тройке (ap, name, plan)
			var matched = allTemplates
				.Where(t => keys.Contains((t.AP, t.Name, t.Plan.ToString())))
				.ToDictionary(t => (t.AP, t.Name, t.Plan.ToString()), t => t.IdT101);

			return matched;
		}


		// создание загрузки данных форм, заполнение банка и даты
		public async Task<int> CreateLoad(int regnum, DateTime dt)
		{
            await using var db = dbFactory.CreateDbContext();
            var dateOnly = DateOnly.FromDateTime(dt);

            // Поиск существующей записи
            var existing = await db.FormInfos
                .FirstOrDefaultAsync(x => x.Regnum == regnum && x.Dt == dateOnly);

			if (existing != null) 
			{ 
				rewrite = true; // стереть и записать данные заново

				return existing.IdInfo; 
			}

            // Создание новой записи
            var record = new FormInfo
			{
				Regnum = regnum,
				Dt = DateOnly.FromDateTime(dt)
			}; 

            db.FormInfos.Add(record);
            await db.SaveChangesAsync();

			// возврат id созданной записи
			return record.IdInfo;
        }

      }
}


