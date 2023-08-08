using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sitecore;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.DependencyInjection;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using Sitecore.Buckets.Extensions;
using Sitecore.IO;
using Sitecore.Sites;
using Sitecore.Web;
using Sitecore.Diagnostics;
using System.Globalization;
using System.Web;
using Lawfirm.Foundation.SitecoreExtensions.Entities;
using Lawfirm.Foundation.SitecoreExtensions.Services;

namespace Lawfirm.Foundation.SitecoreExtensions.Extensions
{
	public static class ItemExtensions
	{
		public static Item GetInLanguageWithVersion(this Item item)
		{
			var existingLanguage = ExistingLanguages(item).FirstOrDefault();
			return existingLanguage == null
				? (Item)null
				: Sitecore.Context.Database.GetItem(item.ID, existingLanguage);
		}
		public static Language[] ExistingLanguages(this Item item)
		{
			return ItemManager.GetContentLanguages(item).Where(lang => ItemManager.GetVersions(item, lang).Count > 0).ToArray();
		}

		public static string RelativeUrl(this Item item, UrlOptions options = null)
		{
			var serverUrlRegex = new Regex("(http[s]?)*(://)+([a-zA-Z0-9.-]*)");

			var url = item.Url(options);

			var relativeUrl = serverUrlRegex.Replace(url, string.Empty);

			return relativeUrl;
		}

		public static string Url(this Item item, UrlOptions options = null)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			if (item.Paths.IsMediaItem)
			{
				return MediaManager.GetMediaUrl(item);
			}

			return options != null ? LinkManager.GetItemUrl(item, options) : LinkManager.GetItemUrl(item);
		}

		public static string ImageUrl(this MediaItem mediaItem, int width, int height)
		{
			if (mediaItem == null)
			{
				throw new ArgumentNullException(nameof(mediaItem));
			}

			var options = new MediaUrlOptions { Height = height, Width = width };
			var url = MediaManager.GetMediaUrl(mediaItem, options);
			var cleanUrl = StringUtil.EnsurePrefix('/', url);
			var hashedUrl = HashingUtils.ProtectAssetUrl(cleanUrl);

			return hashedUrl;
		}


		public static Item TargetItem(this Item item, ID linkFieldId)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			if (item.Fields[linkFieldId] == null || !item.Fields[linkFieldId].HasValue)
			{
				return null;
			}

