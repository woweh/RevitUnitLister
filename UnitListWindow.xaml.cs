using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

using Microsoft.Win32;

using Newtonsoft.Json;

namespace RevitUnitLister
{
    /// <summary>
    /// Interaction logic for UnitListWindow.xaml
    /// </summary>
    public partial class UnitListWindow : Window
    {
        private readonly RevitUnitsData _unitsData;
        private readonly RevitUnitsExporter _exporter;

        public UnitListWindow()
        {
            InitializeComponent();
            _exporter = new RevitUnitsExporter();
        }

        public UnitListWindow(RevitUnitsData unitsData) : this()
        {
            _unitsData = unitsData;
            LoadData();
        }

        private void LoadData()
        {
            if (_unitsData == null)
            {
                return;
            }

            // Update statistics
            TxtTotalQuantities.Text = _unitsData.TotalQuantities.ToString();
            TxtTotalUnits.Text = _unitsData.TotalUnits.ToString();

            // Show errors and warnings if any
            if (_unitsData.Errors.Count > 0)
            {
                TxtErrors.Text = $"Errors: {_unitsData.Errors.Count}";
                TxtErrors.Visibility = Visibility.Visible;
                IssuesExpander.Visibility = Visibility.Visible;
            }

            if (_unitsData.Warnings.Count > 0)
            {
                TxtWarnings.Text = $"Warnings: {_unitsData.Warnings.Count}";
                TxtWarnings.Visibility = Visibility.Visible;
                IssuesExpander.Visibility = Visibility.Visible;
            }

            // Show errors by default if there are any issues
            if (_unitsData.Errors.Count > 0 || _unitsData.Warnings.Count > 0)
            {
                ShowErrors();
            }

            // Load quantities list - Sort alphabetically by display name, then by discipline
            // This makes it easier to find quantities (e.g., all "Temperature" entries together)
            LstQuantities.ItemsSource = _unitsData.Quantities
                .OrderBy(q => q.DisplayName)
                .ThenBy(q => q.DisciplineName)
                .ToList();
        }

        private void LstQuantities_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LstQuantities.SelectedItem is QuantityData selectedQuantity)
            {
                TxtSelectedQuantity.Text = $"{selectedQuantity.DisplayName} ({selectedQuantity.DisciplineName})";
                // Convert HashSet to List for DataGrid binding
                GridUnits.ItemsSource = selectedQuantity.Units.ToList();
            }
            else
            {
                TxtSelectedQuantity.Text = "Select a quantity";
                GridUnits.ItemsSource = null;
            }
        }

        private void BtnShowErrors_Click(object sender, RoutedEventArgs e)
        {
            ShowErrors();
        }

        private void BtnShowWarnings_Click(object sender, RoutedEventArgs e)
        {
            ShowWarnings();
        }

        private void ShowErrors()
        {
            LstIssues.ItemsSource = _unitsData.Errors;
            IssuesExpander.Header = $"Issues - Errors ({_unitsData.Errors.Count})";
            if (_unitsData.Errors.Count > 0)
            {
                IssuesExpander.IsExpanded = true;
            }
        }

        private void ShowWarnings()
        {
            LstIssues.ItemsSource = _unitsData.Warnings;
            IssuesExpander.Header = $"Issues - Warnings ({_unitsData.Warnings.Count})";
            if (_unitsData.Warnings.Count > 0)
            {
                IssuesExpander.IsExpanded = true;
            }
        }

        private void BtnCopyIssues_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var issues = new StringBuilder();
                issues.AppendLine($"=== Revit Unit Lister Issues ({DateTime.Now}) ===");
                issues.AppendLine();

                issues.AppendLine($"Errors: {_unitsData.Errors.Count}");
                foreach (string error in _unitsData.Errors)
                {
                    issues.AppendLine($"  - {error}");
                }

                issues.AppendLine();

                issues.AppendLine($"Warnings: {_unitsData.Warnings.Count}");
                foreach (string warning in _unitsData.Warnings)
                {
                    issues.AppendLine($"  - {warning}");
                }

                issues.AppendLine();

                issues.AppendLine($"Summary:");
                issues.AppendLine($"  - Duplicate quantities skipped: {_unitsData.DuplicateQuantitiesSkipped}");
                issues.AppendLine($"  - Failed quantities: {_unitsData.FailedQuantities}");
                issues.AppendLine($"  - Failed units: {_unitsData.FailedUnits}");

                Clipboard.SetText(issues.ToString());
                MessageBox.Show("Issues copied to clipboard!", "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy issues: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnExportJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    DefaultExt = "json",
                    FileName = $"RevitUnits.json",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    OverwritePrompt = true
                };

                if (saveDialog.ShowDialog() == false)
                {
                    return;
                }

                _exporter.ExportToJson(_unitsData, saveDialog.FileName);

                MessageBox.Show(
                    $"Successfully exported {_unitsData.TotalQuantities} quantities with {_unitsData.TotalUnits} units to:\n{saveDialog.FileName}",
                    "Export Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting JSON: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"RevitUnits.csv",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    OverwritePrompt = true
                };

                if (saveDialog.ShowDialog() == false)
                {
                    return;
                }

                _exporter.ExportToCsv(_unitsData, saveDialog.FileName);

                MessageBox.Show(
                    $"Successfully exported {_unitsData.TotalQuantities} quantities with {_unitsData.TotalUnits} units to:\n{saveDialog.FileName}",
                    "Export Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting CSV: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}