using EPiServer;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace Alloy.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency((typeof(global::EPiServer.Web.InitializationModule)))]
    public class StaticReferencesInitialization : IInitializableModule
    {
        private IContentRepository _contentRepository;

        public void Initialize(InitializationEngine context)
        {
            //ISSUE: Works ok for single language, but will probably be issues with multi and/or enterprise
            StaticReferenceUtility.RefreshStaticReferences();
        }
        
        public void Uninitialize(InitializationEngine context)
        {

        }

        public void Preload(string[] parameters)
        {

        }
    }
}