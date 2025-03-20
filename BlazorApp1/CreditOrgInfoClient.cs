using global::CregitInfoWS;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Data;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace BlazorApp1
{

	public class CreditOrgInfoClient
	{
		private readonly CreditOrgInfoSoap _client;

		public CreditOrgInfoClient()
		{
			var binding = new BasicHttpBinding();
			var endpoint = new EndpointAddress("http://www.cbr.ru/CreditInfoWebServ/CreditOrgInfo.asmx");

			var factory = new ChannelFactory<CreditOrgInfoSoap>(binding, endpoint);
			_client = factory.CreateChannel();
		}


		//вспомогательный метод - вывод dataset в консоль
		private void ShowDataSet(DataSet dataSet)
		{
			/*foreach (DataTable table in dataSet.Tables)
			{
				Console.WriteLine($"Таблица: {table.TableName}");
				foreach (DataColumn column in table.Columns)
				{
					Console.WriteLine(string.Join(" | ", column.ColumnName));
				}

				// Вывод заголовков колонок
				Console.WriteLine(new string('-', 50)); // разделитель
				foreach (DataRow row in table.Rows)
				{
					Console.WriteLine(string.Join(" | ", row.ItemArray)); // строки
				}
				foreach (DataRow row in dataSet.Tables[0].Rows.Cast<DataRow>())
				{
					foreach (var item in row.ItemArray)
					{
						Console.WriteLine(string.Join(" | ", item));
					}
				}

			}*/
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
		
		
	}
}


