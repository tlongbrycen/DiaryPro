using DiaryPro.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DiaryPro
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            //起動時にアプリWindowsサイズ設定
            ApplicationView.PreferredLaunchViewSize = new Size { Height = 768, Width = 1250 };
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            var initDB = Task.Run(async () =>
            {
                await DataAccessModel.InitializeDatabase();
            });
            initDB.Wait();
            Trace.WriteLine("AppFolder: " + ApplicationData.Current.LocalFolder.Path);
            /*var t = Task.Run(async () => {
                await TestDBAsync();
            });
            t.Wait();*/
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// For test DataAccessModel
        /// </summary>
        /// <returns></returns>
        private async Task TestDBAsync()
        {
            //Create a note with dummy data
            NoteModel note = new NoteModel();
            note.header = "header";
            note.content = "content";
            note.date = DateTime.Now.ToString("yyyy/MM/dd HH/mm/ss");
            note.images = new List<ImgModel>();
            StorageFile imgFile = await StorageFile.GetFileFromApplicationUriAsync(
                new Uri("ms-appx:///Assets/icon_note_462x598.png"));
            for (int i = 0; i < 3; i++)
            {
                ImgModel image = new ImgModel();
                image.descript = i.ToString();
                image.img = await UtilityModel.FileToByteAsync(imgFile);
                note.images.Add(image);
            }
            //Try insert the note to DB
            DataAccessModel.AddData(note);
            //Try get the note in DB
            DataAccessModel.GetAllData();
        }
    }
}
