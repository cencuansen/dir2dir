using dir2dir.dtos;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace dir2dir
{
    public class MainWinObject
    {

        readonly WebView2 webView21;

        public MainWinObject(WebView2 webView2)
        {
            webView21 = webView2;
        }

        public string DirectorySelector()
        {
            using FolderBrowserDialog folderDialog = new()
            {
                InitialDirectory = "C:"
            };
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                return folderDialog.SelectedPath;
            }
            return "";
        }

        public string GetSubDirs(string directoryPath)
        {
            IEnumerable<DirItemDto> items = Directory.GetDirectories(directoryPath).Select(directory =>
            {
                DirectoryInfo directoryInfo = new(directory);
                return new DirItemDto
                {
                    Name = directoryInfo.Name,
                    ModifiedTime = directoryInfo.LastWriteTime,
                    CreatedTime = directoryInfo.CreationTime,
                    FullPath = directoryInfo.FullName,
                    IsFile = false,
                    IsDir = true,
                    IsLink = directoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint),
                    IsHidden = directoryInfo.Attributes.HasFlag(FileAttributes.Hidden),
                    IsReadOnly = directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly),
                };
            }).ToList();
            return JsonConvert.SerializeObject(items);
        }

        public void MoveDirs(string data)
        {
            MoveDirDto? dto = JsonConvert.DeserializeObject<MoveDirDto>(data);
            double dirIndex = 1.0;
            bool allOk = true;

            try
            {
                foreach (string oldDir in dto?.Froms ?? Array.Empty<string>())
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(oldDir);
                    string name = directoryInfo.Name;
                    string newDir = Path.Combine(dto.To, name);
                    FileSystem.MoveDirectory(oldDir, newDir);
                    Directory.CreateSymbolicLink(newDir, oldDir);

                    MoveProgressDto moveProgressDto = new MoveProgressDto
                    {
                        Path = directoryInfo.FullName,
                        Percentage = dirIndex++ / dto.Froms.Length
                    };
                    EventTrigger(Constants.DIR_MOVE_PROGRESS, moveProgressDto);
                }
            }
            catch (Exception exception)
            {
                allOk = false;
                EventTrigger(Constants.DIR_MOVE_ERROR, exception.Message);
            }

            if (allOk)
            {
                EventTrigger(Constants.DIR_MOVE_OK);
            }
        }

        public string CalcDirSize(dynamic data)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(data);
            long total = CalculateDirectorySize(directoryInfo);
            DirSizeDto dirSize = new()
            {
                FullPath = directoryInfo.FullName,
                Size = total
            };
            return JsonConvert.SerializeObject(dirSize);
        }

        private long CalculateDirectorySize(DirectoryInfo directoryInfo)
        {
            long size = 0;

            try
            {
                foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                {
                    size += fileInfo.Length;
                }
            }
            catch
            {
                return size;
            }

            foreach (DirectoryInfo subDirectoryInfo in directoryInfo.GetDirectories())
            {
                size += CalculateDirectorySize(subDirectoryInfo);
            }

            return size;

        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="data">数据</param>
        private void EventTrigger(string eventName, dynamic data = null)
        {
            var eventData = new MessageDto
            {
                Name = eventName,
                Data = data
            };
            webView21.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(eventData));
        }

    }
}
