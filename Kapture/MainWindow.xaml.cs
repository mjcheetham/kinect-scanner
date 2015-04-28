using Ionic.Zip;
using KaptureLibrary.IO;
using KaptureLibrary.Kinect;
using KaptureLibrary.Processing;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kapture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<UIRecording> data;
        private SensorManager manager;
        private bool programBusy;
        private CancellationTokenSource cancelProcessor;

        public MainWindow()
        {
            InitializeComponent();

            // create sensor manager
            this.manager = new SensorManager(DepthFormat.HighRes30Fps, ColourFormat.HighRes30Fps);
            this.programBusy = false;

            // set recordings collection as data context
            this.data = new ObservableCollection<UIRecording>();
            this.DataContext = data;

            // set exit application command
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand,
                (o, e) => { Application.Current.Shutdown(); }
                ));
        }

        private async void NewRecording(object sender, ExecutedRoutedEventArgs e)
        {
            // Ask for duration
            var c0 = new NewRecordingPrompt();
            c0.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            c0.Owner = this;
            c0.ShowDialog();
            if (!c0.DialogResult.Value) return;
            int durationms = c0.Duration * 1000;


            // Setup file directories
            var tmpBase = Path.GetTempPath();
            var calibrationPath = tmpBase + "kapture.cal";
            var mappingParamsPath = tmpBase + "kapture.map";
            var timestampPath = tmpBase + "kapture.time";
            var dPath = tmpBase + "kapture.z";
            var cPath = tmpBase + "kapture.rgb";

            // Ask to calibrate
            var c1 = new CalibrateCubes(manager);
            var c2 = new CalibrateTable(manager);
            c1.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            c1.Owner = this;
            c2.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            c2.Owner = this;

            c1.ShowDialog();
            if (!c1.DialogResult.Value) return;
            c2.ShowDialog();
            if (!c2.DialogResult.Value) return;

            var markers = c1.Markers;
            var search = c2.MarkerSearchSpace;

            await this.manager.SaveCalibrationToDiskAsync(calibrationPath, search, markers);

            // Record
            var recordingProgress = new Progress<long>((t) =>
                {
                    var span = TimeSpan.FromMilliseconds(t);
                    this.SimpleStatusText.Text = "Recording: T+" + span.TotalSeconds + " s";
                });
            await this.manager.CaptureToDiskAsync(mappingParamsPath, timestampPath, dPath, cPath, durationms, recordingProgress);


            // Configure save file dialog box
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".krec";
            dlg.Filter = "Kapture recording|*.krec";

            // Show save file dialog box
            var result = dlg.ShowDialog();

            // Process save file dialog box results 
            if (result == true)
            {

                // Transfer to archive
                var zip = new ZipFile(dlg.FileName);
                zip.SaveProgress += (sndr, args) =>
                {
                    // must update status on UI thread (Ionic.Zip doesn't support async!)
                    Dispatcher.Invoke(() => { this.SimpleStatusText.Text = "Compressing..."; });
                };
                await Task.Run(() =>
                {
                    using (zip)
                    {
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                        zip.AddItem(calibrationPath, "");
                        zip.AddItem(mappingParamsPath, "");
                        zip.AddItem(timestampPath, "");
                        zip.AddItem(dPath, "");
                        zip.AddItem(cPath, "");
                        zip.Save();
                    }
                });

                this.SimpleStatusText.Text = "Recording saved.";
            }
        }

        private async void OpenRecording(object sender, ExecutedRoutedEventArgs e)
        {
            // Configure open file dialog box
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ".krec";
            dlg.Filter = "Kapture recording|*.krec";

            // Show open file dialog box
            var result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Extract files from archive
                var tmpBase = Path.GetTempPath();
                var tmpDir = Path.GetRandomFileName();
                var extractPath = tmpBase + tmpDir + "\\";

                var zip = ZipFile.Read(dlg.FileName);
                zip.ExtractProgress += (sndr, args) =>
                {
                    // must update status on UI thread (Ionic.Zip doesn't support async!)
                    Dispatcher.Invoke(() => { this.SimpleStatusText.Text = "Decompressing..."; });
                };
                await Task.Run(() =>
                    {
                        using (zip)
                        {
                            zip.ExtractAll(tmpBase + tmpDir);
                        }
                    });

                this.SimpleStatusText.Text = "Done.";

                // Setup file paths
                var calibrationPath = extractPath + "kapture.cal";
                var mappingParamsPath = extractPath + "kapture.map";
                var timestampPath = extractPath + "kapture.time";
                var dPath = extractPath + "kapture.z";
                var cPath = extractPath + "kapture.rgb";

                // Create recording from files
                var r = new Recording(mappingParamsPath, timestampPath,
                    dPath, DepthFormat.HighRes30Fps,
                    cPath, ColourFormat.HighRes30Fps,
                    calibrationPath);

                // Add to UI data model
                this.data.Add(new UIRecording(r));
            }
        }

        // TODO: show preferences
        private void ShowPreferences(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("This is a work in progress. To make changes instead you must directly edit the settings files in the KaptureLibrary project at the moment.", "Todo");
        }

        private void ToggleSensor(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.manager.Connected)
            {
                this.manager.Disconnect();
                this.SimpleStatusText.Text = "Disconnected.";
            }
            else
            {
                var result = this.manager.Connect();
                this.SimpleStatusText.Text = result ? "Connected." : "Couldn't connect to a Kinect sensor.";
            }
            this.ToggleSensorMenuItem.Header = this.manager.Connected ? "Disconnect" : "Connect";
        }

        private void IsSensorConnected(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.manager.Connected;
        }

        // TODO: show sensor information
        private void ShowSensorInformation(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("This is a work in progress. Not sure what information would be useful here.", "Todo");
        }

        private void SaveSelectedRecording(object sender, ExecutedRoutedEventArgs e)
        {
            var r = (UIRecording)(this.RecordingList.SelectedItem);

            // Configure save file dialog box
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".ply";
            dlg.Filter = "Point cloud|*.ply";

            // Show save file dialog box
            var result = dlg.ShowDialog();

            // Process save file dialog box results 
            if (result == true)
            {
                // Save document 
                PointCloudWriter.WritePLY(dlg.FileName, r.ProcessedCloud);
                this.SimpleStatusText.Text = "Point cloud saved.";
            }
        }

        private void CanSaveRecording(object sender, CanExecuteRoutedEventArgs e)
        {
            var r = (UIRecording)this.RecordingList.SelectedItem;
            e.CanExecute = (r != null) && (r.ProcessingStatus == ProcessingStatus.Complete);
        }

        private void RecordingList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (sender as ListBox).SelectedItem = null;
        }

        private async void ProcessRecording(object sender, ExecutedRoutedEventArgs e)
        {
            var r = (UIRecording)(this.RecordingList.SelectedItem);

            this.programBusy = true;
            this.SimpleStatusText.Text = "Processing...";
            r.ProcessingStatus = ProcessingStatus.Busy;

            var processingProgress = new Progress<int>((i) =>
                {
                    double pcnt = (double)i / r.Recording.NumberOfFrames;
                    this.ProcessingProgressBar.Value = pcnt;
                    this.ProcessingProgressBarText.Text = (int)(pcnt * 100) + "%";
                });
            this.cancelProcessor = new CancellationTokenSource();

            try
            {
                var cloud = await Processor.GenerateObject(r.Recording, processingProgress, this.cancelProcessor.Token);
                r.ProcessedCloud = cloud;

                r.ProcessingStatus = ProcessingStatus.Complete;
                this.SimpleStatusText.Text = "Processing complete.";
            }
            catch (OperationCanceledException)
            {
                this.SimpleStatusText.Text = "Processing cancelled by user.";
                r.ProcessingStatus = ProcessingStatus.Idle;
            }
            finally
            {
                this.ProcessingProgressBar.Value = 0;
                this.ProcessingProgressBarText.Text = "Idle";
                this.cancelProcessor = null;
                this.programBusy = false;
            }
        }

        private void CanProcessRecording(object sender, CanExecuteRoutedEventArgs e)
        {
            var r = (UIRecording)(this.RecordingList.SelectedItem);
            e.CanExecute = !this.programBusy && (r != null) && (r.ProcessingStatus == ProcessingStatus.Idle);
        }

        private void CloseRecording(object sender, ExecutedRoutedEventArgs e)
        {
            var r = (UIRecording)this.RecordingList.SelectedItem;
            r.Recording.Dispose();
            this.data.Remove(r);
        }

        private void RecordingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.FrameSlider.IsEnabled = (sender as ListBox).SelectedItem != null;
        }

        private void IsRecordingSelected(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.RecordingList.SelectedItem != null;
        }

        private void CancelProcessing(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.cancelProcessor != null)
                this.cancelProcessor.Cancel();
        }

        private void CanCancelProcessing(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (this.cancelProcessor != null) && (this.programBusy);
        }

    }

}
