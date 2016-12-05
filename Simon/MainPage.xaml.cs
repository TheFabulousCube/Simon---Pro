using Simon.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Simon
{
    /// <summary>
    /// An adaptation of the classic Simon Memory game.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Game constants
        private const int HintStartingNumber = 3;
        private const int HintFrequency = 3;
        const int arrayOffset = -1;

        // Game variables
        bool playerTurn = false;
        private int randNumber;
        private int hints = HintStartingNumber;
        private int numberOfButtons = 6;
        private int playerSequence = 0;

        // Visual Elements
        public static string currentImage = "Flat";
        public static string currentSound = "Churchbell";
        public static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        public static ApplicationDataCompositeValue[] storedScore = new ApplicationDataCompositeValue()[];
        StorageFolder roamingFolder = ApplicationData.Current.RoamingFolder;


        // Helper arrays
        List<ImageButton> allButtons;
        List<ImageButton> sequence;
        List<MediaElement> allSounds;

        // Top Scores
        TopScore thisScore;
        public static IList<TopScore> topTen = new List<TopScore>();

        private bool AllButtons = false;
        private bool gameOver = false;

        public MainPage()
        {
            this.InitializeComponent();
            allButtons = new List<ImageButton> { Red, Blue, Green, Yellow, Orange, Purple };
            sequence = new List<ImageButton>();
            allSounds = new List<MediaElement> { buttonSound1, buttonSound2, buttonSound3, buttonSound4, buttonSound5, buttonSound6 };
            if (roamingSettings != null)  LoadSettings();
            AdjustSounds(currentSound);
            AdjustImages(currentImage);
            AreYouReady();
            PopulateSoundPicker();
        } // end MainPage

        private void LoadSettings()
        {
            currentSound = (roamingSettings.Values["savedSound"] != null) ? roamingSettings.Values["savedSound"].ToString() : currentSound;
            currentImage = roamingSettings.Values["savedImage"] != null ? roamingSettings.Values["savedImage"].ToString() : currentImage;
            if (roamingSettings.Values["topTen"] != null)
            {
                for (int i = 1; i <= 10; i++)
                {
                    // Top scores are stored as an array, storedScore
                    // each score is comprised of name, round, & level
                    storedScore[0]["name"] = topTen.N
                }
                topTen = roamingSettings.Values["topTen" + 2] as IList<TopScore>;
            }
        }

        private void PopulateSoundPicker()
        {
            foreach (MediaElement sound in allSounds)
            { }
        }

        public async void SimonsTurn()
        {
            if (gameOver)
            {
                await GameOver();

            }
            /** Start Game **/
            if (!playerTurn) // Simon's Turn
            {
                await Task.Delay(800); // give player a second to get ready

                // disable all the buttons for Simon's Turn
                foreach (ImageButton button in allButtons)
                {
                    button.IsHitTestVisible = false;
                }
                Hint.IsHitTestVisible = false;
                SixButtons.IsHitTestVisible = false;
                ImagePickerFlyout.IsHitTestVisible = false;
                SoundPickerFlyout.IsHitTestVisible = false;
                Restart.IsHitTestVisible = false;
                /** Pick a new button & add it to the sequence */
                Random buttonSelected = new Random();
                randNumber = buttonSelected.Next(0, numberOfButtons);
                sequence.Add(allButtons[randNumber]);
                /** Pick a new button & add it to the sequence */

                // Simon plays the sequence
                foreach (ImageButton button in sequence)
                {
                    // Button_Click(button, new RoutedEventArgs());
                    Uri temp = button.NormalStateImageUriSource;
                    button.NormalStateImageUriSource = button.PressedStateImageUriSource;
                    button.HoverStateImageUriSource = button.PressedStateImageUriSource;
                    int place = allButtons.IndexOf(button);
                    Play_Sound(allSounds[place]);
                    await Task.Delay(500); // length of simon's button click
                    button.NormalStateImageUriSource = temp;
                    button.HoverStateImageUriSource = temp;
                    await Task.Delay(50); // time between buttons, you must seperate button clicks on same one twice!!
                }
                // re enable all the buttons for Player's Turn
                foreach (ImageButton button in allButtons)
                {
                    button.IsHitTestVisible = true;
                }
                Hint.IsHitTestVisible = true;
                SixButtons.IsHitTestVisible = true;
                ImagePickerFlyout.IsHitTestVisible = true;
                SoundPickerFlyout.IsHitTestVisible = true;
                Restart.IsHitTestVisible = true;

                playerTurn = true;
                StateText.Text = "Your Turn";
            } // end Simon's turn


        } // end game
        private void Play_Sound(MediaElement sound)
        {
            sound.Position = new TimeSpan(0);
            sound.Play();
        }

        private void AdjustSounds(string newSound = "churchbell")
        {
            buttonSound1.Source = new Uri("ms-appx:///Sounds/" + newSound + "/Red.mp3");
            buttonSound2.Source = new Uri("ms-appx:///Sounds/" + newSound + "/Blue.mp3");
            buttonSound3.Source = new Uri("ms-appx:///Sounds/" + newSound + "/Orange.mp3");
            buttonSound4.Source = new Uri("ms-appx:///Sounds/" + newSound + "/Purple.mp3");
            buttonSound5.Source = new Uri("ms-appx:///Sounds/" + newSound + "/Green.mp3");
            buttonSound6.Source = new Uri("ms-appx:///Sounds/" + newSound + "/Yellow.mp3");

        }

        private void AdjustImages(string newImage = "Flat")
        {
            foreach (ImageButton button in allButtons)
            {
                button.NormalStateImageUriSource = new Uri("ms-appx:///Images/" + newImage + "/Normal/" + button.Name + ".png");
                button.HoverStateImageUriSource = new Uri("ms-appx:///Images/" + newImage + "/Normal/" + button.Name + ".png");
                button.PressedStateImageUriSource = new Uri("ms-appx:///Images/" + newImage + "/Pressed/" + button.Name + ".png");
            }
        }

        private void SixButtons_Click(object sender, RoutedEventArgs e)
        {
            switch (numberOfButtons)
            {
                case 4:
                    {
                        numberOfButtons = 6;
                        SixButtons.Content = "4 Buttons";
                        MiddleButtons.Height = new GridLength(1, GridUnitType.Star);
                        break;
                    }
                case 6:
                    {
                        numberOfButtons = 4;
                        SixButtons.Content = "6 Buttons";
                        MiddleButtons.Height = new GridLength(0, GridUnitType.Star);
                        break;
                    }
            }

        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Play_Sound(allSounds[allButtons.IndexOf(sender as ImageButton)]);
            if ((sender as ImageButton) == sequence.ElementAt(playerSequence))
            {
                playerSequence++;
            }
            else
            {
                StateText.Text = "Game Over";
                gameOver = true;
                SimonsTurn();

            }
            if (playerSequence >= (sequence.Count()))
            {
                playerSequence = 0;
                playerTurn = false;
                StateText.Text = "Simon's Turn";
                SimonsTurn();
            }
            // int place = allButtons.IndexOf(sender as ImageButton);


        }

        private void Hint_Click(object sender, RoutedEventArgs e)
        {
            currentSound = "Meow";
            AdjustSounds(currentSound);
        }



        // Custom Dialogs

        private async void AreYouReady()
        {
            ContentDialog readyDialog = new ContentDialog()
            {
                Title = "Are You Ready?",
                Content = "Press OK to begin. This is a CONTENT dialog",
                PrimaryButtonText = "Ok"
            };

            await readyDialog.ShowAsync();
            
            SimonsTurn();

        }

        private async Task<bool> GameOver()
        {
            string title = "Dang!  You were doing so well!";
            // TODO check for high score, if so allow name entry
            TopScore scoreToBeat = topTen.OrderBy(t => t.Round).ThenBy(t => t.Level).FirstOrDefault();
            //TODO change formula. Override .equals??  count>sTB OR count=sTB and PS>sTB
            

            if (topTen.Count < 10)
            {
                title = "Hey! You got a high score!";
                topTen.Add(new TopScore("My Name", sequence.Count, playerSequence));
            }
            else if ((sequence.Count > scoreToBeat.Round || (sequence.Count == scoreToBeat.Round && playerSequence > scoreToBeat.Level)))
            {
                // TODO add popup for name
                title = "Hey! You got a high score!";
                topTen.Add(new TopScore("My Name", sequence.Count, playerSequence));
                topTen.Remove(scoreToBeat);
            }

            ContentDialog gameOverDialog = new ContentDialog()
            {
                Title = title,
                Content = "You got " + playerSequence + " right on Round " + sequence.Count + "!",
                PrimaryButtonText = "Start Over",
                SecondaryButtonText = "Not just now"
            };


            ContentDialogResult result = await gameOverDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                sequence.Clear();
                playerSequence = 0;
                playerTurn = false;
                gameOver = false;

                return true;
            }
            return false;
        }

        private void SoundPicker_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            currentSound = rb.Content.ToString();
            AdjustSounds(currentSound);
        }

        private void ImagePicker_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            currentImage = rb.Content.ToString();
            AdjustImages(currentImage);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(About));
        }

        private void TopTen_Click(object sender, RoutedEventArgs e)
        {
            IList<TopScore> displayList = topTen;
            displayList = displayList.OrderByDescending(l => l.Level).ThenByDescending(l => l.Round).Take(10).ToList();
            listView.ItemsSource = displayList;
            if (!TopTen.IsOpen) { TopTen.IsOpen = true; }
        }

        private void RestartGame(object sender, RoutedEventArgs e)
        {
            sequence.Clear();
            playerSequence = 0;
            playerTurn = false;
            gameOver = false;
            SimonsTurn();
        }
    }
}
