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
			XDocument xmlDoc = new XDocument(new XElement("Root", response.Nodes));

			// Создаем DataSet и загружаем в него XML
			DataSet dataSet = new DataSet();
			using (var reader = xmlDoc.CreateReader())
			{
				dataSet.ReadXml(reader);
			}

			return dataSet;
		}
	}
	}


