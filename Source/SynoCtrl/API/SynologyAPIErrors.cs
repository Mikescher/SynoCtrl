using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SynoCtrl.Util
{
	public static class SynologyAPIErrors
	{
		private static readonly List<Tuple<int, string, string>> ERROR_CODES = new List<Tuple<int, string, string>>
		{
			Tuple.Create(0, "*", "Success"),

			Tuple.Create(100, "*", "Unknown error"),
			Tuple.Create(101, "*", "Invalid parameter"),
			Tuple.Create(102, "*", "The requested API does not exist"),
			Tuple.Create(103, "*", "The requested method does not exist"),
			Tuple.Create(104, "*", "The requested version does not support the functionality"),
			Tuple.Create(105, "*", "The logged in session does not have permission"),
			Tuple.Create(106, "*", "Session timeout"),
			Tuple.Create(107, "*", "Session interrupted by duplicate login"),
			
			Tuple.Create(400, "SYNO.API.Auth", "No such account or incorrect password"),
			Tuple.Create(401, "SYNO.API.Auth", "Account disabled"),
			Tuple.Create(402, "SYNO.API.Auth", "Permission denied"),
			Tuple.Create(403, "SYNO.API.Auth", "2-step verification code required"),
			Tuple.Create(404, "SYNO.API.Auth", "Failed to authenticate 2-step verification code"),
			
			Tuple.Create(400, "SYNO.DownloadStation.Task", "File upload failed"),
			Tuple.Create(401, "SYNO.DownloadStation.Task", "Max number of tasks reached"),
			Tuple.Create(402, "SYNO.DownloadStation.Task", "Destination denied"),
			Tuple.Create(403, "SYNO.DownloadStation.Task", "Destination denied"),
			Tuple.Create(404, "SYNO.DownloadStation.Task", "Invalid task id"),
			Tuple.Create(405, "SYNO.DownloadStation.Task", "Invalid task action"),
			Tuple.Create(406, "SYNO.DownloadStation.Task", "No default destination"),
			Tuple.Create(407, "SYNO.DownloadStation.Task", "Set destination failed"),
			Tuple.Create(408, "SYNO.DownloadStation.Task", "File does not exist"),
			
			Tuple.Create(400, "SYNO.DownloadStation.BTSearch", "Unknown error"),
			Tuple.Create(401, "SYNO.DownloadStation.BTSearch", "Invalid parameter"),
			Tuple.Create(402, "SYNO.DownloadStation.BTSearch", "Parse the user setting failed"),
			Tuple.Create(403, "SYNO.DownloadStation.BTSearch", "Get category failed"),
			Tuple.Create(404, "SYNO.DownloadStation.BTSearch", "Get the search result from DB failed"),
			Tuple.Create(405, "SYNO.DownloadStation.BTSearch", "Get the user setting failed"),

			Tuple.Create(400, "SYNO.FileStation.*", "Invalid parameter of file operation"),
			Tuple.Create(401, "SYNO.FileStation.*", "Unknown error of file operation"),
			Tuple.Create(402, "SYNO.FileStation.*", "System is too busy"),
			Tuple.Create(403, "SYNO.FileStation.*", "Invalid user does this file operation"),
			Tuple.Create(404, "SYNO.FileStation.*", "Invalid group does this file operation"),
			Tuple.Create(405, "SYNO.FileStation.*", "Invalid user and group does this file operation"),
			Tuple.Create(406, "SYNO.FileStation.*", "Can't get user/group information from the account server"),
			Tuple.Create(407, "SYNO.FileStation.*", "Operation not permitted408No such file or directory"),
			Tuple.Create(409, "SYNO.FileStation.*", "Non-supported file system 410Failed to connect internet-based file system(ex: CIFS)"),
			Tuple.Create(411, "SYNO.FileStation.*", "Read-only file system"),
			Tuple.Create(412, "SYNO.FileStation.*", "Filename too long in the non-encrypted file system"),
			Tuple.Create(413, "SYNO.FileStation.*", "Filename too long in the encrypted file system"),
			Tuple.Create(414, "SYNO.FileStation.*", "File already exists"),
			Tuple.Create(415, "SYNO.FileStation.*", "Disk quota exceeded"),
			Tuple.Create(416, "SYNO.FileStation.*", "No space left on device"),
			Tuple.Create(417, "SYNO.FileStation.*", "Input/output error"),
			Tuple.Create(418, "SYNO.FileStation.*", "Illegal name or path"),
			Tuple.Create(419, "SYNO.FileStation.*", "Illegal file name"),
			Tuple.Create(420, "SYNO.FileStation.*", "Illegal file name on FAT filesystem"),
			Tuple.Create(421, "SYNO.FileStation.*", "Device or resource busy"),
			
			Tuple.Create(400, "SYNO.FileStation.*", "Invalid parameter of file operation"),

			Tuple.Create(599, "SYNO.FileStation.*", "No such task of the file operation"),
			
			Tuple.Create(800, "SYNO.FileStation.Favorite", "A folder path of favorite folder is already added to user's favorites."),
			Tuple.Create(801, "SYNO.FileStation.Favorite", "A name of favorite folder conflicts with an existing folder path in the user's favorites."),
			Tuple.Create(802, "SYNO.FileStation.Favorite", "There are too many favorites to be added."),
			
			Tuple.Create(900, "SYNO.FileStation.Delete", "Failed to delete file(s)/folder(s). More information in <errors> object."),

			Tuple.Create(1000, "SYNO.FileStation.CopyMove", "Failed to copy files/folders. More information in <errors> object."),
			Tuple.Create(1001, "SYNO.FileStation.CopyMove", "Failed to move files/folders. More information in <errors> object."),
			Tuple.Create(1002, "SYNO.FileStation.CopyMove", "An error occurred at the destination. More information in <errors> object."),
			Tuple.Create(1003, "SYNO.FileStation.CopyMove", "Cannot overwrite or skip the existing file because no overwrite parameter is given."),
			Tuple.Create(1004, "SYNO.FileStation.CopyMove", "File cannot overwrite a folder with the same name,or folder cannot overwrite a file with the same name."),
			Tuple.Create(1006, "SYNO.FileStation.CopyMove", "Cannot copy/move file/folder with special characters to a FAT32 file system."),
			Tuple.Create(1007, "SYNO.FileStation.CopyMove", "Cannot copy/move a file bigger than 4G to a FAT32 file system."),
			
			Tuple.Create(1100, "SYNO.FileStation.CreateFolder", "Failed to create a folder. More information in <errors> object."),
			Tuple.Create(1100, "SYNO.FileStation.CreateFolder", "The number of folders to the parent folder would exceed the system limitation."),
			
			Tuple.Create(1200, "SYNO.FileStation.Rename", "Failed to rename it. More information in <errors> object."),
			
			Tuple.Create(1300, "SYNO.FileStation.Compress", "Failed to compress files/folders."),
			Tuple.Create(1301, "SYNO.FileStation.Compress", "Cannot create the archive because the given archive name is too long."),
			
			Tuple.Create(1400, "SYNO.FileStation.Extract", "Failed to extract files."),
			Tuple.Create(1401, "SYNO.FileStation.Extract", "Cannot open the file as archive."),
			Tuple.Create(1402, "SYNO.FileStation.Extract", "Failed to read archive data error"),
			Tuple.Create(1403, "SYNO.FileStation.Extract", "Wrong password."),
			Tuple.Create(1404, "SYNO.FileStation.Extract", "Failed to get the file and dir list in an archive."),
			Tuple.Create(1405, "SYNO.FileStation.Extract", "Failed to find the item ID in an archive file."),
			
			Tuple.Create(1800, "SYNO.FileStation.Upload", "There is no Content-Length information in the HTTP header or the received size doesn’t match the value of Content-Length information in the HTTP header."),
			Tuple.Create(1801, "SYNO.FileStation.Upload", "Wait too long, no date can be received from client(Default maximum wait time is 3600 seconds)."),
			Tuple.Create(1802, "SYNO.FileStation.Upload", "No filename information in the last part of file content."),
			Tuple.Create(1803, "SYNO.FileStation.Upload", "Upload connection is cancelled."),
			Tuple.Create(1804, "SYNO.FileStation.Upload", "Failed to upload too big file to FAT file system."),
			Tuple.Create(1805, "SYNO.FileStation.Upload", "Can't overwrite or skip the existed file, if no overwrite parameter is given."),
			
			Tuple.Create(2000, "SYNO.FileStation.Sharing", "Sharing link does not exist."),
			Tuple.Create(2001, "SYNO.FileStation.Sharing", "Cannot generate sharing link because too many sharing links exist."),
			Tuple.Create(2002, "SYNO.FileStation.Sharing", "Failed to access sharing links."),
		};

		public static string GetErrorMessage(string api, int errorcode)
		{
			var fod = ERROR_CODES.FirstOrDefault(c => c.Item1 == errorcode && WildcardMatch(c.Item2, api));
			return fod?.Item3;
		}

		private static bool WildcardMatch(string pattern, string needle)
		{
			return new Regex("^" + string.Join(".*", pattern.Split('*').Select(Regex.Escape)) + "$").IsMatch(needle);
		}
	}
}
