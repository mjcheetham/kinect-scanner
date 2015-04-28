using System.Collections.Generic;
using System.Windows.Input;

namespace Kapture
{
    public static class KaptureCommands
    {

        public static readonly RoutedUICommand Preferences = new RoutedUICommand(
            "Edit preferences",
            "Preferences",
            typeof(MainWindow),
            new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.OemComma, ModifierKeys.Control, "Ctrl+,") }));

        public static readonly RoutedUICommand ToggleSensor = new RoutedUICommand(
            "Connect/disconnect Kinect sensor",
            "ToggleSensor",
            typeof(MainWindow),
            new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.K, ModifierKeys.Control) }));

        public static readonly RoutedUICommand SensorInformation = new RoutedUICommand(
            "Display information about the currently connected sensor",
            "SensorInformation",
            typeof(MainWindow));

        public static readonly RoutedUICommand ProcessRecording = new RoutedUICommand(
            "Process a recording",
            "ProcessRecording",
            typeof(MainWindow));

        public static readonly RoutedUICommand CancelProcessing = new RoutedUICommand(
            "Stop processing a recording",
            "CancelProcessing",
            typeof(MainWindow));
        
        
    }
}
