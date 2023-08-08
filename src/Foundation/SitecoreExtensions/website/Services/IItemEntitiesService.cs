using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lawfirm.Foundation.SitecoreExtensions.Entities;
using Sitecore.Data;
using Sitecore.Globalization;
using Sitecore.Data.Items;

namespace Lawfirm.Foundation.SitecoreExtensions.Services
{
	public interface IItemEntitiesService
	{
		ItemEntity[] GetDescendants(Item parentItem, Language language = null);

		ItemEntity[] GetDescendants(ID parentId, Language language = null);

		ItemEntity[] GetChildren(Item parentItem, Language language = null);

		ItemEntity[] GetChildren(ID parentId, Language language = null);
	}
}
