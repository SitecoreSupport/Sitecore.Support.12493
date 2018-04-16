namespace Sitecore.Support.XA.Feature.Composites.Pipelines.GetXmlBasedLayoutDefinition
{
  using Microsoft.Extensions.DependencyInjection;
  using Sitecore;
  using Sitecore.Data.Items;
  using Sitecore.DependencyInjection;
  using Sitecore.Mvc.Pipelines.Response.GetXmlBasedLayoutDefinition;
  using Sitecore.XA.Foundation.Caching;
  using Sitecore.XA.Foundation.Multisite;
  using System.Collections.Generic;
  using System.Linq;
  using System.Xml.Linq;

  public class InjectCompositeComponents : Sitecore.XA.Feature.Composites.Pipelines.GetXmlBasedLayoutDefinition.InjectCompositeComponents
  {
    public override void Process(GetXmlBasedLayoutDefinitionArgs args)
    {
      Item item = args.ContextItem ?? Sitecore.Mvc.Presentation.PageContext.Current.Item;
      XElement result = args.Result;
      if (result != null && item.Paths.IsContentItem)
      {
        Item siteItem = ServiceLocator.ServiceProvider.GetService<IMultisiteContext>().GetSiteItem(item);
        if (siteItem != null)
        {
          List<XElement> compositeComponents = GetCompositeComponents(result);
          if (compositeComponents.Any())
          {
            DictionaryCacheValue dictionaryCacheValue =
              DictionaryCache.Get(CreateCompositesXmlCacheKey(item.ID, siteItem.ID));
            if (Context.PageMode.IsNormal && dictionaryCacheValue != null &&
                dictionaryCacheValue.Properties.ContainsKey("CompositesXml"))
            {
              args.Result = XElement.Parse(dictionaryCacheValue.Properties["CompositesXml"].ToString());
            }
            else
            {
              if (!args.CustomData.ContainsKey("sxa-composite-recursion-level"))
              {
                args.CustomData.Add("sxa-composite-recursion-level", 1);
              }
              else
              {
                args.CustomData["sxa-composite-recursion-level"] =
                  (int) args.CustomData["sxa-composite-recursion-level"] + 1;
              }
              foreach (XElement item2 in compositeComponents)
              {
                ProcessCompositeComponent(args, item2, result);
              }
              List<XElement> content = result.Descendants("d").ToList();
              args.Result.Descendants("d").Remove();
              args.Result.Add(content);
              if (Context.PageMode.IsNormal)
              {
                StoreValueInCache(CreateCompositesXmlCacheKey(item.ID, siteItem.ID), args.Result.ToString());
              }
            }
          }
        }
      }
    }
  }
}