using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Alloy.Business;
using Alloy.Models.Pages;
using Alloy.Models.ViewModels.SingletonPage;
using EPiServer.Core;
using EPiServer.Shell.Navigation;

namespace Alloy.Controllers
{
    [Authorize(Roles = "WebAdmins, Administrators")]
    [RoutePrefix("episerver/singletons")]
    public class SingletonPageController : Controller
    {
        [HttpGet]
        [Route("edit")]
        [MenuItem("/global/cms/edit", Text = "Singletons", Url = "/episerver/singletons/edit")]
        public ActionResult Index()
        {
            var model = new EditViewModel();
            foreach (var keyActions in _pageKeyActions)
            {
                if (keyActions.Value == null || keyActions.Value.DoesPageTypeExist == null) continue;
                {
                    var singleton = new SingletonViewModel() {Key = keyActions.Key};

                    if(keyActions.Value.DoesPageTypeExist())
                        model.Exists.Add(singleton);
                    else
                        model.NonExisting.Add(singleton);
                }
            }
            return View(model);
        }

        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(EditViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.CreateTypeKey) && _pageKeyActions.ContainsKey(model.CreateTypeKey))
            {
                var createAction = _pageKeyActions[model.CreateTypeKey].CreatePageType;
                if (createAction != null)
                {
                    var result = createAction();
                    if (!result.Created)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, result.Message);
                    }
                }
            }
            return new RedirectResult("/episerver/singletons/edit");
        }

        #region Helper methods

        private Dictionary<string, PageActions> _pageKeyActions = new Dictionary<string, PageActions>
        {
            { "StartPage", new PageActions(Exists<StartPage>, () => Create<StartPage>("Start"))},
            { "StaticFirstPage", new PageActions(Exists<StaticFirstPage>, () => Create<StaticFirstPage>("StaticFirstPage"))},
            { "StaticPages", new PageActions(CheckIfStaticPagesExists, CreateStaticPages)}
        };
        
        private class PageActions
        {
            public PageActions(Func<bool> pageTypeExistsAction, Func<PageCreateResult> createPageType)
            {
                DoesPageTypeExist = pageTypeExistsAction;
                CreatePageType = createPageType;
            }

            /// <summary>
            /// Should return true if page type exists
            /// </summary>
            public Func<bool> DoesPageTypeExist { get; private set; }
            public Func<PageCreateResult> CreatePageType { get; private set; }
        }

        /// <summary>
        /// This method can be used to check if singleton pages exists if not much custom stuff is needed.
        /// </summary>
        private static bool Exists<T>() where T : PageData
        {
            return SingletonFactory.CheckIfSingletonExists<T>();
        }

        /// <summary>
        /// This method can be used to create singleton pages if not much custom stuff is needed.
        /// </summary>
        private static PageCreateResult Create<T>(string name, ContentReference ancestor = null, Action<T> pageActions = null, string urlOverride = null) where T : PageData
        {
            try
            {
                SingletonFactory.CreateOrGetSingletonPage<T>(name, ancestor ?? ContentReference.RootPage, pageActions, urlOverride);
            }
            catch (Exception ex)
            {
                // Log("Error trying to create page...")
                return new PageCreateResult(string.Format("Error trying to create page {0}, exception: {1}", name, ex));
            }

            return (PageCreateResult)true;
        }

        private class PageCreateResult
        {
            public PageCreateResult(bool created)
            {
                Created = created;
            }

            public PageCreateResult(string message, bool created = false)
            {
                Created = created;
                Message = message;
            }

            public bool Created { get; private set; }

            public string Message { get; private set; }

            public static explicit operator PageCreateResult(bool created)
            {
                return new PageCreateResult(created);
            }
        }

        private static bool CheckIfStaticPagesExists()
        {
            // checks if _any_ of the static pages in the group of pages exists. 'StaticFirstPage' is not part of the group
            var staticContainerRef = SingletonFactory.GetSingletonPageRef<StaticContainerPage>();
            if (staticContainerRef == null || staticContainerRef == ContentReference.EmptyReference) return false;

            var staticSecondRef = SingletonFactory.GetSingletonPageRef<StaticSecondPage>();
            if (staticSecondRef == null || staticSecondRef == ContentReference.EmptyReference) return false;

            var staticThirdRef = SingletonFactory.GetSingletonPageRef<StaticThirdPage>();
            if (staticThirdRef == null || staticThirdRef == ContentReference.EmptyReference) return false;

            return true;
        }

        private static PageCreateResult CreateStaticPages()
        {
            var containerRef = SingletonFactory.CreateOrGetSingletonPage<StaticContainerPage>("StaticContainerPage", ContentReference.RootPage, urlOverride:"StaticContainerPage");
            SingletonFactory.CreateOrGetSingletonPage<StaticSecondPage>("StaticSecondPage", containerRef, urlOverride: "StaticSecondPage");
            SingletonFactory.CreateOrGetSingletonPage<StaticThirdPage>("StaticThirdPage", containerRef, 
                (p) => {   
                    p.MetaDescription = "Predefined meta description"; 
                    p.SummaryText = "Predefined summary text"; 
                },
                urlOverride: "StaticThirdPage");

            return (PageCreateResult) true;
        }

        #endregion
    }
}