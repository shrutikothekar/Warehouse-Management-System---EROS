using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

using System.Data.SqlClient;

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
namespace DatabaseBackupService.Controllers
{
    public class Backup
    {
        private readonly string _connectionString;

        public Backup(string connectionString)
        {
            _connectionString = connectionString;
        }

        //public void BackupDatabase(string backupFilePath)
        //{
        //    using (var connection = new NpgsqlConnection(_connectionString))
        //    {
        //        var commandText = $"COPY (SELECT * FROM pg_catalog.pg_tables WHERE schemaname != 'pg_catalog' AND schemaname != 'information_schema') TO '{backupFilePath}' WITH CSV HEADER";


        //        using (var command = new NpgsqlCommand(commandText, connection))
        //        {
        //            try
        //            {
        //                connection.Open();
        //                command.ExecuteNonQuery();
        //                Console.WriteLine("Backup successful.");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Error during backup: {ex.Message}");
        //                // Handle exception or throw it further
        //            }
        //        }
        //    }
        //}
        public void BackupDatabase(string backupFilePath)
        {
            // Ensure the directory exists
            var directoryPath = Path.GetDirectoryName(backupFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                // Construct the SQL command to perform the backup
                var commandText = $"COPY (SELECT * FROM pg_catalog.pg_tables WHERE schemaname != 'pg_catalog' AND schemaname != 'information_schema') TO '{backupFilePath}' WITH CSV HEADER";

                using (var command = new NpgsqlCommand(commandText, connection))
                {
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        Console.WriteLine($"Backup successful. File saved at: {backupFilePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during backup: {ex.Message}");
                        // Handle exception or throw it further
                    }
                }
            }
            //public void BackupDatabase(string backupFilePath)
            //{
            //    using (var connection = new SqlConnection(_connectionString))
            //    {
            //        var commandText = $"BACKUP DATABASE Mi_Db TO DISK='{backupFilePath}'";
            //        using (var command = new SqlCommand(commandText, connection))
            //        {
            //            connection.Open();
            //            command.ExecuteNonQuery();
            //        }
            //    }
            //}
        }

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

                    // Daily Backup
                    if (_configuration.GetValue<bool>("BackupSettings:Daily"))
                    {
                        string dailyBackupPath = $"D:\\Backups\\Daily";
                        Directory.CreateDirectory(dailyBackupPath);
                        string backupFilePath = $"{dailyBackupPath}\\backup_{now:yyyyMMdd}.sql";
                        _backupService.BackupDatabase(backupFilePath);
                        _logger.LogInformation($"Daily backup performed at {now}");
                    }

                    // Weekly Backup
                    if (_configuration.GetValue<bool>("BackupSettings:Weekly") && now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        string weeklyBackupPath = $"D:\\Backups\\Weekly";
                        Directory.CreateDirectory(weeklyBackupPath);
                        string backupFilePath = $"{weeklyBackupPath}\\backup_{now:yyyyMMdd}.sql";
                        _backupService.BackupDatabase(backupFilePath);
                        _logger.LogInformation($"Weekly backup performed at {now}");
                    }

                    // Monthly Backup
                    if (_configuration.GetValue<bool>("BackupSettings:Monthly") && now.Day == 1)
                    {
                        string monthlyBackupPath = $"D:\\Backups\\Monthly";
                        Directory.CreateDirectory(monthlyBackupPath);
                        string backupFilePath = $"{monthlyBackupPath}\\backup_{now:yyyyMMdd}.sql";
                        _backupService.BackupDatabase(backupFilePath);
                        _logger.LogInformation($"Monthly backup performed at {now}");
                    }

                    // Quarterly Backup
                    if (_configuration.GetValue<bool>("BackupSettings:Quarterly") && (now.Month == 1 || now.Month == 4 || now.Month == 7 || now.Month == 10) && now.Day == 1)
                    {
                        string quarterlyBackupPath = $"D:\\Backups\\Quarterly";
                        Directory.CreateDirectory(quarterlyBackupPath);
                        string backupFilePath = $"{quarterlyBackupPath}\\backup_{now:yyyyMMdd}.sql";
                        _backupService.BackupDatabase(backupFilePath);
                        _logger.LogInformation($"Quarterly backup performed at {now}");
                    }

                    // Yearly Backup
                    if (_configuration.GetValue<bool>("BackupSettings:Yearly") && now.Month == 1 && now.Day == 1)
                    {
                        string yearlyBackupPath = $"D:\\Backups\\Yearly";
                        Directory.CreateDirectory(yearlyBackupPath);
                        string backupFilePath = $"{yearlyBackupPath}\\backup_{now:yyyyMMdd}.sql";
                        _backupService.BackupDatabase(backupFilePath);
                        _logger.LogInformation($"Yearly backup performed at {now}");
                    }

                    // Wait for 24 hours before the next check
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
            }
        }
    }
}
