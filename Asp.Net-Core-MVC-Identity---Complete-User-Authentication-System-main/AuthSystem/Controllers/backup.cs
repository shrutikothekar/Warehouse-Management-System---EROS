using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DatabaseBackupService.Controllers
{
    public class Backup
    {
        private readonly string _connectionString;
        private readonly string _pgDumpPath;
        private readonly ILogger<Backup> _logger;

        public Backup(string connectionString, string pgDumpPath, ILogger<Backup> logger)
        {
            _connectionString = connectionString;
            _pgDumpPath = pgDumpPath; // Use the pgDumpPath parameter
            _logger = logger;
        }

        public void BackupDatabase(string backupFilePath)
        {
            try
            {
                // Ensure the directory exists
                var directoryPath = Path.GetDirectoryName(backupFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Construct the pg_dump command
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _pgDumpPath,
                    Arguments = $"-h 192.168.0.152 -d EROS1 -U postgres -p 5432 -F tar -f {backupFilePath}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Set the environment variable for the password
                processStartInfo.EnvironmentVariables["PGPASSWORD"] = "system"; // Use the actual password

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    // Capture standard output and error asynchronously
                    Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                    Task<string> errorTask = process.StandardError.ReadToEndAsync();

                    // Wait for process to complete
                    process.WaitForExit();

                    // Wait for output and error tasks to complete
                    string output = outputTask.Result;
                    string error = errorTask.Result;

                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation($"Backup completed successfully. Output: {output}");
                    }
                    else
                    {
                        _logger.LogError($"Backup failed with exit code {process.ExitCode}. Error: {error}");
                        throw new Exception($"Backup process failed with exit code {process.ExitCode}. Check logs for details.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during backup: {ex.Message}");
                throw; // Propagate exception to caller or handle as needed
            }
        }
    }
    //public void BackupDatabase(string backupFilePath)
    //{
    //    using (var connection = new SqlConnection(_connectionString))
    //    {
    //        var commandText = $"BACKUP DATABASE Weighbridge_Db TO DISK='{backupFilePath}'";
    //        using (var command = new SqlCommand(commandText, connection))
    //        {
    //            connection.Open();
    //            command.ExecuteNonQuery();
    //        }
    //    }
    //}

    public class BackupBackgroundService : BackgroundService
    {
        private readonly Backup _backupService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupBackgroundService> _logger;

        public BackupBackgroundService(Backup backupService, IConfiguration configuration, ILogger<BackupBackgroundService> logger)
        {
            _backupService = backupService;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                // Get last backup times
                var lastDailyBackup = _configuration.GetValue<DateTime?>("LastBackupTimes:Daily");
                var lastWeeklyBackup = _configuration.GetValue<DateTime?>("LastBackupTimes:Weekly");
                var lastMonthlyBackup = _configuration.GetValue<DateTime?>("LastBackupTimes:Monthly");
                var lastQuarterlyBackup = _configuration.GetValue<DateTime?>("LastBackupTimes:Quarterly");
                var lastYearlyBackup = _configuration.GetValue<DateTime?>("LastBackupTimes:Yearly");

                try
                {
                    // Daily Backup
                    if (_configuration.GetValue<bool>("BackupSettings:Daily") && (!lastDailyBackup.HasValue || lastDailyBackup.Value.Date != now.Date))
                    {
                        string dailyBackupPath = $"D:\\Backups\\Daily";
                        Directory.CreateDirectory(dailyBackupPath);
                        string backupFilePath = $"{dailyBackupPath}\\backup_{now:yyyyMMdd}.bak";
                        _backupService.BackupDatabase(backupFilePath);
                        _logger.LogInformation($"Daily backup performed at {now}");
                        _configuration.GetSection("LastBackupTimes:Daily").Value = now.ToString("o");
                    }

                    // Weekly Backup
                    if (_configuration.GetValue<bool>("BackupSettings:Weekly") && now.DayOfWeek == DayOfWeek.Sunday && (!lastWeeklyBackup.HasValue || lastWeeklyBackup.Value.Date != now.Date))
                    {
                        string weeklyBackupPath = $"D:\\Backups\\Weekly";
                        Directory.CreateDirectory(weeklyBackupPath);
                        string backupFilePath = $"{weeklyBackupPath}\\backup_{now:yyyyMMdd}.bak";
                        _backupService.BackupDatabase(backupFilePath);
                        _logger.LogInformation($"Weekly backup performed at {now}");
                        _configuration.GetSection("LastBackupTimes:Weekly").Value = now.ToString("o");
                    }

                    // Monthly Backup
                    if (_configuration.GetValue<bool>("BackupSettings:Monthly") && (!lastMonthlyBackup.HasValue || now >= lastMonthlyBackup.Value.AddMonths(1)))
                    {
                        string monthlyBackupPath = $"D:\\Backups\\Monthly";
                        Directory.CreateDirectory(monthlyBackupPath);
                        string backupFilePath = $"{monthlyBackupPath}\\backup_{now:yyyyMMdd}.bak";
                        _backupService.BackupDatabase(backupFilePath);
                        _logger.LogInformation($"Monthly backup performed at {now}");
                        _configuration.GetSection("LastBackupTimes:Monthly").Value = now.ToString("o");
                    }

                    // Quarterly Backup
                    if (_configuration.GetValue<bool>("BackupSettings:Quarterly") && (now.Month == 1 || now.Month == 4 || now.Month == 7 || now.Month == 10) && now.Day == 1 && (!lastQuarterlyBackup.HasValue || now >= lastQuarterlyBackup.Value.AddMonths(3)))
                    {
                        DateTime backupDate = now.AddMonths(3);
                        string quarterlyBackupPath = $"D:\\Backups\\Quarterly";
                        Directory.CreateDirectory(quarterlyBackupPath);
                        string backupFilePath = $"{quarterlyBackupPath}\\backup_{backupDate:yyyyMMdd}.bak";
                        _backupService.BackupDatabase(backupFilePath);
                        _logger.LogInformation($"Quarterly backup performed at {now}");
                        _configuration.GetSection("LastBackupTimes:Quarterly").Value = now.ToString("o");
                    }

                    // Yearly Backup
                    if (_configuration.GetValue<bool>("BackupSettings:Yearly") && now.Month == 1 && now.Day == 1)
                    {
                        string yearlyBackupPath = $"D:\\Backups\\Yearly";
                        Directory.CreateDirectory(yearlyBackupPath);
                        string backupFilePath = $"{yearlyBackupPath}\\backup_{now:yyyyMMdd}.bak";
                        _backupService.BackupDatabase(backupFilePath);
                        _logger.LogInformation($"Yearly backup performed at {now}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception during scheduled backup: {ex.Message}");
                }

                // Wait for 24 hours before the next check
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
