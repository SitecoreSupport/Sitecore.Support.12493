namespace Sitecore.Support.XA.Feature.Composites.Pipelines.GetXmlBasedLayoutDefinition
{
    using Microsoft.Extensions.DependencyInjection;
    using Mvc.Extensions;
    using Mvc.Presentation;
    using Sitecore;
    using Sitecore.Data.Items;
    using Sitecore.DependencyInjection;
    using Sitecore.Mvc.Pipelines.Response.GetXmlBasedLayoutDefinition;
    using Sitecore.XA.Foundation.Caching;
    using Sitecore.XA.Foundation.Multisite;
    using Sitecore.XA.Foundation.Presentation.Layout;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class InjectCompositeComponents : Sitecore.XA.Feature.Composites.Pipelines.GetXmlBasedLayoutDefinition.InjectCompositeComponents
    {
        public override void Process(GetXmlBasedLayoutDefinitionArgs args)
        {
            Item item = args.ContextItem ?? PageContext.Current.Item;
            XElement result = args.Result;
            if ((result == null) || !item.Paths.IsContentItem)
            {
                return;
            }
            Item siteItem = ServiceProviderServiceExtensions.GetService<IMultisiteContext>(ServiceLocator.ServiceProvider).GetSiteItem(item);
            if (siteItem == null)
            {
                return;
            }
            IEnumerable<XElement> compositeComponents = this.GetCompositeComponents(result);
            if (!compositeComponents.Any<XElement>())
            {
                return;
            }

            #region Removed Code
            //DictionaryCacheValue dictionaryCacheValue = DictionaryCache.Get(CreateCompositesXmlCacheKey(item.ID, siteItem.ID));
            //if (Context.PageMode.IsNormal && dictionaryCacheValue != null && dictionaryCacheValue.Properties.ContainsKey("CompositesXml"))
            //{
            //    args.Result = XElement.Parse(dictionaryCacheValue.Properties["CompositesXml"].ToString());
            //}
            //else
            //{
            #endregion
            if (!args.CustomData.ContainsKey("sxa-composite-recursion-level"))
            {
                args.CustomData.Add("sxa-composite-recursion-level", 1);
            }
            else
            {
                args.CustomData["sxa-composite-recursion-level"] = ((int)args.CustomData["sxa-composite-recursion-level"]) + 1;
            }
            foreach (XElement element2 in compositeComponents)
            {
                this.ProcessCompositeComponent(args, element2, result);
            }
            List<XElement> content = result.Descendants("d").ToList<XElement>();
            args.Result.Descendants("d").Remove<XElement>();
            args.Result.Add(content);
            bool hasPersonalizationRules = false;
            foreach (var deviceModel in new LayoutModel(args.Result.ToString()).Devices.DevicesCollection)
            {
                var renderingModels = deviceModel.Renderings.RenderingsCollection.ToList();
                hasPersonalizationRules = renderingModels.Any(rm => rm.XmlNode.FindChildNode(node => node.Name.Equals("rls")) != null);
                if (hasPersonalizationRules)
                {
                    break;
                }
            }
            #region Removed Code
            //    if (Context.PageMode.IsNormal)
            //    {
            //        StoreValueInCache(CreateCompositesXmlCacheKey(item.ID, siteItem.ID), args.Result.ToString());
            //    }
            //}
            #endregion
        }
    }
}