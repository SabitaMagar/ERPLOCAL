﻿using System.Web;
using System.Web.Mvc;

namespace NeoERP.Planning
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
