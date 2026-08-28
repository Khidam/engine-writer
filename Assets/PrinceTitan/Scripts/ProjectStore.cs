using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PrinceTitan
{
    public static class ProjectStore
    {
        public static string RootPath { get { return Path.Combine(Application.persistentDataPath, "PrinceTitan"); } }
        public static string ProjectPath { get { return Path.Combine(RootPath, "project.json"); } }
        public static string BackupPath { get { return ProjectPath + ".backup"; } }
        public static string TrashPath { get { return Path.Combine(RootPath, "Deleted Projects"); } }

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
            project.schema = "prince-titan/3";
            var json = JsonUtility.ToJson(project, true);
            var temporary = ProjectPath + ".tmp";
            File.WriteAllText(temporary, json, new UTF8Encoding(false));
            if (File.Exists(ProjectPath))
            {
                File.Copy(ProjectPath, BackupPath, true);
                File.Delete(ProjectPath);
            }
            File.Move(temporary, ProjectPath);
        }

        public static string DeleteProjectToTrash()
        {
            Directory.CreateDirectory(TrashPath);
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var destination = Path.Combine(TrashPath, "PrinceTitan-" + stamp + ".json");
            if (File.Exists(ProjectPath)) File.Move(ProjectPath, destination);
            if (File.Exists(BackupPath))
            {
                var backupDestination = Path.Combine(TrashPath, "PrinceTitan-" + stamp + "-backup.json");
                File.Copy(BackupPath, backupDestination, true);
                File.Delete(BackupPath);
            }
            return destination;
        }

        public static bool RestoreLatestBackup()
        {
            Directory.CreateDirectory(RootPath);
            FileInfo latest = null;
            if (Directory.Exists(TrashPath))
                latest = new DirectoryInfo(TrashPath).GetFiles("PrinceTitan-*.json").Where(file => !file.Name.EndsWith("-backup.json", StringComparison.OrdinalIgnoreCase)).OrderByDescending(file => file.LastWriteTimeUtc).FirstOrDefault();
            if (latest == null && File.Exists(BackupPath)) latest = new FileInfo(BackupPath);
            if (latest == null) return false;
            File.Copy(latest.FullName, ProjectPath, true);
            return true;
        }

        public static bool HasRecoverableBackup()
        {
            return File.Exists(BackupPath) || (Directory.Exists(TrashPath) && Directory.GetFiles(TrashPath, "*.json").Length > 0);
        }

        public static string ExportChapter(ProjectData project, ChapterData chapter)
        {
            if (chapter == null) return string.Empty;
            var folder = ExportFolder();
            var name = SafeFileName(string.IsNullOrWhiteSpace(chapter.title) ? "capitulo" : chapter.title);
            var path = Path.Combine(folder, name + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt");
            var context = "CLASSIFICAÇÃO: " + (chapter.classification ?? "RELATO") + Environment.NewLine +
                          "LOCAL: " + (WorldSeed.Site(project, chapter.locationId)?.name ?? "Não definido") + Environment.NewLine +
                          "PONTO DE VISTA: " + (chapter.pointOfView ?? "Não definido") + Environment.NewLine +
                          "MÁQUINA: " + (WorldSeed.Machine(project, chapter.machineId)?.name ?? "Não definida");
            var body = (chapter.title ?? "Sem título") + Environment.NewLine + context + Environment.NewLine + Environment.NewLine + (chapter.body ?? string.Empty);
            File.WriteAllText(path, body, new UTF8Encoding(false));
            WriteManifest(project, folder);
            return path;
        }

        public static string ExportWorldBook(ProjectData project)
        {
            var folder = ExportFolder();
            var path = Path.Combine(folder, "PrinceTitan-ArquivoDeGuerra-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".md");
            var text = new StringBuilder();
            text.AppendLine("# " + project.projectName);
            text.AppendLine();
            text.AppendLine("## Organizações e nações");
            foreach (var organization in project.organizations)
                text.AppendLine("- **" + organization.name + "** — " + organization.kind + ". " + organization.doctrine + " Recursos: " + organization.resources);
            text.AppendLine();
            text.AppendLine("## Pessoas, habilidades e funções");
            foreach (var person in project.people)
                text.AppendLine("- **" + person.name + "** — " + person.role + "; formação: " + person.teamRole + "; habilidade: " + person.ability + "; técnica: " + person.technique);
            text.AppendLine();
            text.AppendLine("## Máquinas e Titãs");
            foreach (var machine in project.machines)
                text.AppendLine("- **" + machine.name + " / " + machine.model + "** — " + machine.currentState + ". Sistemas: " + machine.systems + " Tripulação: " + machine.crew);
            text.AppendLine();
            text.AppendLine("## Missões");
            foreach (var mission in project.world.missions)
            {
                var origin = WorldSeed.Site(project, mission.originSiteId);
                var destination = WorldSeed.Site(project, mission.destinationSiteId);
                text.AppendLine("- **" + mission.callsign + " — " + mission.title + "**: " + (origin == null ? "?" : origin.name) + " → " + (destination == null ? "?" : destination.name) + ". " + mission.objective + " Estado: " + mission.status + ".");
            }
            text.AppendLine();
            text.AppendLine("## Gravações recuperadas");
            foreach (var recording in project.world.recordings)
                text.AppendLine("- **" + recording.title + "** — " + recording.location + ". " + recording.summary);
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
            var migrating = !string.Equals(project.schema, "prince-titan/3", StringComparison.OrdinalIgnoreCase);
            if (migrating)
            {
                project.organizations = defaults.organizations;
                project.sites = defaults.sites;
                project.people = defaults.people;
                project.machines = defaults.machines;
                project.world = defaults.world;
                foreach (var chapter in project.chapters)
                {
                    if (string.IsNullOrEmpty(chapter.locationId)) chapter.locationId = defaults.chapters[0].locationId;
                    if (string.IsNullOrEmpty(chapter.pointOfView)) chapter.pointOfView = defaults.chapters[0].pointOfView;
                    if (string.IsNullOrEmpty(chapter.machineId)) chapter.machineId = defaults.chapters[0].machineId;
                    if (string.IsNullOrEmpty(chapter.classification)) chapter.classification = "RELATO MIGRADO";
                }
            }
            else
            {
                if (project.organizations == null || project.organizations.Count == 0) project.organizations = defaults.organizations;
                if (project.sites == null || project.sites.Count == 0) project.sites = defaults.sites;
                if (project.people == null || project.people.Count == 0) project.people = defaults.people;
                if (project.machines == null || project.machines.Count == 0) project.machines = defaults.machines;
                if (project.world == null) project.world = defaults.world;
                if (project.world.missions == null || project.world.missions.Count == 0) project.world.missions = defaults.world.missions;
                if (project.world.eventHistory == null) project.world.eventHistory = defaults.world.eventHistory;
                if (project.world.recordings == null || project.world.recordings.Count == 0) project.world.recordings = defaults.world.recordings;
            }
            if (string.IsNullOrEmpty(project.activeChapterId) || project.chapters.All(chapter => chapter.id != project.activeChapterId)) project.activeChapterId = project.chapters[0].id;
            if (string.IsNullOrWhiteSpace(project.projectName)) project.projectName = "Prince of Titans";
            project.schema = "prince-titan/3";
        }

        private static string SafeFileName(string value)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '-');
            value = value.Trim();
            return value.Length > 70 ? value.Substring(0, 70) : value;
        }
    }
}
