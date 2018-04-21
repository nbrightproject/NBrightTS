using System.Collections.Generic;
using System.Data;
using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.Providers;
using NBrightDNNv2;

namespace NBrightDNNv2.SqlDataProvider
{

	/// -----------------------------------------------------------------------------
	/// <summary>
	/// An abstract class for the data access layer
	/// </summary>
	/// -----------------------------------------------------------------------------
	public abstract class DataProvider
	{

		#region Shared/Static Methods

		private static DataProvider provider;

		// return the provider
		public static DataProvider Instance()
		{
			if (provider == null)
			{
                const string assembly = "NBrightDNNv2.SqlDataprovider.SqlDataprovider,NBrightDNNv2";
				Type objectType = Type.GetType(assembly, true, true);

				provider = (DataProvider)Activator.CreateInstance(objectType);
				DataCache.SetCache(objectType.FullName, provider);
			}

			return provider;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not returning class state information")]
		public static IDbConnection GetConnection()
		{
			const string providerType = "data";
			ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(providerType);

			Provider objProvider = ((Provider)_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);
			string _connectionString;
			if (!String.IsNullOrEmpty(objProvider.Attributes["connectionStringName"]) && !String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings[objProvider.Attributes["connectionStringName"]]))
			{
				_connectionString = System.Configuration.ConfigurationManager.AppSettings[objProvider.Attributes["connectionStringName"]];
			}
			else
			{
				_connectionString = objProvider.Attributes["connectionString"];
			}

			IDbConnection newConnection = new System.Data.SqlClient.SqlConnection();
			newConnection.ConnectionString = _connectionString.ToString();
			newConnection.Open();
			return newConnection;
		}

		#endregion


		#region "NBrightBuy Abstract Methods"

        public abstract IDataReader GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string lang = "");
        public abstract int GetListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "");
        public abstract IDataReader Get(int itemId, string lang = "");
        public abstract int Update(int ItemId, int PortalId, int ModuleId, String TypeCode, String XMLData, String GUIDKey, DateTime ModifiedDate, String TextData, int XrefItemId, int ParentItemId, int UserId, string lang);
        public abstract void Delete(int itemId);
        public abstract void CleanData();
        public abstract IDataReader GetData(int itemId);
        public abstract IDataReader GetDataLang(int parentitemId,String lang);
        
		#endregion


	}

}