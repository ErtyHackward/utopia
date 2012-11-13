using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UtopiaApi.Models.Repositories
{
    public class Repository
    {
        private readonly Lazy<UtopiaDataContext> _lazyContext = new Lazy<UtopiaDataContext>(() => new UtopiaDataContext());

        public UtopiaDataContext Context
        {
            get { return _lazyContext.Value; }
        }
    }
}