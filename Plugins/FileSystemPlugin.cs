using Microsoft.SemanticKernel;
using System.ComponentModel;
using SemanticKernelDevHub.Models;

namespace SemanticKernelDevHub.Plugins;

/// <summary>
/// File system operations plugin for Semantic Kernel
/// Handles file monitoring, reading, and movement operations for meeting transcripts
/// </summary>
public class FileSystemPlugin
{
    private readonly string _incomingDirectory;
    private readonly string _processingDirectory;
    private readonly string _archiveDirectory;
    private readonly string _templatesDirectory;
    private FileSystemWatcher? _fileWatcher;
    private readonly List<string> _supportedExtensions = new() { ".txt", ".md", ".docx" };

    public FileSystemPlugin()
    {
        var baseDirectory = Directory.GetCurrentDirectory();
        _incomingDirectory = Path.Combine(baseDirectory, "Data", "Incoming");
        _processingDirectory = Path.Combine(baseDirectory, "Data", "Processing");
        _archiveDirectory = Path.Combine(baseDirectory, "Data", "Archive");
        _templatesDirectory = Path.Combine(baseDirectory, "Data", "Templates");

        EnsureDirectoriesExist();
    }

    /// <summary>
    /// Event triggered when a new file is detected
    /// </summary>
    public event EventHandler<FileSystemEventArgs>? FileDetected;

    /// <summary>
    /// Reads the content of a transcript file
    /// </summary>
    /// <param name="filePath">Path to the transcript file</param>
    /// <returns>File content as string</returns>
    [KernelFunction("read_transcript_file")]
    [Description("Reads the content of a meeting transcript file")]
    public async Task<string> ReadTranscriptFile(
        [Description("Full path to the transcript file to read")] string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"❌ File not found: {filePath}";
            }

            if (!IsValidTranscriptFile(filePath))
            {
                return $"❌ Unsupported file type: {Path.GetExtension(filePath)}. Supported: {string.Join(", ", _supportedExtensions)}";
            }

            var content = await File.ReadAllTextAsync(filePath);
            
            if (string.IsNullOrWhiteSpace(content))
            {
                return "❌ File is empty or contains no readable content";
            }

