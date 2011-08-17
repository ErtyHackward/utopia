//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Utopia.GUI.XnaAdapters
//{

//    //This design pattern is nearly an anti pattern, explicit dependency injection is way more clean
//    // check http://www.beefycode.com/post/Why-I-Hate-IServiceProvider.aspx
//    // This was implemented only to ease the nuclex UI integration 
//    // and if you dwelve into nuclex UI code you ll find it only serves to get the GraphicsDevice from the ContentManager !

//    public class ServiceProvider : IServiceProvider
//    {
//        Dictionary<Type, object> services = new Dictionary<Type, object>();


//        /// <summary>
//        /// Adds a new service to the collection.
//        /// </summary>
//        public void AddService<T>(T service)
//        {
//            services.Add(typeof(T), service);
//        }
     

//        /// <summary>
//        /// Looks up the specified service.
//        /// </summary>
//        public object GetService(Type serviceType)
//        {
//            object service;

//            services.TryGetValue(serviceType, out service);

//            return service;
//        }
//    }

//}
