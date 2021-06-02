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
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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

        private ObservableCollection<NoteModel> noteModelCollection;

        private int selectedNoteIndex = -1;

        private int selectedImgIndex = -1;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                NavParamBase navParam = (NavParamBase)e.Parameter;
                if (navParam.SourcePage.Equals(typeof(HomePage)) &&
                    navParam.TargetPage.Equals(typeof(NotePage)))
                {
                    string request = ((NavParamHomeToNote)navParam).ExtraCommand;
                    //noteModelCollection = DataAccessModel.GetAllData();
                    noteModelCollection = DataAccessModel.GetData(MAX_RECORD_PER_PAGE, 0);
                    tbHeader.FontSize = sldrHeader.Value;
                    tbContent.FontSize = sldrContent.Value;
                    if (noteModelCollection.Count > 0)
                    {
                        tbHeader.Text = noteModelCollection[0].header;
                        tbContent.Text = noteModelCollection[0].content;
                        listViewNote.SelectedItem = noteModelCollection[0];
                        selectedNoteIndex = 0;
                        if(noteModelCollection[0].images.Count > 0)
                        {
                            imgNote.Source = UtilityModel.BytesToImage(
                                noteModelCollection[0].images[0].img);
                            tbImgDescript.Text = noteModelCollection[0].images[0].descript;
                            selectedImgIndex = 0;
                        }
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
            if (note.ID == -1) return;
            noteModelCollection.Insert(0, note);
            listViewNote.SelectedItem = noteModelCollection[0];
            selectedNoteIndex = 0;
            if (noteModelCollection[selectedNoteIndex].images.Count > 0)
            {
                imgNote.Source = UtilityModel.BytesToImage(
                    noteModelCollection[selectedNoteIndex].images[0].img);
                tbImgDescript.Text = noteModelCollection[selectedNoteIndex].images[0].descript;
                selectedImgIndex = 0;
            }
            else
            {
                imgNote.Source = null;
                tbImgDescript.Text = "";
                selectedImgIndex = -1;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataAccessModel.DeleteData(noteModelCollection[selectedNoteIndex].ID);
            noteModelCollection.RemoveAt(selectedNoteIndex);
            if (selectedNoteIndex > 0)
            {
                selectedNoteIndex = selectedNoteIndex - 1;
                listViewNote.SelectedItem = noteModelCollection[selectedNoteIndex];
            }
            else
            {
                if(noteModelCollection.Count > 0)
                {
                    selectedNoteIndex = 0;
                    listViewNote.SelectedItem = noteModelCollection[selectedNoteIndex];
                }
                else
                {
                    selectedNoteIndex = -1;
                }
            }
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
            selectedNoteIndex = listView.Items.IndexOf(clickedMenuItem);
            tbHeader.Text = noteModelCollection[selectedNoteIndex].header;
            tbContent.Text = noteModelCollection[selectedNoteIndex].content;
            if(noteModelCollection[selectedNoteIndex].images.Count > 0)
            {
                imgNote.Source = UtilityModel.BytesToImage(
                    noteModelCollection[selectedNoteIndex].images[0].img);
                tbImgDescript.Text = noteModelCollection[selectedNoteIndex].images[0].descript;
                selectedImgIndex = 0;
            }
            else
            {
                imgNote.Source = null;
                tbImgDescript.Text = "";
                selectedImgIndex = -1;
            }    
        }

        private void tbHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void btnImgUp_Click(object sender, RoutedEventArgs e)
        {
            if (selectedNoteIndex == -1) return;
            if (selectedImgIndex == -1) return;
            if (selectedImgIndex == 0)
            {
                selectedImgIndex = noteModelCollection[selectedNoteIndex].images.Count - 1;
            }
            else
            {
                selectedImgIndex = selectedImgIndex - 1;
            }
            imgNote.Source = UtilityModel.BytesToImage(
                noteModelCollection[selectedNoteIndex].images[selectedImgIndex].img);
            tbImgDescript.Text = noteModelCollection[selectedNoteIndex].images[selectedImgIndex].descript;
        }

        private void btnImgDown_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnImgAdd_Click(object sender, RoutedEventArgs e)
        {
            if (selectedNoteIndex == -1) return;
            // FilePickerでイメージファイル選択画面を開く
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            StorageFile imgFile = await picker.PickSingleFileAsync();
            if (imgFile == null) return;
            // 選択されたイメージファイルを保存する
            ImgModel img = new ImgModel();
            img.descript = tbImgDescript.Text;
            img.img = await UtilityModel.FileToByteAsync(imgFile);
            var insertID = DataAccessModel.AddData(img, noteModelCollection[selectedNoteIndex].ID);
            if (insertID == -1) return;
            // イメージファイルを表示する
            using (IRandomAccessStream fileStream = await imgFile.OpenAsync(FileAccessMode.Read))
            {
                // Set the image source to the selected bitmap
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                imgNote.Source = bitmapImage;
            }
            //　イメージの選択されているインデックス
            noteModelCollection[selectedNoteIndex].images.Add(img);
            if (noteModelCollection[selectedNoteIndex].images.Count > 0)
            {
                selectedImgIndex = selectedImgIndex + 1;
            }
            else
            {
                selectedImgIndex = 0;
            }
        }

        private void btnImgRemove_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
