using System.Collections;
using System.Linq;
using CUE4Parse.FileProvider;
using System.Threading;
using FModel.Framework;
using FModel.Services;

namespace FModel.ViewModels.Commands;

public class RightClickMenuCommand : ViewModelCommand<ApplicationViewModel>
{
    private ThreadWorkerViewModel _threadWorkerView => ApplicationService.ThreadWorkerView;

    public RightClickMenuCommand(ApplicationViewModel contextViewModel) : base(contextViewModel)
    {
    }

    public override async void Execute(ApplicationViewModel contextViewModel, object parameter)
    {
        if (parameter is not object[] parameters || parameters[0] is not string trigger)
            return;

        var assetItems = ((IList) parameters[1]).Cast<AssetItem>().ToArray();
        if (!assetItems.Any()) return;

        await _threadWorkerView.Begin(cancellationToken =>
        {
            switch (trigger)
            {
                case "Assets_Extract_New_Tab":
                    foreach (var asset in assetItems)
                    {
                        Thread.Sleep(10);
                        cancellationToken.ThrowIfCancellationRequested();
                        contextViewModel.CUE4Parse.Extract(cancellationToken, asset.FullPath, true);
                    }
                    break;
                case "Assets_Export_Data":
                    foreach (var asset in assetItems)
                    {
                        Thread.Sleep(10);
                        cancellationToken.ThrowIfCancellationRequested();
                        contextViewModel.CUE4Parse.ExportData(asset.FullPath);
                    }
                    break;
                case "Assets_Save_Properties":
                    foreach (var asset in assetItems)
                    {
                        Thread.Sleep(10);
                        cancellationToken.ThrowIfCancellationRequested();
                        contextViewModel.CUE4Parse.Extract(cancellationToken, asset.FullPath, false, EBulkType.Properties);
                    }
                    break;
                case "Assets_Save_Textures":
                    foreach (var asset in assetItems)
                    {
                        Thread.Sleep(10);
                        cancellationToken.ThrowIfCancellationRequested();
                        contextViewModel.CUE4Parse.Extract(cancellationToken, asset.FullPath, false, EBulkType.Textures);
                    }
                    break;
                case "Assets_Save_Models":
                    foreach (var asset in assetItems)
                    {
                        Thread.Sleep(10);
                        cancellationToken.ThrowIfCancellationRequested();
                        contextViewModel.CUE4Parse.Extract(cancellationToken, asset.FullPath, false, EBulkType.Meshes | EBulkType.Auto);
                    }
                    break;
                case "Assets_Save_Animations":
                    foreach (var asset in assetItems)
                    {
                        Thread.Sleep(10);
                        cancellationToken.ThrowIfCancellationRequested();
                        contextViewModel.CUE4Parse.Extract(cancellationToken, asset.FullPath, false, EBulkType.Animations | EBulkType.Auto);
                    }
                    break;
                case "Assets_Export_Value_Map":
                    foreach (var asset in assetItems)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (contextViewModel.CUE4Parse.CanExtractValueMap(asset.FullPath, out var osGameFile))
                        {
                            var valueMap = contextViewModel.CUE4Parse.ExtractValueMap(asset.FullPath);
                            contextViewModel.CUE4Parse.TabControl.SelectedTab.ExportValueMap(osGameFile.Name, valueMap);
                        }
                    }

                    break;
                case "Assets_Import_Value_Map":
                    foreach (var asset in assetItems)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (contextViewModel.CUE4Parse.CanExtractValueMap(asset.FullPath, out var osGameFile))
                        {
                            var valueMap = contextViewModel.CUE4Parse.TabControl.SelectedTab.ImportValueMap(osGameFile.Name);
                            contextViewModel.CUE4Parse.ImportValueMap(asset.FullPath, valueMap);
                        }
                    }

                    break;
                case "Assets_Export_Database":
                    foreach (var asset in assetItems)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var exportedDb = contextViewModel.CUE4Parse.ExportDatabase(asset.FullPath, out var fileName);
                        if (exportedDb != null)
                        {
                            contextViewModel.CUE4Parse.TabControl.SelectedTab.ExportDatabase(fileName, exportedDb);
                        }
                    }

                    break;
                case "Assets_Import_Database":
                    foreach (var asset in assetItems)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var databaseData = contextViewModel.CUE4Parse.TabControl.SelectedTab.ImportDatabase();
                        contextViewModel.CUE4Parse.ImportDatabase(asset.FullPath, databaseData);
                    }

                    break;
            }
        });
    }
}
