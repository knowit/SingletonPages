using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace Alloy.Models.Pages
{
    [SiteContentType(GroupName = Global.GroupNames.Singletons)]
    public class StaticThirdPage : StandardPage
    {
        [Display(GroupName = SystemTabNames.Content,
            Order = 330)]
        [CultureSpecific]
        public virtual string SummaryText { get; set; }
    }
}