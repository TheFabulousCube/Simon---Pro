using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Simon
{
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        /************************************************************************
         * This page handles binding to the radio buttons for 
         * string = currentImage  and string=currentSound
         * the values are stored as bools
         * It is based of off:
         * http://blog.jerrynixon.com/2014/12/lets-code-data-binding-to-radio-button.html
         * 
         * 
Value = currentImage
_Value = privateImage
one, two, three = Flat, Raised, Metallic
ValueAs1, = FlatButtons
         * **********************************************************************/

        private string privateImage = "Flat";
        private string privateSound = "Churchbell";
        // string currentSound;

        public event PropertyChangedEventHandler PropertyChanged;


        public string currentImage
        {
            get { return privateImage; }
            set
            {
                SetProperty(ref privateImage, value);
                RaisePropertyChanged("FlatButtons");
                RaisePropertyChanged("RaisedButtons");
                RaisePropertyChanged("MetallicButtons");
                roamingSettings.Values["savedImage"] = currentImage;
                AdjustImages(currentImage);
            }
        }

        public string currentSound
        {
            get { return privateSound; }
            set
            {
                SetProperty(ref privateSound, value);
                RaisePropertyChanged("ChurchbellSounds");
                RaisePropertyChanged("ToneSounds");
                RaisePropertyChanged("MeowSounds");
                roamingSettings.Values["savedSound"] = currentSound;
                AdjustSounds(currentSound);
            }
        }
        public bool FlatButtons
        {
            get { return currentImage.Equals("Flat"); }
            set {
                    currentImage = "Flat";
                    AdjustImages(currentImage);
                }
        }
        public bool RaisedButtons
        {
            get { return currentImage.Equals("Raised"); }
            set
            {
                currentImage = "Raised";
                AdjustImages(currentImage);
            }
        }
        public bool MetallicButtons
        {
            get { return currentImage.Equals("Metallic"); }
            set
            {
                currentImage = "Metallic";
                AdjustImages(currentImage);
            }
        }

        public bool ChurchbellSounds
        {
            get { return currentSound.Equals("Churchbell"); }
            set
            {
                currentSound = "Churchbell";
                AdjustSounds(currentSound);
            }
        }

        public bool ToneSounds
        {
            get { return currentSound.Equals("Tone"); }
            set
            {
                currentSound = "Tone";
                AdjustSounds(currentSound);
            }
        }

        public bool MeowSounds
        {
            get { return currentSound.Equals("Meow"); }
            set
            {
                currentSound = "Meow";
                AdjustSounds(currentSound);
            }
        }


        // SetField (Name, value); // where there is a data member
        public bool SetProperty<T>(ref T field, T value, [CallerMemberName] String property
           = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(property);
            return true;
        }

        public void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }


    }
 
}
