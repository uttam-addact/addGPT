using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lawfirm.Foundation.SitecoreExtensions
{
	public static class Templates
	{
		public struct IBreadcrumb
		{
			public static readonly ID TemplateId = new ID("{9610EB9A-4999-4B6C-844E-47CF667C14B5}");

			public struct Fields
			{
				public static readonly ID PageBreadcrumbTitle = new ID("{AA0116F7-63B4-4134-A08B-DDC4ECE22FC8}");
			}
		}
		public struct IRightRailNavigation
		{
			public static readonly ID TemplateId = new ID("{D31FEFC7-1498-4FCC-80C1-49FD015AF4A0}");
		}
	}
}
