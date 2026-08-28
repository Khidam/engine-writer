using System;
using System.IO;
using System.Linq;
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
                Debug.LogWarning("Prince Titan não conseguiu abrir o projeto: " + exception.Message);
            }

            var created = WorldSeed.CreateDefaultProject();
            Save(created);
            return created;
        }

        public static void Save(ProjectData project)
        {
            if (project == null) return;
            Directory.CreateDirectory(RootPath);
            project.schema = "prince-titan/2";
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
            var folder = ExportFolder();
            var name = SafeFileName(string.IsNullOrWhiteSpace(chapter.title) ? "capitulo" : chapter.title);
            var path = Path.Combine(folder, name + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt");
            var body = (chapter.title ?? "Sem título") + Environment.NewLine + Environment.NewLine + (chapter.body ?? string.Empty);
            File.WriteAllText(path, body, new UTF8Encoding(false));
            WriteManifest(project, folder);
            return path;
        }

        public static string ExportWorldBook(ProjectData project)
        {
            var folder = ExportFolder();
            var path = Path.Combine(folder, "PrinceTitan-Mundo-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".md");
            var text = new StringBuilder();
            text.AppendLine("# " + project.projectName);
            text.AppendLine();
            text.AppendLine("## Quatro poderes");
            foreach (var faction in project.factions)
            {
                var state = project.world.factions.FirstOrDefault(f => f.factionId == faction.id);
                text.AppendLine("- **" + faction.name + "** — " + faction.kind + "; influência " + (state == null ? 0f : state.influence).ToString("0") + "%. " + faction.motto);
            }
            text.AppendLine();
            text.AppendLine("## Pessoas e famílias");
            foreach (var person in project.people)
                text.AppendLine("- **" + person.name + "** — " + person.family + "; " + person.role + "; origem: " + person.origin + "; nascimento: " + person.birthYear + ".");
            text.AppendLine();
            text.AppendLine("## Lugares, mercados e companhias");
            foreach (var site in project.sites)
                text.AppendLine("- **" + site.name + "** — " + site.kind + "; " + site.note);
            File.WriteAllText(path, text.ToString(), new UTF8Encoding(false));
            WriteManifest(project, folder);
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

        private static string ExportFolder()
        {
            var folder = Path.Combine(RootPath, "Exports");
            Directory.CreateDirectory(folder);
            return folder;
        }

        private static void WriteManifest(ProjectData project, string folder)
        {
            File.WriteAllText(Path.Combine(folder, "PrinceTitan-projeto.json"), JsonUtility.ToJson(project, true), new UTF8Encoding(false));
        }

        private static void Repair(ProjectData project)
        {
            var defaults = WorldSeed.CreateDefaultProject();
            if (project.world == null) project.world = defaults.world;
            if (project.world.factions == null || project.world.factions.Count != 4) project.world.factions = defaults.world.factions;
            if (project.world.markets == null || project.world.markets.Count == 0) project.world.markets = defaults.world.markets;
            if (project.world.movers == null || project.world.movers.Count == 0) project.world.movers = defaults.world.movers;
            if (project.factions == null || project.factions.Count != 4) project.factions = WorldSeed.CloneFactions();
            if (project.sites == null || project.sites.Count == 0) project.sites = WorldSeed.CloneSites();
            if (project.people == null || project.people.Count == 0) project.people = WorldSeed.ClonePeople();
            if (string.IsNullOrEmpty(project.activeChapterId) || project.chapters.All(c => c.id != project.activeChapterId))
                project.activeChapterId = project.chapters[0].id;
            if (string.IsNullOrWhiteSpace(project.projectName)) project.projectName = "Príncipe dos Titãs";
            project.schema = "prince-titan/2";
        }

        private static string SafeFileName(string value)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '-');
            value = value.Trim();
            return value.Length > 70 ? value.Substring(0, 70) : value;
        }
    }
}
