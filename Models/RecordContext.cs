using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using CSCore;
using CSCore.Codecs.WAV;

namespace GSRecordMining.Models
{
    public class RecordContext : DbContext
    {
        public RecordContext()
        {
            if(Directory.Exists(DBDirectory()))
            {
                Directory.CreateDirectory(DBDirectory());
            }
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            
            options.UseSqlite($"Data Source={DBDirectory()}{Path.DirectorySeparatorChar}IndexedRecord.db");
        }
        private string DBDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (baseDir.Contains("bin"))
            {
                int index = baseDir.IndexOf("bin");
                baseDir = baseDir[..index];
            }
            return $"{baseDir}{Path.DirectorySeparatorChar}AppData";
        }
        public DbSet<SystemUser> SystemUsers => Set<SystemUser>();
        public DbSet<IndexedRecord> IndexedRecords => Set<IndexedRecord>();
        public DbSet<NAS> NAS => Set<NAS>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemUser>().ToTable("SystemUser");
            modelBuilder.Entity<IndexedRecord>().ToTable("IndexedRecord");
            modelBuilder.Entity<NAS>().ToTable("NAS");
        }

    }
    public class SystemUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }
        public string Alias { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        
    }
    public class NAS
    {
        [Key]
        public string Host { get; set; } = string.Empty;
        public string Sharename { get; set; } = string.Empty;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        
    }
    public class Filter
    {
        public DateTime? dStart { get {
            return DateTime.MinValue; 
            
            } }
        public DateTime? dEnd
        {
            get
            {
                return DateTime.MinValue;

            }
        }
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;

    }
    public class IndexedRecord
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }
        public string FilePath { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string FileName
        {
            get;set;
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public DateTime Time
        {
            get; set;

        }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string From
        {
            get; set;

        }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string To
        {
            get; set;

        }

        public TimeSpan Durration
        {
            get; set;

        }
        /*public string FileName { 
            get {
                return FilePath.Split(@"\").Last();
            } 
        } 
        public DateTime Time { get {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(long.Parse(FileName.Split(@"-")[1]));
            } }
        public string From { get {
                return FileName.Split(@"-")[2];
            } } 
        public string To { get {
                return FileName.Split(@"-")[3].Split(@".")[0];
            } }
        public TimeSpan Durration { 
            get {

                return new TimeSpan(0);
            } 
        } */

    }

}
