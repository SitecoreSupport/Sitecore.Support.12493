namespace Sitecore.Support.XA.Foundation.Presentation.Pipelines.GetXmlBasedLayoutDefinition
{
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.DependencyInjection;
    using Sitecore.Mvc.Pipelines.Response.GetXmlBasedLayoutDefinition;
    using Sitecore.Mvc.Presentation;
    using Sitecore.SecurityModel.License;
    using Sitecore.XA.Foundation.Caching;
    using Sitecore.XA.Foundation.Multisite;
    using Sitecore.XA.Foundation.Presentation;
    using Sitecore.XA.Foundation.Presentation.Services;
    using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class AddPartialDesignsRenderings : Sitecore.XA.Foundation.Presentation.Pipelines.GetXmlBasedLayoutDefinition.AddPartialDesignsRenderings
    {
        public override void Process(GetXmlBasedLayoutDefinitionArgs args)
        {
            Item item = args.ContextItem ?? PageContext.Current.Item;
            if (item != null)
            {
                Item designItem = ServiceProviderServiceExtensions.GetService<IPresentationContext>(ServiceLocator.ServiceProvider).GetDesignItem(item);
                if ((designItem != null) || item.InheritsFrom(Sitecore.XA.Foundation.Presentation.Templates.PartialDesign.ID))
                {
                    if (!Sitecore.SecurityModel.License.License.HasModule("Sitecore.SXA"))
                    {
                        args.PageContext.RequestContext.HttpContext.Response.Redirect($"{Settings.NoLicenseUrl}?license=Sitecore.SXA");
                    }
                    else
                    {
                        ID designId = (designItem != null) ? designItem.ID : item.ID;
                        Item siteItem = ServiceProviderServiceExtensions.GetService<IMultisiteContext>(ServiceLocator.ServiceProvider).GetSiteItem(item);
                        ILayoutXmlService service = ServiceProviderServiceExtensions.GetService<ILayoutXmlService>(ServiceLocator.ServiceProvider);

                        #region Removed Code
                        //DictionaryCacheValue dictionaryCacheValue = DictionaryCache.Get(CreateLayoutXmlCacheKey(item.ID, designId, siteItem.ID));
                        //if (Context.PageMode.IsNormal && dictionaryCacheValue != null && dictionaryCacheValue.Properties.ContainsKey("LayoutXml"))
                        //{
                        //    args.Result = (dictionaryCacheValue.Properties["LayoutXml"] as XElement);
                        //}
                        //else
                        //{
                        #endregion
                        List<XElement> source = service.GetRenderings(item, designItem).ToList<XElement>();
                        if (source.Any<XElement>())
                        {
                            service.MergePartialDesignsRenderings(args.Result, source);
                            #region Removed Code
                            //    if (Context.PageMode.IsNormal)
                            //    {
                            //        StoreValueInCache(CreateLayoutXmlCacheKey(item.ID, designId, siteItem.ID), args.Result);
                            //    }
                            //}
                            #endregion
                        }
                    }
                }
            }
        }
    }
}