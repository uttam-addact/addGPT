using Newtonsoft.Json.Linq;
using Sitecore.Data.Items;
using Sitecore.LayoutService.Configuration;
using Sitecore.Mvc.Presentation;

namespace Lawfirm.Foundation.SitecoreExtensions.Resolvers
{
	public class ItemandItemDescendentsContentResolver : Sitecore.LayoutService.ItemRendering.ContentsResolvers.RenderingContentsResolver
	{
		protected override JObject ProcessItem(Item item, Rendering rendering, IRenderingConfiguration renderingConfig)
		{
			var jObject = base.ProcessItem(item, rendering, renderingConfig);

			if (item.Children.Count == 0) return jObject;

			jObject["items"] = ProcessItems(item.Children, rendering, renderingConfig);

			return jObject;
		}
	}
}

