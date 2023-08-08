using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Lawfirm.Foundation.SitecoreExtensions.Entities;

namespace Lawfirm.Foundation.SitecoreExtensions.Services
{
	public class ItemEntitiesService : IItemEntitiesService
	{
		private string ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings[Sitecore.Context.ContentDatabase?.ConnectionStringName]?.ConnectionString ?? ConfigurationManager.ConnectionStrings[Database.GetDatabase("master").ConnectionStringName]?.ConnectionString;
			}
		}

		public ItemEntity[] GetChildren(Item parentItem, Language language = null)
		{
			if (parentItem == null)
			{
				return Array.Empty<ItemEntity>();
			}

			return this.GetChildren(parentItem.ID, language);
		}

		public ItemEntity[] GetChildren(ID parentId, Language language = null)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			Sitecore.Diagnostics.Log.Debug($"[ItemEntitiesService] Getting children items for {parentId} ({language}) was started.", this);

			if (language == null)
			{
				language = Sitecore.Context.ContentLanguage;
			}

			var entities = new List<ItemEntity>();

			var query = $"SELECT [Items].[ID], [Items].[Name], [Items].[TemplateID], [Items].[ParentID], [UnversionedFields].[Value] From [Items] " +
				$"LEFT JOIN [UnversionedFields] ON [Items].[ID] = [UnversionedFields].[ItemId] AND [UnversionedFields].[FieldId] = 'B5E02AD9-D56F-4C41-A065-A133DB87BDEB' AND [UnversionedFields].[Language] = '{language}' " +
				$"WHERE ParentID = '{parentId}'";

			using (var connection = new SqlConnection(this.ConnectionString))
			{
				var command = new SqlCommand(query, connection);
				connection.Open();

				var dataReader = command.ExecuteReader();

				try
				{
					while (dataReader.Read())
					{
						var itemEntity = new ItemEntity()
						{
							ID = ID.Parse(dataReader[0]),
							Name = dataReader[1] as string,
							TemplateID = ID.Parse(dataReader[2]),
							ParentID = ID.Parse(dataReader[3]),
							DisplayName = dataReader[4] as string,
						};

						entities.Add(itemEntity);
					}
				}
				catch (Exception exception)
				{
					Sitecore.Diagnostics.Log.Error("[ItemEntitiesService] Can not read data from database.", exception, this);
					dataReader.Close();
				}
			}

			watch.Stop();
			Sitecore.Diagnostics.Log.Debug($"[ItemEntitiesService] Finished. Execution time {watch.Elapsed.TotalMilliseconds}ms.");

			return entities.ToArray();
		}

		public ItemEntity[] GetDescendants(Item parentItem, Language language = null)
		{
			if (parentItem == null)
			{
				return Array.Empty<ItemEntity>();
			}

			return this.GetDescendants(parentItem.ID, language);
		}

		public ItemEntity[] GetDescendants(ID parentId, Language language = null)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			Sitecore.Diagnostics.Log.Debug($"[ItemEntitiesService] Getting children items for {parentId} ({language}) was started.", this);

			if (language == null)
			{
				language = Sitecore.Context.ContentLanguage;
			}

			var entities = new List<ItemEntity>();

			var query = $"WITH Display_Name AS (SELECT ItemId, Value FROM UnversionedFields WHERE (FieldId = 'B5E02AD9-D56F-4C41-A065-A133DB87BDEB') AND (Language = '{language}')), Content_Items AS (SELECT ID, Name, ParentID, TemplateID FROM Items WHERE (ID = '{parentId}') " +
						$"UNION ALL SELECT i.ID, i.Name, i.ParentID, i.TemplateID FROM Items AS i INNER JOIN Content_Items AS ci ON ci.ID = i.ParentID) SELECT Content_Items_1.ID, Content_Items_1.Name, Content_Items_1.TemplateID, Content_Items_1.ParentID, Display_Name_1.Value AS Display_Name, Items_1.TemplateID AS ParentTemplateId FROM Content_Items AS Content_Items_1 INNER JOIN Items AS Items_1 ON Content_Items_1.ParentID = Items_1.ID LEFT OUTER JOIN Display_Name AS Display_Name_1 ON Content_Items_1.ID = Display_Name_1.ItemId WHERE (Content_Items_1.ID <> '{parentId}')";

			using (var connection = new SqlConnection(this.ConnectionString))
			{
				var command = new SqlCommand(query, connection);
				connection.Open();

				var dataReader = command.ExecuteReader();

				try
				{
					while (dataReader.Read())
					{
						var itemEntity = new ItemEntity()
						{
							ID = ID.Parse(dataReader[0]),
							Name = dataReader[1] as string,
							DisplayName = dataReader[2] as string,
							ParentID = parentId,
							ParentTemplateID = ID.Parse(dataReader[5])
						};

						entities.Add(itemEntity);
					}
				}
				catch (Exception exception)
				{
					Sitecore.Diagnostics.Log.Error("[ItemEntitiesService] Can not read data from database.", exception, this);
					dataReader.Close();
				}
			}

			watch.Stop();
			Sitecore.Diagnostics.Log.Debug($"[ItemEntitiesService] Finished. Execution time {watch.Elapsed.TotalMilliseconds}ms.");

			return entities.ToArray();
		}
	}
}
