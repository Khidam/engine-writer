using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace PrinceTitan
{
    public static class ProjectStore
    {
        public static string RootPath
        {
            get { return Path.Combine(Application.persistentDataPath, "PrinceTitan"); }
        }

        public static string ProjectPath
        {
            get { return Path.Combine(RootPath, "project.json"); }
        }

        public static ProjectData LoadOrCreate()
        {
            try
            {
                if (File.Exists(ProjectPath))
                {
                    var json = File.ReadAllText(ProjectPath, Encoding.UTF8);
                    var loaded = JsonUtility.FromJson<ProjectData>(json);
                    if (loaded != null && loaded.chapters != null && loaded.chapters.Count > 0)
                    {
                        Repair(loaded);
                        return loaded;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Prince Titan could not load the project: " + exception.Message);
            }

            var created = WorldSeed.CreateDefaultProject();
            Save(created);
            return created;
        }

        public static void Save(ProjectData project)
        {
            if (project == null) return;
            Directory.CreateDirectory(RootPath);
            var json = JsonUtility.ToJson(project, true);
            var temporary = ProjectPath + ".tmp";
            var backup = ProjectPath + ".backup";
            File.WriteAllText(temporary, json, new UTF8Encoding(false));
            if (File.Exists(ProjectPath))
            {
                File.Copy(ProjectPath, backup, true);
                File.Delete(ProjectPath);
            }
            File.Move(temporary, ProjectPath);
        }

        public static string ExportChapter(ProjectData project, ChapterData chapter)
        {
            if (chapter == null) return string.Empty;
            var folder = Path.Combine(RootPath, "Exports");
            Directory.CreateDirectory(folder);
            var name = SafeFileName(string.IsNullOrWhiteSpace(chapter.title) ? "chapter" : chapter.title);
            var path = Path.Combine(folder, name + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt");
            var body = (chapter.title ?? "Untitled") + Environment.NewLine + Environment.NewLine + (chapter.body ?? string.Empty);
            File.WriteAllText(path, body, new UTF8Encoding(false));
            var manifestPath = Path.Combine(folder, "PrinceTitan-project.json");
            File.WriteAllText(manifestPath, JsonUtility.ToJson(project, true), new UTF8Encoding(false));
            return path;
        }

        public static int CountWords(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            var count = 0;
            var inside = false;
            for (var i = 0; i < value.Length; i++)
            {
                var isWord = char.IsLetterOrDigit(value[i]) || value[i] == '\'' || value[i] == '’';
                if (isWord && !inside) count++;
                inside = isWord;
            }
            return count;
        }

        private static void Repair(ProjectData project)
        {
            if (project.world == null) project.world = WorldSeed.CreateDefaultProject().world;
            if (project.world.factions == null || project.world.factions.Count != 4) project.world.factions = WorldSeed.CreateDefaultProject().world.factions;
            if (project.world.markets == null || project.world.markets.Count == 0) project.world.markets = WorldSeed.CreateDefaultProject().world.markets;
            if (project.world.movers == null || project.world.movers.Count == 0) project.world.movers = WorldSeed.CreateDefaultProject().world.movers;
            if (string.IsNullOrEmpty(project.activeChapterId)) project.activeChapterId = project.chapters[0].id;
        }

        private static string SafeFileName(string value)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '-');
            value = value.Trim();
            return value.Length > 70 ? value.Substring(0, 70) : value;
        }
    }
}
