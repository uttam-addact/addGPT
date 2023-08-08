using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lawfirm.Foundation.SitecoreExtensions.Entities
{
	public class ItemEntity
	{
		public ID ID { get; set; }

		public ID ParentID { get; set; }

		public ID TemplateID { get; set; }

		public ID ParentTemplateID { get; set; }

		public string Name { get; set; }

		public string DisplayName { get; set; }
	}
}
