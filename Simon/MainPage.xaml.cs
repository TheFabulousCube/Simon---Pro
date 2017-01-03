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
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using Microsoft.Graphics.Canvas.Text;
using Windows.UI;
using System.Reflection;
using Windows.UI.Core;
using System.Net.NetworkInformation;

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
        const int arrayOffset = 1;

        // Game variables
        bool playerTurn = false;
        private bool gameOver = false;
        private int randNumber;

        private Hint hints = new Hint(HintStartingNumber);
        public string HintDisplay
        {
            get { return ("Hints\n" + hints.NumberOfHints); }
        }
        public bool HintsEnabled
        {
            get { return (hints.HintsAvailable()); }
        }
        private int numberOfButtons = 6;
        private int playerSequence = 0;

        // Visual Elements
        //public static string currentImage = "Flat";
        //public static string currentSound = "Churchbell";
        public static string currentFont = "Determination Mono";
        public static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        StorageFolder roamingFolder = ApplicationData.Current.RoamingFolder;


        // Helper arrays
        List<ImageButton> allButtons;
        List<ImageButton> sequence;
        List<MediaElement> allSounds;

        // Top Scores
        public static string name;
        public static DateTime last100Update;
        public static IList<TopScore> topTen = new List<TopScore>();
        public static IList<TopScoreEntity> top100 = new List<TopScoreEntity>();
        public readonly int numberFromTop100 = 100;

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this;
            allButtons = new List<ImageButton> { Red, Blue, Green, Yellow, Orange, Purple };
            sequence = new List<ImageButton>();
            allSounds = new List<MediaElement> { buttonSound1, buttonSound2, buttonSound3, buttonSound4, buttonSound5, buttonSound6 };
            if (roamingSettings != null) LoadSettings();
            AdjustSounds(currentSound);
            AdjustImages(currentImage);
            AreYouReady();


        } // end MainPage

        private async void LoadSettings()
        {
            ///****************************************************/

            //// Create the table if it doesn't exist.
            //TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Settings"));
            ///****************************************************/

            currentSound = (roamingSettings.Values["savedSound"] != null) ? roamingSettings.Values["savedSound"].ToString() : "Churchbell";

            currentImage = (roamingSettings.Values["savedImage"] != null) ? roamingSettings.Values["savedImage"].ToString() : "Flat" ;
 
            currentFont = roamingSettings.Values["savedFont"] != null ? roamingSettings.Values["savedFont"].ToString() : currentFont;
            page.FontFamily = new FontFamily(currentFont);
            name = roamingSettings.Values["savedName"] != null ? roamingSettings.Values["savedName"].ToString() : name;
            topTen = LoadTopTen();
            top100 = await LoadTop100();

            }

        private IList<TopScore> LoadTopTen()
        {
            IList<TopScore> list = new List<TopScore>();
            ApplicationDataCompositeValue storedScore = (ApplicationDataCompositeValue)roamingSettings.Values["allScores"];
            bool EOF = false;
            int i = 0;
            /************************  Top 10 from Roaming   ***********************************/
            while (!EOF)
            {
                if (storedScore == null || storedScore["Name" + i] == null)
                {
                    EOF = true;
                }
                else
                {
                    // Top scores are stored as an array, storedScore
                    // each score is comprised of name, round, & level
                    string _name = (string)storedScore["Name" + i];
                    int _level = (int)storedScore["Level" + i];
                    int _round = (int)storedScore["Round" + i];
                    int _buttons = (int)storedScore["Buttons" + i];
                    string _device = (string)storedScore["Device" + i];
                    TopScore temp = new TopScore(_name, _level, _round, _buttons, _device);
                    list.Add(temp);
                    i += 1;
                }
            
            }
            return list;
       }

        private async Task<IList<TopScoreEntity>> LoadTop100()
        {
            progress.IsActive = true;
            progress.Visibility = Visibility.Visible;
            IList <TopScoreEntity> list = new List<TopScoreEntity>();
            bool EOF = false;
            int i = 1;
            if (NetworkInterface.GetIsNetworkAvailable())
            {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = App.account;

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Top100");

            // Create the table if it doesn't exist.
            await table.CreateIfNotExistsAsync();
              while (!EOF)
                {
                    TableOperation retrieveOperation = TableOperation.Retrieve<TopScoreEntity>("TopScores", i.ToString());
                    TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
                    if (retrievedResult.Result == null)
                    {
                        EOF = true;
                    }
                    else
                    {
                        // Top scores are stored as an array, storedScore
                        // each score is comprised of name, round, & level
                        
                        list.Add((TopScoreEntity)retrievedResult.Result);
                        i += 1;
                    }
                }
            }

            progress.Visibility = Visibility.Collapsed;
            progress.IsActive = false;
            return list;


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
                StateText.Text = "Simon's Turn";
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
                if ((sequence.Count % HintFrequency == 1) && (sequence.Count / HintFrequency > 0))
                {
                    hints.EarnAHint();
                    Hint.Content = HintDisplay;
                }
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
                    await Task.Delay(500 - (place * 5)); // length of simon's button click
                    button.NormalStateImageUriSource = temp;
                    button.HoverStateImageUriSource = temp;
                    await Task.Delay(50 ); // time between buttons, you must seperate button clicks on same one twice!!
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
                        SixButtons.Content = " Four\nButtons";
                        MiddleButtons.Visibility = Visibility.Visible;
                        break;
                    }
                case 6:
                    {
                        numberOfButtons = 4;
                        SixButtons.Content = " Six\nButtons";
                        if (sequence.Count > 0)
                        {
                            sequence.RemoveAll(s => s.Name.Equals("Orange") || s.Name.Equals("Purple"));

                        }
                        MiddleButtons.Visibility = Visibility.Collapsed;
                        if (sequence.Count <= 0)
                        {
                            ResetGame();
                            StateText.Text = "Simon's Turn";
                            SimonsTurn();
                        }
                        break;
                    }
            }

        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ImageButton tempBtn = sender as ImageButton;
            Play_Sound(allSounds[allButtons.IndexOf(tempBtn)]);

            tempBtn.NormalStateImageUriSource = new Uri("ms-appx:///Images/" + currentImage + "/Normal/" + tempBtn.Name + ".png");
            tempBtn.HoverStateImageUriSource = new Uri("ms-appx:///Images/" + currentImage + "/Normal/" + tempBtn.Name + ".png");

            if ((sequence.Count > 0) && (sender as ImageButton) == sequence.ElementAt(playerSequence))
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
            ImageButton ib = sequence.ElementAt(playerSequence);
            hints.UseAHint(ib);
            Hint.Content = HintDisplay;
        }



        // Custom Dialogs

        private async void AreYouReady()
        {
            ContentDialog readyDialog = new ContentDialog()
            {
                Title = "Are You Ready?",
                Content = "(Press OK to begin.)",
                PrimaryButtonText = "Ok"
            };

            await readyDialog.ShowAsync();

            SimonsTurn();

        }

        private async Task<bool> GameOver()
        {

            // TODO check for high score, if so allow name entry
            TopScore scoreToBeat = (topTen.Count < 10) ? new TopScore(null, 0, 0, 4) : topTen.OrderBy(t => t.Level).ThenBy(t => t.Round).FirstOrDefault();
            var findOne = top100.OrderBy(t => t.Level).ThenBy(t => t.Round).FirstOrDefault();
            TopScoreEntity _100ToBeat = (top100.Count < numberFromTop100) ? new TopScoreEntity((top100.Count + arrayOffset).ToString(), 0, 0, 4) : top100.OrderByDescending(t => t.Level).ThenByDescending(t => t.Round).Take(numberFromTop100).LastOrDefault();
            TopScore playerScore = new TopScore(name, sequence.Count, playerSequence, numberOfButtons);
            TopScoreEntity playerScoreEntity = new TopScoreEntity(name, sequence.Count, playerSequence, numberOfButtons);

            string title = "Dang!  You were doing so well!";
            TextBlock message = new TextBlock(){ Text = "You got " + playerScore.Round + " right on Round " + playerScore.Level + "!" };
            TextBox nameBox = new TextBox() { Text = (name != null) ? name : ""};

            ContentDialog gameOverDialog = new ContentDialog()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Start Over"
            };

            var panel = new StackPanel();
            panel.Children.Add(message);
            panel.Children.Add(nameBox);

            if (playerScore.Beats(scoreToBeat))
            {
                message.Text += "\nYou got a High Score!";

                // TODO add name box?
                gameOverDialog.Content = panel;
                gameOverDialog.PrimaryButtonText = "Save and Start Over";
                gameOverDialog.PrimaryButtonClick += delegate
                {
                    name = nameBox.Text;
                    roamingSettings.Values["savedName"] = name;
                    playerScore.Name = name;
                    topTen.Add(playerScore);
                    SaveLocalScores();
                };

            }

            if (playerScoreEntity.Beats(_100ToBeat))
            {
                message.Text +=  "\nYou got top 100!";

                // TODO add name box?
                gameOverDialog.Content = panel;
                gameOverDialog.PrimaryButtonText = "Save and Start Over";
                gameOverDialog.PrimaryButtonClick += delegate
                {
                    name = nameBox.Text;
                    roamingSettings.Values["savedName"] = name;
                    playerScoreEntity.Name = name;
                    playerScoreEntity.RowKey = _100ToBeat.RowKey;
                    top100.Add(playerScoreEntity);
                    SaveThisGlobalScore(playerScoreEntity);
                };

            }
            /*******************************************************/
            ContentDialogResult result = await gameOverDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ResetGame();
                return true;
            }
            
            return true;
        }

        private static void SaveLocalScores()
        {
            /****************************************************************************************/
            /*******************        Save high scores to Roaming        **************************/
            ApplicationDataCompositeValue storedScore = new ApplicationDataCompositeValue(); // roaming settings
            for (int i = 0; i < ((topTen.Count <= 10)?(topTen.Count):(11)); i++)
            {
                // Top scores are stored as an array, storedScore
                // each score is comprised of name, round, level, # of buttons, & device type
                TopScore temp = topTen.ElementAt(i);
                storedScore["Name" + i] = temp.Name;
                storedScore["Level" + i] = temp.Level;
                storedScore["Round" + i] = temp.Round;
                storedScore["Buttons" + i] = temp.Buttons;
                storedScore["Device" + i] = temp.Device;
            }
            roamingSettings.Values["allScores"] = storedScore;
            /****************************************************************************************/

        }

        private async void SaveThisGlobalScore(TopScoreEntity thisScore)
        {
            /*************************************************************************************/
            /***************   Insert or Update this score to cloud   ****************************/

            progress.IsActive = true;
            progress.Visibility = Visibility.Visible;
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                // no internet connection
                // TODO show dialog, wait for answer
                ContentDialog noInternet = new ContentDialog()
                {
                    Title = "Internet connection required",
                    Content = "Please connect to the internet",
                    PrimaryButtonText = "Try Again",
                    SecondaryButtonText = "Skip it"
                };

                ContentDialogResult result; // = await noInternet.ShowAsync();

                do
                {
                    result = await noInternet.ShowAsync();
                    if (result == ContentDialogResult.Secondary)
                    {
                        progress.Visibility = Visibility.Collapsed;
                        progress.IsActive = false;
                        return;
                    }
                }
                while ((!NetworkInterface.GetIsNetworkAvailable() && result == ContentDialogResult.Primary));
                switch (result)
                {
                    case (ContentDialogResult.Primary):
                        {
                            //    return to start and try again
                            break;
                        }
                    case (ContentDialogResult.Secondary):
                        {
                            // just skip it.
                            // watch this, if name is entered this could act funny
                            progress.Visibility = Visibility.Collapsed;
                            progress.IsActive = false;
                            return;
                        }
                    default:
                        {
                            progress.Visibility = Visibility.Collapsed;
                            progress.IsActive = false;
                            return;
                        }
                }
            }
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = App.account;

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Top100");

            // Create the table if it doesn't exist.
            await table.CreateIfNotExistsAsync();
            TableOperation insertOperation = TableOperation.InsertOrReplace(thisScore);
            await table.ExecuteAsync(insertOperation);
            progress.Visibility = Visibility.Collapsed;
            progress.IsActive = false;
        }

 
        private static void SaveGlobalScores()
        {
            /****************************************************************************************/
            /*******************        Save high scores to Cloud        **************************/
            TableBatchOperation saveToCloud = new TableBatchOperation(); // save to cloud


            //table.ExecuteBatchAsync(saveToCloud);
            /****************************************************************************************/
        }

        private void SoundPicker_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            currentSound = rb.Content.ToString();
            AdjustSounds(currentSound);
            roamingSettings.Values["savedSound"] = currentSound;
        }

        private void ImagePicker_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            currentImage = rb.Content.ToString();
            AdjustImages(currentImage);
            roamingSettings.Values["savedImage"] = currentImage;
        }

 

        private void About_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(About));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame.CanGoBack)
            {
                // Show UI in title bar if opted-in and in-app backstack is not empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
            }
            else
            {
                // Remove the UI from the title bar if in-app back stack is empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }
        }
        private void TopTen_Click(object sender, RoutedEventArgs e)
        {
            IList<TopScore> displayList = topTen;
            // TODO make use of TopTenPopup class 
            displayList = displayList.OrderByDescending(l => l.Level).ThenByDescending(l => l.Round).Take(10).ToList();
            listView.ItemsSource = displayList;
            ResizePopup();
            if (!TopTen.IsOpen) { TopTen.IsOpen = true; }
        }

        private void ResizePopup()
        {
            // resize popup            
            double width = InnerFrame.ActualWidth * 1.2;
            double height = InnerFrame.ActualHeight * 1.3;

            listView.Width = width;
            listView.Height = height;

            var windwidth = page.ActualWidth;
            var windheight = page.ActualHeight;

            double hOffset = (OuterFrame.ActualWidth - width)/2;
            double vOffset = (OuterFrame.ActualHeight - height)/4;

            TopTen.HorizontalOffset = hOffset;
            TopTen.VerticalOffset = vOffset;
        }


        private async void Top100_Click(object sender, RoutedEventArgs e)
        {
            /*************************************************************************************/
            /***********************   Top 100 from Cloud   **************************************/
            
            progress.IsActive = true;
            progress.Visibility = Visibility.Visible;
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                // no internet connection
                // TODO show dialog, wait for answer
                ContentDialog noInternet = new ContentDialog()
                {
                    Title = "Internet connection required",
                    Content = "Please connect to the internet",
                    PrimaryButtonText = "Try Again",
                    SecondaryButtonText = "Skip it"
                };

                ContentDialogResult result; // = await noInternet.ShowAsync();

                do
                {
                    result = await noInternet.ShowAsync();
                    if (result == ContentDialogResult.Secondary)
                    {
                        progress.Visibility = Visibility.Collapsed;
                        progress.IsActive = false;
                        return;
                    }
                }
                while ((!NetworkInterface.GetIsNetworkAvailable() && result == ContentDialogResult.Primary));
                switch (result)
                {
                    case (ContentDialogResult.Primary):
                        {
                            //if (!NetworkInterface.GetIsNetworkAvailable())
                            //    result = await noInternet.ShowAsync();
                            break;
                        }
                    case (ContentDialogResult.Secondary):
                        {
                            progress.Visibility = Visibility.Collapsed;
                            progress.IsActive = false;
                            return;
                        }
                    default:
                        {
                            progress.Visibility = Visibility.Collapsed;
                            progress.IsActive = false;
                            return;
                        }
                }
            }
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = App.account;

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Top100");

            // Create the table if it doesn't exist.
            await table.CreateIfNotExistsAsync();

            top100.Clear();
            bool EOF = false;
            int i = 1;
            while (!EOF)
            {
            TableOperation retrieveOperation = TableOperation.Retrieve<TopScoreEntity>("TopScores", i.ToString());
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
                if (retrievedResult.Result == null)
                {
                    EOF = true;
                }
                else
                {
                    // Top scores are stored as an array, storedScore
                    // each score is comprised of name, round, & level

                    TopScoreEntity temp = new TopScoreEntity();
                    temp = (TopScoreEntity)retrievedResult.Result;
                    top100.Add(temp);
                    i += 1;
                }
            }

            IList<TopScoreEntity> displayList = top100;
            // TODO make use of TopTenPopup class 
            displayList = displayList.OrderByDescending(l => l.Level).ThenByDescending(l => l.Round).Take(numberFromTop100).ToList();

            listView.ItemsSource = displayList;
            ResizePopup();
            progress.IsActive = false;
            progress.Visibility = Visibility.Collapsed;
            if (!TopTen.IsOpen) { TopTen.IsOpen = true; }
        }

        private void ResetGame()
        {
            sequence.Clear();
            playerSequence = 0;
            hints.NumberOfHints = HintStartingNumber;
            Hint.Content = HintDisplay;
            playerTurn = false;
            gameOver = false;
        }

        private void RestartGame(object sender, RoutedEventArgs e)
        {
            ResetGame();
            SimonsTurn();
        }

        private void Font_Click(object sender, RoutedEventArgs e)
        {
            List<FontClass> fList = new List<FontClass>();
            FontClass curFont = (new FontClass() { MyFontFamily = new FontFamily(currentFont), FontFamilyValue = currentFont });
            string[] fontList = CanvasTextFormat.GetSystemFontFamilies();
            foreach (var font in fontList)
            {
                fList.Add(new FontClass() { MyFontFamily = new FontFamily(font), FontFamilyValue = font });
            }
            FontComboBox.ItemsSource = fList;
            FontComboBox.SelectedIndex = fList.IndexOf(curFont);
            SampleText.FontFamily = curFont.MyFontFamily;
            FontPicker.IsOpen = true;
        }

        private void Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FontClass MyFont = (FontClass)FontComboBox.SelectedItem;
            if (MyFont == null)
            {
                MyFont = (new FontClass() { MyFontFamily = new FontFamily(currentFont), FontFamilyValue = currentFont });
            }
            SampleText.FontFamily = MyFont.MyFontFamily;

        }

        private void SaveFont_Click(object sender, RoutedEventArgs e)
        {

            page.FontFamily = SampleText.FontFamily;
            currentFont = SampleText.FontFamily.Source;

            roamingSettings.Values["savedFont"] = currentFont;
            FontPicker.IsOpen = false;
        }

        private void RestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            // reset all variables
            currentImage = "Flat";
            currentSound = "Churchbell";
            currentFont = "Determination Mono";

            // update them
            AdjustSounds(currentSound);
            AdjustImages(currentImage);
            page.FontFamily = new FontFamily(currentFont);

            // save to roaming
            roamingSettings.Values["savedImage"] = currentImage;
            roamingSettings.Values["savedSound"] = currentSound;
            roamingSettings.Values["savedFont"] = currentFont;

        }

        private void ClearTopTen_Click(object sender, RoutedEventArgs e)
        {
            topTen = new List<TopScore>();
            SaveLocalScores();
        }

        private async void EnterName_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog enterYourName = new ContentDialog()
            {
                Title = "You can store your name for when you gat a high score",
                PrimaryButtonText = "Save"
            };

            TextBox nameBox = new TextBox() { Text = (name != null) ? name : "" };

            var message = new TextBlock { Text = "What name do you want to save?" };

            var panel = new StackPanel();
            panel.Children.Add(message);
            panel.Children.Add(nameBox);
            enterYourName.Content = panel;

            ContentDialogResult nameResult = await enterYourName.ShowAsync();
            if (nameResult == ContentDialogResult.Primary)
            {
                name = nameBox.Text;
                roamingSettings.Values["savedName"] = name;
            }
        }

    }

}
