using PuppeteerSharp;
using PuppeteerSharp.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Domain.Extentions
{
    public static class Common
    {
        public static string Convert(this IJSHandle prop)
        {
            return prop.RemoteObject.Value.ToString().Replace("\n", ",");
        }
    }
}