            return content;
        }
        catch (UnauthorizedAccessException)
        {
            return $"❌ Access denied to file: {filePath}";
        }
        catch (IOException ex)
        {
            return $"❌ Error reading file: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"❌ Unexpected error reading file: {ex.Message}";
        }
    }

    /// <summary>
    /// Lists all transcript files in the incoming directory
    /// </summary>
    /// <returns>List of transcript file paths</returns>
    [KernelFunction("list_incoming_files")]
    [Description("Lists all transcript files waiting to be processed in the incoming directory")]
    public async Task<List<string>> ListIncomingFiles()
    {
        try
        {
            var files = Directory.GetFiles(_incomingDirectory)
                .Where(file => IsValidTranscriptFile(file))
                .OrderBy(file => File.GetCreationTime(file))
                .ToList();

            return await Task.FromResult(files);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error listing incoming files: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Moves a file from incoming to processing directory
    /// </summary>
    /// <param name="fileName">Name of the file to move</param>
    /// <returns>New file path or error message</returns>
    [KernelFunction("move_to_processing")]
    [Description("Moves a transcript file from incoming to processing directory")]
    public async Task<string> MoveToProcessing(
        [Description("Name of the file to move to processing")] string fileName)
    {
        try
        {
            var sourcePath = Path.Combine(_incomingDirectory, fileName);
            var destinationPath = Path.Combine(_processingDirectory, fileName);

            if (!File.Exists(sourcePath))
            {
                return $"❌ Source file not found: {sourcePath}";
            }

            if (File.Exists(destinationPath))
            {
                // Create unique filename if destination exists
                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var extension = Path.GetExtension(fileName);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                destinationPath = Path.Combine(_processingDirectory, $"{nameWithoutExt}_{timestamp}{extension}");
            }

            File.Move(sourcePath, destinationPath);
            
            return await Task.FromResult(destinationPath);
        }
        catch (Exception ex)
        {
            return $"❌ Error moving file to processing: {ex.Message}";
        }
    }

    /// <summary>
    /// Moves a file from processing to archive directory
    /// </summary>
    /// <param name="fileName">Name of the file to archive</param>
    /// <returns>Archived file path or error message</returns>
    [KernelFunction("archive_file")]
    [Description("Moves a processed transcript file to the archive directory")]
    public async Task<string> ArchiveFile(
        [Description("Name of the file to archive")] string fileName)
    {
        try
        {
            var sourcePath = Path.Combine(_processingDirectory, fileName);
            var destinationPath = Path.Combine(_archiveDirectory, fileName);

            if (!File.Exists(sourcePath))
            {
                return $"❌ Source file not found: {sourcePath}";
            }

            if (File.Exists(destinationPath))
            {
                // Create unique filename if destination exists
                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var extension = Path.GetExtension(fileName);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                destinationPath = Path.Combine(_archiveDirectory, $"{nameWithoutExt}_archived_{timestamp}{extension}");
            }

            File.Move(sourcePath, destinationPath);
            
            return await Task.FromResult(destinationPath);
        }
        catch (Exception ex)
        {
            return $"❌ Error archiving file: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets information about a transcript file
    /// </summary>
    /// <param name="filePath">Path to the transcript file</param>
    /// <returns>File information</returns>
    [KernelFunction("get_file_info")]
    [Description("Gets metadata information about a transcript file")]
    public async Task<string> GetFileInfo(
        [Description("Full path to the transcript file")] string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"❌ File not found: {filePath}";
            }

            var fileInfo = new FileInfo(filePath);
            var content = await File.ReadAllTextAsync(filePath);
            var lineCount = content.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
            
            var info = $@"
📄 **File Information**
📁 **Name:** {fileInfo.Name}
📂 **Directory:** {Path.GetDirectoryName(filePath)}
📏 **Size:** {FormatFileSize(fileInfo.Length)}
📅 **Created:** {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}
🔄 **Modified:** {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}
📝 **Lines:** {lineCount:N0}
📊 **Characters:** {content.Length:N0}
🔤 **Extension:** {fileInfo.Extension}
✅ **Readable:** {IsValidTranscriptFile(filePath)}";

            return info;
        }
        catch (Exception ex)
        {
            return $"❌ Error getting file info: {ex.Message}";
        }
    }

    /// <summary>
    /// Lists all template transcript files
    /// </summary>
    /// <returns>List of template file paths</returns>
    [KernelFunction("list_template_files")]
    [Description("Lists all sample/template transcript files available for analysis")]
    public async Task<List<string>> ListTemplateFiles()
    {
        try
        {
            var files = Directory.GetFiles(_templatesDirectory)
                .Where(file => IsValidTranscriptFile(file))
                .OrderBy(file => Path.GetFileName(file))
                .ToList();

            return await Task.FromResult(files);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error listing template files: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Starts monitoring the incoming directory for new files
    /// </summary>
    /// <returns>Status message</returns>
    [KernelFunction("start_file_watcher")]
    [Description("Starts monitoring the incoming directory for new transcript files")]
    public async Task<string> StartFileWatcher()
    {
        try
        {
            if (_fileWatcher != null)
            {
                return "📡 File watcher is already running";
            }

            _fileWatcher = new FileSystemWatcher(_incomingDirectory)
            {
                Filter = "*.*",
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _fileWatcher.Created += OnFileCreated;
            _fileWatcher.Renamed += OnFileRenamed;

            return await Task.FromResult($"📡 File watcher started monitoring: {_incomingDirectory}");
        }
        catch (Exception ex)
        {
            return $"❌ Error starting file watcher: {ex.Message}";
        }
    }

    /// <summary>
    /// Stops monitoring the incoming directory
    /// </summary>
    /// <returns>Status message</returns>
    [KernelFunction("stop_file_watcher")]
    [Description("Stops monitoring the incoming directory for new files")]
    public async Task<string> StopFileWatcher()
    {
        try
        {
            if (_fileWatcher == null)
            {
                return "📡 File watcher is not running";
            }

            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Dispose();
            _fileWatcher = null;

            return await Task.FromResult("📡 File watcher stopped");
        }
        catch (Exception ex)
        {
            return $"❌ Error stopping file watcher: {ex.Message}";
        }
    }

    /// <summary>
    /// Creates a MeetingTranscript object from a file path
    /// </summary>
    /// <param name="filePath">Path to the transcript file</param>
    /// <returns>MeetingTranscript object</returns>
    public async Task<MeetingTranscript> CreateTranscriptFromFile(string filePath)
    {
        var content = await ReadTranscriptFile(filePath);
        var fileName = Path.GetFileName(filePath);
        
        var transcript = new MeetingTranscript
        {
            Title = MeetingTranscript.ExtractTitleFromFileName(fileName),
            MeetingDate = MeetingTranscript.ExtractDateFromFileName(fileName),
            Content = content,
            FilePath = filePath,
            Status = TranscriptStatus.Pending
        };

        // Extract participants from content
        transcript.Participants = MeetingParticipant.ExtractParticipants(content);

        return transcript;
    }

    /// <summary>
    /// Ensures all required directories exist
    /// </summary>
    private void EnsureDirectoriesExist()
    {
        var directories = new[] { _incomingDirectory, _processingDirectory, _archiveDirectory, _templatesDirectory };
        
        foreach (var directory in directories)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Console.WriteLine($"📁 Created directory: {directory}");
            }
        }
    }

    /// <summary>
    /// Checks if a file is a valid transcript file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>True if the file is supported</returns>
    private bool IsValidTranscriptFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedExtensions.Contains(extension);
    }

    /// <summary>
    /// Formats file size in human-readable format
    /// </summary>
    /// <param name="bytes">Size in bytes</param>
    /// <returns>Formatted size string</returns>
    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number = number / 1024;
            counter++;
        }
        
        return $"{number:n1} {suffixes[counter]}";
    }

    /// <summary>
    /// Handles file creation events
    /// </summary>
    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (IsValidTranscriptFile(e.FullPath))
        {
            Console.WriteLine($"📥 New transcript file detected: {e.Name}");
            FileDetected?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Handles file rename events
    /// </summary>
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        if (IsValidTranscriptFile(e.FullPath))
        {
            Console.WriteLine($"📝 Transcript file renamed: {e.OldName} → {e.Name}");
            FileDetected?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Dispose()
    {
        _fileWatcher?.Dispose();
    }
}
