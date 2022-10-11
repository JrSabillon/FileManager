using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web;

namespace SyT_FileManager.AppCode
{
    public class BarcodeHandler
    {
        public Font LoadFont(string font, int size)
        {
            var pfc = new PrivateFontCollection();
            string f = string.Empty;

            switch (font)
            {
                case "E39":
                default:
                    f = "Code39.ttf";
                    break;
                case "E13":
                    //f = "EAN-13.TTF";
                    throw new NotImplementedException();
                case "E9":
                    //f = "FRE30F9X.TTF";
                    throw new NotImplementedException();
            }

            string fontsDir = ConfigurationManager.AppSettings.Get("FontsPath");
            if(Directory.Exists(fontsDir))
                pfc.AddFontFile(fontsDir + @"" + f);

            return new Font(pfc.Families[0], size);
        }

        public string FormatBarcode(string code)
        {
            return $"*{code}*";
        }
    }
}