using System.Linq;
using Alloy.Models.Pages;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace Alloy.Business
{
    public static class StaticReferenceUtility
    {
        public static void RefreshStaticReferences()
        {
            SetStaticFirstPage();
            SetStaticContainerPages();
        }

        public static bool TryGetSingletonPageRef<T>(out ContentReference singletonRef) where T : PageData
        {
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            ContentReference pageRef =
                contentRepository.GetDescendents(ContentReference.RootPage)
                    .FirstOrDefault(
                        c => contentRepository.Get<IContent>(c).GetOriginalType() == typeof(T));

            if (pageRef == null || pageRef == ContentReference.EmptyReference)
            {
                singletonRef = ContentReference.EmptyReference;
                return false;
            }

            singletonRef = pageRef;
            return true;
        }


        private static void SetStaticFirstPage()
        {
            ContentReference staticFirstRef;
            if (TryGetSingletonPageRef<StaticFirstPage>(out staticFirstRef))
                StaticContentReferences.StaticFirstPage = staticFirstRef;
        }

        private static void SetStaticContainerPages()
        {
            ContentReference staticContainerRef;
            if (TryGetSingletonPageRef<StaticContainerPage>(out staticContainerRef))
                StaticContentReferences.StaticContainerPage = staticContainerRef;

            ContentReference staticSecondRef;
            if (TryGetSingletonPageRef<StaticSecondPage>(out staticSecondRef))
                StaticContentReferences.StaticSecondPage = staticSecondRef;

            ContentReference staticThirdRef;
            if (TryGetSingletonPageRef<StaticThirdPage>(out staticThirdRef))
                StaticContentReferences.StaticThirdPage = staticThirdRef;
        }
    }
}