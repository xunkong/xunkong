using System.Runtime.InteropServices;
using Vanara.PInvoke;
using static Xunkong.Desktop.Helpers.FileDialogNative;

namespace Xunkong.Desktop.Helpers;

internal static class FileDialogHelper
{


    public static async Task<string?> PickSingleFileAsync(nint parentWindow, IEnumerable<(string Name, string Extension)>? fileTypeFilter = null) => await Task.Run(() =>
    {
        IFileOpenDialog? dialog = null;
        IShellItem? resShell = null;
        string? result;

        try
        {
            dialog = new NativeFileOpenDialog();
            dialog.SetOptions(FOS.FOS_NOREADONLYRETURN | FOS.FOS_DONTADDTORECENT);

            SetFileTypeFilter(dialog, fileTypeFilter);

            if (dialog.Show(parentWindow) < 0)
            {
                return null;
            };

            dialog.GetResult(out resShell);
            resShell.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out result);
            return result;
        }
        catch (COMException)
        {
            return null;
        }
        finally
        {
            if (dialog != null) Marshal.FinalReleaseComObject(dialog);
            if (resShell != null) Marshal.FinalReleaseComObject(resShell);
        }
    }).ConfigureAwait(false);



    public static async Task<string?> PickMultiFilesAsync(nint parentWindow, IEnumerable<(string Name, string Extension)>? fileTypeFilter = null) => await Task.Run(() =>
    {
        IFileOpenDialog? dialog = null;
        IShellItem? resShell = null;
        string? result;

        try
        {
            dialog = new NativeFileOpenDialog();
            dialog.SetOptions(FOS.FOS_NOREADONLYRETURN | FOS.FOS_DONTADDTORECENT | FOS.FOS_ALLOWMULTISELECT);

            SetFileTypeFilter(dialog, fileTypeFilter);

            if (dialog.Show(parentWindow) < 0)
            {
                return null;
            };

            dialog.GetResult(out resShell);
            resShell.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out result);
            return result;
        }
        catch (COMException)
        {
            return null;
        }
        finally
        {
            if (dialog != null) Marshal.FinalReleaseComObject(dialog);
            if (resShell != null) Marshal.FinalReleaseComObject(resShell);
        }
    }).ConfigureAwait(false);



    public static async Task<string?> OpenSaveFileDialogAsync(nint parentWindow, string? suggestPath = null, bool ignoreRecentPath = false, IEnumerable<(string Name, string Spec)>? fileTypeFilter = null) => await Task.Run(() =>
    {
        IFileSaveDialog? dialog = null;
        IShellItem? resShell = null;
        string result;
        try
        {
            dialog = new NativeFileSaveDialog();
            dialog.SetOptions(FOS.FOS_NOREADONLYRETURN | FOS.FOS_DONTADDTORECENT | FOS.FOS_OVERWRITEPROMPT);

            var types = SetFileTypeFilter(dialog, fileTypeFilter);

            if (!string.IsNullOrWhiteSpace(suggestPath))
            {
                var name = Path.GetFileName(suggestPath);
                dialog.SetFileName(name);
                var folder = Path.GetDirectoryName(suggestPath);
                if (Directory.Exists(folder))
                {
                    if (ignoreRecentPath)
                    {
                        dialog.SetFolder(Shell32.SHCreateItemFromParsingName<IShellItem>(folder));
                    }
                    else
                    {
                        dialog.SetDefaultFolder(Shell32.SHCreateItemFromParsingName<IShellItem>(folder));
                    }
                }
            }


            if (dialog.Show(parentWindow) < 0)
            {
                return null;
            };

            dialog.GetResult(out resShell);
            resShell.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out result);
            dialog.GetFileTypeIndex(out uint index);
            var extension = Path.GetExtension(types[index - 1].pszSpec);
            if (!result.EndsWith(extension))
            {
                result += extension;
            }
            return result;
        }
        catch (COMException)
        {
            return null;
        }
        finally
        {
            if (dialog != null) Marshal.FinalReleaseComObject(dialog);
            if (resShell != null) Marshal.FinalReleaseComObject(resShell);
        }
    }).ConfigureAwait(false);



    private static COMDLG_FILTERSPEC[] SetFileTypeFilter(in IFileDialog dialog, IEnumerable<(string Name, string Spec)>? fileTypeFilter = null)
    {
        uint count = (uint)(fileTypeFilter?.Count() ?? 0);
        COMDLG_FILTERSPEC[] types;
        if (fileTypeFilter == null || count == 0)
        {
            count++;
            types = new COMDLG_FILTERSPEC[] { new COMDLG_FILTERSPEC { pszName = "all", pszSpec = "*" } };
        }
        else
        if (count == 1)
        {
            types = new COMDLG_FILTERSPEC[] { new COMDLG_FILTERSPEC { pszName = fileTypeFilter.First().Name, pszSpec = fileTypeFilter.First().Spec } };
        }
        else
        {
            count++;
            types = new COMDLG_FILTERSPEC[count];
            types[0] = new COMDLG_FILTERSPEC { pszName = "all", pszSpec = string.Join(';', fileTypeFilter.Select(x => x.Spec)) };
            fileTypeFilter.Select(x => new COMDLG_FILTERSPEC { pszName = x.Name, pszSpec = x.Spec }).ToArray().CopyTo(types, 1);
        }
        dialog.SetFileTypes(count, types);
        return types;
    }



    public static async Task<string?> PickFolderAsync(nint parentWindow, string? suggestPath = null, bool ignoreRecentPath = false) => await Task.Run(() =>
    {
        IFileDialog? dialog = null;
        IShellItem? resShell = null;
        string? result;

        try
        {
            dialog = new NativeFileOpenDialog();
            dialog.SetOptions(FOS.FOS_NOREADONLYRETURN | FOS.FOS_DONTADDTORECENT | FOS.FOS_PICKFOLDERS);

            if (Directory.Exists(suggestPath))
            {
                if (ignoreRecentPath)
                {
                    dialog.SetFolder(Shell32.SHCreateItemFromParsingName<IShellItem>(suggestPath));
                }
                else
                {
                    dialog.SetDefaultFolder(Shell32.SHCreateItemFromParsingName<IShellItem>(suggestPath));
                }
            }

            if (dialog.Show(parentWindow) < 0)
            {
                return null;
            };

            dialog.GetFolder(out resShell);
            resShell.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out result);
            return result;
        }
        catch (COMException)
        {
            return null;
        }
        finally
        {
            if (dialog != null) Marshal.FinalReleaseComObject(dialog);
            if (resShell != null) Marshal.FinalReleaseComObject(resShell);
        }
    }).ConfigureAwait(false);








}
