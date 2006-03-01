using System;
using System.IO;

namespace WebGen
{
	class Generator
	{
		string _sourcePath;
		string _destFolder;

		public void Generate(string sourcePath, string destFolder)
		{
			_sourcePath = Path.GetFullPath(sourcePath);
			_destFolder = Path.GetFullPath(destFolder);

			CleanUp();
			CreateDestFolder();

			string template = null;

			using (StreamReader sr = File.OpenText(Path.Combine(_sourcePath, "template.html")))
				template = sr.ReadToEnd();

			GenerateContent(template, new string[0]);
		}

		private void CreateDestFolder()
		{
			if (Directory.Exists(_destFolder) == false)
				Directory.CreateDirectory(_destFolder);
		}

		private void CleanUp()
		{
			if (Directory.Exists(_destFolder) == false)
				return;

			Action<string> clean = null; clean = delegate(string path)
			{
				foreach (string file in Directory.GetFiles(path))
					try { File.Delete(file); } catch {}

				foreach (string dir in Directory.GetDirectories(path))
				{
					clean(dir);
					try { Directory.Delete(dir); } catch {}
				}
			};

			clean(_destFolder);
		}

		private void GenerateContent(string template, string[] path)
		{
			string folder     = string.Join("\\", path);
			string destFolder = Path.Combine(_destFolder, folder);
			string backPath   = "";

			for (int i = 0; i < path.Length; i++)
				backPath += "..\\";

			if (Directory.Exists(destFolder) == false)
				Directory.CreateDirectory(destFolder);

			string   sourcePath  = Path.Combine(_sourcePath, folder);
			string[] sourceFiles = Directory.GetFiles(sourcePath);
			string   backLinks   = GeneratePath(path, backPath);

			foreach (string fileName in sourceFiles)
			{
				if (fileName.ToLower().EndsWith("template.html"))
					continue;

				string destName = Path.Combine(destFolder, Path.GetFileName(fileName));

				using (StreamWriter sw = File.CreateText(destName))
				using (StreamReader sr = File.OpenText  (fileName))
				{
					Console.WriteLine(destName);

					string source = sr.ReadToEnd();

					switch (Path.GetExtension(destName).ToLower())
					{
						case ".css": 
							sw.Write(source);
							break;

						case ".htm":
							source = GenerateSource(source);

							sw.WriteLine(string.Format(template, source, backPath, backLinks));
							break;
					}
				}
			}

			string[] dirs    = Directory.GetDirectories(sourcePath);
			string[] newPath = new string[path.Length + 1];

			path.CopyTo(newPath, 0);

			foreach (string dir in dirs)
			{
				string[] dirList = dir.Split('/', '\\');
				string   dirName = dirList[dirList.Length - 1];

				if (dirName == "_svn")
					continue;

				newPath[path.Length] = dirName;

				GenerateContent(template, newPath);
			}
		}

		private string GenerateSource(string source)
		{
			for (int
				 idx = source.IndexOf("<%"),
				 end = source.IndexOf("%>", idx + 2);
				 idx >= 0 &&
				 end >= 0;
				 idx = source.IndexOf("<%", end + 2), 
				 end = source.IndexOf("%>", idx + 2))
			{
				string startSource = source.Substring(0, idx);
				string fileName    = source.Substring(idx + 2, end - idx - 2).Trim();
				string endSource   = source.Substring(end + 2);
				string sourcePath  = Path.Combine(_sourcePath, fileName);
				string code;

				using (StreamReader sr = File.OpenText(sourcePath))
					code = sr.ReadToEnd();

				code = "<pre>" + code + "</pre>";

				source = startSource + code + endSource;
			}

			return source;
		}

		private string GeneratePath(string[] path, string backPath)
		{
			string backLinks = "";

			if (path.Length > 1 && path[0] == "Doc")
			{
				backLinks += string.Format(
					"<br><nobr>&nbsp;&nbsp;<small><a class='m' href='{0}index.htm'>BLToolkit</a>",
					backPath);

				for (int i = 1; i < path.Length; i++)
				{
					string parent = "";

					for (int j = i + 1; j < path.Length; j++)
						parent += "..\\";

					backLinks += string.Format(".<a class='m' href='{0}index.htm'>{1}</a>", parent, path[i]);
				}

				backLinks += "<small></nobr></br>";
			}

			return backLinks;
		}
	}
}