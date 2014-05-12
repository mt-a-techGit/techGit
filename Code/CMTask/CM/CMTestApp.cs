using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using CMCore;
using CMCore.webdriver;
using CMCore.site;


    class CMTestApp
    {
         
        static void Main(string[] args)
        {
            CMLib CM = new CMLib();
            CM.start(args[0]);
        }
    }
