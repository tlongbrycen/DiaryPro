using DiaryPro.Models;
using DiaryPro.NavigationParams;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DiaryPro
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NotePage : Page
    {
        public NotePage()
        {
            this.InitializeComponent();
        }

        private static readonly int MAX_RECORD_PER_PAGE = 10;

        private static readonly string TMP_NOTE_FILE_NAME = "tmp_note.dat";

        private ObservableCollection<NoteModel> noteModelCollection;

        private int selectedIndex = -1;

        private StorageFile tmpNoteFile;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                NavParamBase navParam = (NavParamBase)e.Parameter;
                if (navParam.SourcePage.Equals(typeof(HomePage)) &&
                    navParam.TargetPage.Equals(typeof(NotePage)))
                {
                    string request = ((NavParamHomeToNote)navParam).ExtraCommand;
                    noteModelCollection = DataAccessModel.GetData(MAX_RECORD_PER_PAGE, 0);
                    tbHeader.FontSize = sldrHeader.Value;
                    tbContent.FontSize = sldrContent.Value;
                    if (noteModelCollection.Count > 0)
                    {
                        tbHeader.Text = noteModelCollection[0].header;
                        tbContent.Text = noteModelCollection[0].content;
                    }
                }
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            NoteModel note = new NoteModel();
            note.header = "Header";
            note.content = "Content";
            note.date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            note.ID = DataAccessModel.AddData(note);
            noteModelCollection.Insert(0, note);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataAccessModel.DeleteData(noteModelCollection[selectedIndex].ID);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {

        }

        private void sldrHeader_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //UWP slider scale does not work well so this constraint is needed
            if(sldrHeader.Value > sldrHeader.Maximum)
            {
                sldrHeader.Value = sldrHeader.Maximum;
            }
            else if(sldrHeader.Value < sldrHeader.Minimum)
            {
                sldrHeader.Value = sldrHeader.Minimum;
            }
            tbHeader.FontSize = sldrHeader.Value;
        }

        private void sldrContent_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //UWP slider scale does not work well so this constraint is needed
            if (sldrContent.Value > sldrContent.Maximum)
            {
                sldrContent.Value = sldrContent.Maximum;
            }
            else if (sldrContent.Value < sldrContent.Minimum)
            {
                sldrContent.Value = sldrContent.Minimum;
            }
            tbContent.FontSize = sldrContent.Value;
        }

        private void listViewNote_ItemClick(object sender, ItemClickEventArgs e)
        {
            ListView listView = (ListView)sender;
            var clickedMenuItem = (NoteModel)e.ClickedItem;
            selectedIndex = listView.Items.IndexOf(clickedMenuItem);
            tbHeader.Text = noteModelCollection[selectedIndex].header;
            tbContent.Text = noteModelCollection[selectedIndex].content;
        }

        private void tbHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void btnImgUp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnImgDown_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnImgAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnImgRemove_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
