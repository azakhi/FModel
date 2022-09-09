using System.Collections;
using System.Linq;
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
                        cancellationToken.ThrowIfCancellationRequested();
                        contextViewModel.CUE4Parse.Extract(asset.FullPath, true);
                    }

                    break;
                case "Assets_Export_Data":
                    foreach (var asset in assetItems)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        contextViewModel.CUE4Parse.ExportData(asset.FullPath);
                    }

                    break;
                case "Assets_Save_Properties":
                    foreach (var asset in assetItems)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        contextViewModel.CUE4Parse.Extract(asset.FullPath);
                        contextViewModel.CUE4Parse.TabControl.SelectedTab.SaveProperty(false);
                    }

                    break;
                case "Assets_Save_Texture":
                    foreach (var asset in assetItems)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        contextViewModel.CUE4Parse.Extract(asset.FullPath);
                        contextViewModel.CUE4Parse.TabControl.SelectedTab.SaveImage(false);
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
            }
        });
    }
}