			return ((LinkField)item.Fields[linkFieldId]).TargetItem ??
				   ((ReferenceField)item.Fields[linkFieldId]).TargetItem;
		}

		public static bool IsImage(this Item item)
		{
			return new MediaItem(item).MimeType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool IsVideo(this Item item)
		{
			return new MediaItem(item).MimeType.StartsWith("video/", StringComparison.InvariantCultureIgnoreCase);
		}


		public static Item GetAncestor(this Item item, ID templateId)
		{
			return item.Axes
				?.GetAncestors()
				?.FirstOrDefault(i => i.DescendsFrom(templateId));
		}

		public static Item GetLastAncestor(this Item item, ID templateId)
		{
			return item.Axes
				?.GetAncestors()
				?.LastOrDefault(i => i.DescendsFrom(templateId));
		}

		public static Item GetAncestorOrSelfOfTemplate(this Item item, ID templateId)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			return item.IsDerived(templateId)
				? item
				: item.Axes.GetAncestors().LastOrDefault(i => i.IsDerived(templateId));
		}

		public static IList<Item> GetAncestorsAndSelfOfTemplate(this Item item, ID templateId)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			var returnValue = new List<Item>();
			if (item.IsDerived(templateId))
			{
				returnValue.Add(item);
			}

			returnValue.AddRange(item.Axes.GetAncestors().Reverse().Where(i => i.IsDerived(templateId)));
			return returnValue;
		}

		public static string LinkFieldUrl(this Item item, ID fieldId)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			if (ID.IsNullOrEmpty(fieldId))
			{
				throw new ArgumentNullException(nameof(fieldId));
			}

			var field = item.Fields[fieldId];
			if (field == null || !(FieldTypeManager.GetField(field) is LinkField))
			{
				return string.Empty;
			}

			LinkField linkField = field;
			switch (linkField.LinkType.ToLower())
			{
				case "internal":
					// Use LinkMananger for internal links, if link is not empty
					return linkField.TargetItem != null ? GetItemUrl(linkField.TargetItem) : string.Empty;
				case "media":
					// Use MediaManager for media links, if link is not empty
					return linkField.TargetItem != null ? MediaManager.GetMediaUrl(linkField.TargetItem) : string.Empty;
				case "external":
					// Just return external links
					return linkField.Url;
				case "anchor":
					// Prefix anchor link with # if link if not empty
					return !string.IsNullOrEmpty(linkField.Anchor) ? "#" + linkField.Anchor : string.Empty;
				case "mailto":
					// Just return mailto link
					return linkField.Url;
				case "javascript":
					// Just return javascript
					return linkField.Url;
				case "tel":
					// Just return telephone
					return linkField.Url;
				default:
					// Just please the compiler, this
					// condition will never be met
					return linkField.Url;
			}
		}

		public static string GetItemUrl(Item item)
		{
			try
			{
				var website = Sitecore.Configuration.Factory.GetSite(item.GetSiteName());
				using (new SiteContextSwitcher(website))
				{
					var options = LinkManager.GetDefaultUrlOptions();
					options.AlwaysIncludeServerUrl = false;
					options.SiteResolving = true;
					options.LanguageEmbedding = LanguageEmbedding.Always;

					var url = LinkManager.GetItemUrl(item, options);

					if (!url.StartsWith("/") // skip for relative path
						&& !url.StartsWith("https") && !url.StartsWith("http"))
					{
						var uri = new Uri($"https{url}");
						return uri.AbsolutePath;
					}

					return url;
				}
			}
			catch (Exception exception)
			{
				Log.Error($"Error while getting Item Url. Item ID - {item.ID}", exception, typeof(ItemExtensions));
				return string.Empty;
			}
		}

		public static string LinkFieldTarget(this Item item, ID fieldId)
		{
			return item.LinkFieldOptions(fieldId, LinkFieldOption.Target);
		}

		public static string LinkFieldOptions(this Item item, ID fieldId, LinkFieldOption option)
		{
			XmlField field = item.Fields[fieldId];
			switch (option)
			{
				case LinkFieldOption.Text:
					return field?.GetAttribute("text");
				case LinkFieldOption.LinkType:
					return field?.GetAttribute("linktype");
				case LinkFieldOption.Class:
					return field?.GetAttribute("class");
				case LinkFieldOption.Alt:
					return field?.GetAttribute("title");
				case LinkFieldOption.Target:
					return field?.GetAttribute("target");
				case LinkFieldOption.QueryString:
					return field?.GetAttribute("querystring");
				default:
					throw new ArgumentOutOfRangeException(nameof(option), option, null);
			}
		}

		public static bool HasLayout(this Item item)
		{
			return item?.Visualization?.Layout != null;
		}


		public static bool IsDerived(this Item item, ID templateId)
		{
			if (item == null)
			{
				return false;
			}

			return !templateId.IsNull && item.IsDerived(item.Database.Templates[templateId]);
		}

		private static bool IsDerived(this Item item, Item templateItem)
		{
			if (item == null)
			{
				return false;
			}

			if (templateItem == null)
			{
				return false;
			}

			var itemTemplate = TemplateManager.GetTemplate(item);
			return itemTemplate != null &&
				   (itemTemplate.ID == templateItem.ID || itemTemplate.DescendsFrom(templateItem.ID));
		}

		public static bool FieldHasValue(this Item item, ID fieldId)
		{
			if (item == null)
			{
				return false;
			}

			return item.Fields[fieldId] != null && !string.IsNullOrWhiteSpace(item.Fields[fieldId].Value);
		}

		public static int? GetInteger(this Item item, ID fieldId)
		{
			int result;
			return !int.TryParse(item.Fields[fieldId].Value, out result) ? new int?() : result;
		}

		public static double? GetDouble(this Item item, ID fieldId)
		{
			var value = item?.Fields[fieldId]?.Value;
			if (value == null)
			{
				return null;
			}

			double num;
			if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out num) ||
				double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out num) ||
				double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out num))
			{
				return num;
			}

			return null;
		}

		public static IEnumerable<Item> GetMultiListValueItems(this Item item, ID fieldId)
		{
			return new MultilistField(item.Fields[fieldId]).GetItems();
		}

		public static bool HasContextLanguage(this Item item)
		{
			var latestVersion = item.Versions.GetLatestVersion();
			return latestVersion?.Versions.Count > 0;
		}


		public static SiteInfo GetSiteInfo(this Item item)
		{
			var contextItemPath = item.Paths.Path.ToLower();
			var site = SiteContextFactory.Sites
				.FirstOrDefault(s =>
					s.VirtualFolder == "/" && s.RootPath != "" &&
					contextItemPath.StartsWith(FileUtil.MakePath(s.RootPath, s.StartItem).ToLower()));

			return site;
		}

		public static string GetSiteName(this Item item)
		{
			var info = item.GetSiteInfo();
			return info != null ? info.Name : string.Empty;
		}

		public static string ImageUrl(this Item item, ID imageFieldId)
		{
			if (item == null)
			{
				return null;
			}

			var imageField = (ImageField)item.Fields[imageFieldId];
			return imageField?.MediaItem == null ? string.Empty : ((MediaItem)imageField.MediaItem).ImageUrl();
		}

		public static string ImageUrl(this MediaItem mediaItem)
		{
			if (mediaItem == null)
			{
				return null;
			}

			var url = MediaManager.GetMediaUrl(mediaItem);
			var hashedUrl = HashingUtils.ProtectAssetUrl(url);

			return hashedUrl;
		}

		public static bool ItemTreeMatchedPath(this Item i, string path)
		{
			var pathItems = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Reverse();
			return ItemTreeMatchedPath(i, pathItems);
		}

		public static bool ItemTreeMatchedPath(this Item i, IEnumerable<string> pathItems)
		{
			//we loop through the items parents making sure either the name or the displayname matches the item in the path
			//skip buckets
			//return as soon as it doesnt
			Item processingItem = i;
			foreach (var pathSegment in pathItems)
			{
				processingItem = processingItem.Parent;
				if (processingItem == null)
				{
					return false;
				}

				while (processingItem.IsABucket() || processingItem.IsABucketFolder())
				{
					processingItem = processingItem.Parent;
				}

				if (!processingItem.DisplayName.Equals(pathSegment, StringComparison.OrdinalIgnoreCase) &&
					!processingItem.Name.Equals(pathSegment, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}

			return true;
		}

		
		/// <summary>
		/// Check if specific parent item template id's are under the bucket folder
		/// </summary>
		/// <param name="contextItem">Current context item in Bucket</param>
		/// <param name="templateIds">List of parent templates</param>
		/// <returns>If desirable parent item was found, otherwise, returns null</returns>
		public static Item GetSpecificTemplatesInBucket(this Item contextItem, IEnumerable<ID> templateIds)
		{
			var parentBucketItem = contextItem.Parent;

			while (!parentBucketItem.IsABucketFolder())
			{
				if (templateIds.Any(id => parentBucketItem.TemplateID == id))
				{
					return parentBucketItem;
				}

				parentBucketItem = parentBucketItem.Parent;

				if (parentBucketItem == null)
				{
					return null;
				}
			}

			return null;
		}
	}

	public enum LinkFieldOption
	{
		Text,
		LinkType,
		Class,
		Alt,
		Target,
		QueryString
	}
}
