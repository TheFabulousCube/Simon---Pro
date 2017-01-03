using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRTXamlToolkit.Controls;

namespace Simon
{
    struct Hint
    {
        private int numberOfHints;

        public Hint(int hintStartingNumber) : this()
        {
            numberOfHints = hintStartingNumber;
        }

        public int NumberOfHints
        {
            get { return numberOfHints; }
            set { numberOfHints = (value >= 0) ? value: 0; }
        }


        public void UseAHint(ImageButton ib)
        {
            if (numberOfHints >= 1)
            {
                ib.NormalStateImageUriSource = ib.PressedStateImageUriSource;
                ib.HoverStateImageUriSource = ib.PressedStateImageUriSource;
                numberOfHints -= 1;
            }
        }

        public void EarnAHint()
        {
            numberOfHints += 1;
        }

        public bool HintsAvailable()
        {
            return numberOfHints > 0;
        }

        public string DisplayHints()
        {
            return (this.HintsAvailable()) ? "Visible" : "Collapsed";
        }

        public override string ToString()
        {
            return ("Hints\n" + numberOfHints);
        }
    }
}
