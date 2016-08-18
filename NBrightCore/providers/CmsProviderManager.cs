using System;

namespace NBrightCore.providers
{
    public class CmsProviderManager
    {
        static CmsProviderManager()
	    {
            Initialize();
	    }

        private static CmsProvider _objProvider;        
        
        /// <summary>
        /// Returns the default configured data provider
        /// </summary>
        public static CmsProvider Default
        {
            get { return _objProvider; }
        }

                // dynamically create provider
        private static void CreateProvider()
        {
            //Always link to NBrightDNN
            string providerAssembyClass = "NBrightDNN,NBrightDNN.DnnInterface";
            if (!string.IsNullOrEmpty(providerAssembyClass))
            {
                var prov = providerAssembyClass.Split(Convert.ToChar(","));
                var handle = Activator.CreateInstance(prov[0], prov[1]);
                _objProvider = (CmsProvider)handle.Unwrap();
            }
        }


        private static void Initialize()
        {
            if (_objProvider == null)
            {
                CreateProvider();
            }
        }


    }
}
