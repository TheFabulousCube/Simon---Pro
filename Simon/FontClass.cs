using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Simon
{
    public class FontClass
    {
        public FontFamily MyFontFamily { get; set; }
        public string FontFamilyValue { get; set; }
        public override string ToString()
        {
            return FontFamilyValue;
        }
    }
}
