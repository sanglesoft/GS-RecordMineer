using GSRecordMining.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GSRecordMining.Services
{
    public class DBService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly EncodeService _encodeService;
        private readonly SmbService _smbService;

        public DBService(IServiceScopeFactory scopeFactory, EncodeService encodeService, SmbService smbService)
        {
            _encodeService = encodeService;
            _scopeFactory = scopeFactory;
            _smbService = smbService;
        }
        public async Task<Models.ResponseViewModel> verifyLogin(Models.SystemUser systemUser)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                RecordContext _context = scope.ServiceProvider.GetRequiredService<GSRecordMining.Models.RecordContext>();
                try
                {

                    if (!_context.SystemUsers.Any())
                    {
                        _context.SystemUsers.Add(new SystemUser()
                        {
                            Alias = "Admin",
                            Password = _encodeService.PBKDF2Encode("Admin@123"),
                        });
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    return new ResponseViewModel()
                    {
                        statusCode = 500,
                        message = "cannot init authentication",
                        data = new
                        {
                            ex.Message,
                            ex.StackTrace,
                            ex.Source
                        }
                    };
                }
                try
                {
                    if(systemUser==null || systemUser.Alias==null || systemUser.Alias.Trim() == "" || systemUser.Password == null || systemUser.Password.Trim() == "")
                    {
                        return new ResponseViewModel()
                        {
                            statusCode = 400,
                            message = "input invalid",
                            data = ""
                        };
                    }
                    var data = _context.SystemUsers.FirstOrDefault(c => c.Alias.ToLower() == systemUser.Alias.ToLower());
                    if (data != null)
                    {
                        if (_encodeService.PBKDF2Verify(data.Password, systemUser.Password))
                        {
                            
                            return new ResponseViewModel()
                            {
                                statusCode = 200,
                                message = "success",
                                data = data.Alias
                            };
                        }
                        else
                        {
                            return new ResponseViewModel()
                            {
                                statusCode = 400,
                                message = "password not match",
                                data = ""
                            };
                        }
                    }
                    else
                    {
                        return new ResponseViewModel()
                        {
                            statusCode = 400,
                            message = "user not found",
                            data = ""
                        };
                    }
                }

                catch (Exception ex)
                {
                    return new ResponseViewModel()
                    {
                        statusCode = 500,
                        message = ex.Message,
                        data = new
                        {
                            ex.Message,
                            ex.StackTrace,
                            ex.Source,
                            ex.Data
                        }
                    };
                }
            }
            

        }
        public string getstring(string id)
        {
            return _encodeService.FromHexString(id);
        }
        public async Task<Models.ResponseViewModel> updateCDRDurationIndexedRecord(Models.IndexedRecord indexedRecord)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                RecordContext _context = scope.ServiceProvider.GetRequiredService<GSRecordMining.Models.RecordContext>();

                try
                {
                    if (indexedRecord == null)
                    {
                        return new ResponseViewModel()
                        {
                            statusCode = 400,
                            message = "input invalid",
                            data = ""
                        };
                    }
                    var data = await _context.IndexedRecords.FirstOrDefaultAsync(c=>c.FilePath==indexedRecord.FilePath);
                    if(data != null)
                    {
                        data.Durration = indexedRecord.Durration;
                        await _context.SaveChangesAsync();
                    }
                    return new ResponseViewModel()
                    {
                        statusCode = 200,
                        message = "",
                        data = data
                    };
                }

                catch (Exception ex)
                {
                    return new ResponseViewModel()
                    {
                        statusCode = 500,
                        message = ex.Message,
                        data = new
                        {
                            ex.Message,
                            ex.StackTrace,
                            ex.Source,
                            ex.Data
                        }
                    };
                }
            }
        }

        public async Task<Models.ResponseViewModel> buildIndexedRecord()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                RecordContext _context = scope.ServiceProvider.GetRequiredService<GSRecordMining.Models.RecordContext>();

                try
                {
                    List<string> data = new List<string>();
                    var nas = _context.NAS.FirstOrDefault();
                    if (nas != null)
                        data = _smbService.listCDRFromSMB2Connection(nas);

                    List<Models.IndexedRecord> indexing = new List<IndexedRecord>();
                    foreach(string file in data)
                    {
                        if(!_context.IndexedRecords.Any(c=>c.FilePath == file))
                        {
                            string filename = file.Split(@"\").Last();
                            indexing.Add(new IndexedRecord()
                            {
                                FilePath = file,
                                FileName = filename,
                                From = filename.Split(@"-")[2],
                                To = filename.Split(@"-")[3].Split(@".")[0],
                                Time = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(long.Parse(filename.Split(@"-")[1]))
                            });
                        }
                    }
                    await _context.IndexedRecords.AddRangeAsync(indexing);
                    await _context.SaveChangesAsync();

                    return new ResponseViewModel()
                    {
                        statusCode = 200,
                        message = "",
                        data = new { Total=_context.IndexedRecords.Count(),Add= indexing.Count,addData=indexing }
                    };
                }

                catch (Exception ex)
                {
                    return new ResponseViewModel()
                    {
                        statusCode = 500,
                        message = ex.Message,
                        data = new
                        {
                            ex.Message,
                            ex.StackTrace,
                            ex.Source,
                            ex.Data
                        }
                    };
                }
            }
        }

        public async Task<Models.ResponseViewModel> getFromNAS()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                
                List<string> data = new List<string>();

                try
                {
                    RecordContext _context = scope.ServiceProvider.GetRequiredService<GSRecordMining.Models.RecordContext>();
                    var nas = _context.NAS.FirstOrDefault();
                    if (nas != null)
                        data = _smbService.listCDRFromSMB2Connection(nas);

                    return new ResponseViewModel()
                    {
                        statusCode = 200,
                        message = "",
                        data = data
                    };
                }

                catch (Exception ex)
                {
                    return new ResponseViewModel()
                    {
                        statusCode = 500,
                        message = ex.Message,
                        data = new
                        {
                            ex.Message,
                            ex.StackTrace,
                            ex.Source,
                            ex.Data
                        }
                    };
                }
            }
        }
        public async Task<FileStreamResult> getCDR(string filename)
        {

            using (var scope = _scopeFactory.CreateScope())
            {
                RecordContext _context = scope.ServiceProvider.GetRequiredService<GSRecordMining.Models.RecordContext>();

                try
                {
                    var nas = _context.NAS.FirstOrDefault();
                    if (nas != null)
                    return _smbService.getCDRFromSMB2Connection(nas, _encodeService.FromHexString(filename));
                }

                catch
                {
                }
            }
            return null;

        }
        public async Task<Models.ResponseViewModel> getNASConfig() {

            using (var scope = _scopeFactory.CreateScope())
            {
                RecordContext _context = scope.ServiceProvider.GetRequiredService<GSRecordMining.Models.RecordContext>();

                try
                {
                    var nas = await _context.NAS.ToListAsync();
                    if (nas.Any())
                    {
                        
                        return new ResponseViewModel()
                        {
                            statusCode = 200,
                            message = "",
                            data = nas
                        };
                    }
                    else
                    {
                        return new ResponseViewModel()
                        {
                            statusCode = 200,
                            message = "",
                            data = new List<NAS>()
                        };
                    }
                }

                catch (Exception ex)
                {
                    return new ResponseViewModel()
                    {
                        statusCode = 500,
                        message = ex.Message,
                        data = new List<NAS>()
                    };
                }
            }
        }
        public async Task<Models.ResponseViewModel> checkNASConfig() {

            using (var scope = _scopeFactory.CreateScope())
            {
                RecordContext _context = scope.ServiceProvider.GetRequiredService<GSRecordMining.Models.RecordContext>();
                try
                {
                    var nas = await _context.NAS.FirstOrDefaultAsync();
                    if (nas !=null)
                    {
                        
                        return new ResponseViewModel()
                        {
                            statusCode = 200,
                            message = "",
                            data = _smbService.IsValidSMB1Connection(nas)
                        };
                    }
                    else
                    {
                        return new ResponseViewModel()
                        {
                            statusCode = 200,
                            message = "",
                            data = false
                        };
                    }
                }

                catch (Exception ex)
                {
                    return new ResponseViewModel()
                    {
                        statusCode = 500,
                        message = ex.Message,
                        data = false
                    };
                }
            }
        }
        public async Task<Models.ResponseViewModel> saveNASConfig(Models.NAS nas)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                RecordContext _context = scope.ServiceProvider.GetRequiredService<GSRecordMining.Models.RecordContext>();
                try
                {
                    nas.Username = nas.Username == null ? "" : nas.Username;
                    nas.Password = nas.Password == null ? "" : nas.Password;
                    nas.Host = nas.Host == null ? "" : nas.Host;
                    nas.Sharename = nas.Sharename == null ? "" : nas.Sharename;
                    if (nas.Host !=""&&nas.Sharename!=""&&_smbService.IsValidSMB1Connection(nas))
                    {
                        if (_context.NAS.Any())
                        {
                            _context.NAS.Update(nas);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            _context.NAS.Add(nas);
                            await _context.SaveChangesAsync();
                        }
                        return new ResponseViewModel()
                        {
                            statusCode = 200,
                            message = "",
                            data = nas
                        };
                    }
                    else
                    {
                        return new ResponseViewModel()
                        {
                            statusCode = 400,
                            message = "input invalid",
                            data = nas
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new ResponseViewModel()
                    {
                        statusCode = 500,
                        message = ex.Message,
                        data = new
                        {
                            ex.Message,
                            ex.StackTrace,
                            ex.Source,
                            ex.Data
                        }
                    };
                }
            }
        }
        public async Task<Models.ResponseViewModel> getIndexedRecord(Models.Filter? filter)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                RecordContext _context = scope.ServiceProvider.GetRequiredService<GSRecordMining.Models.RecordContext>();

                try
                {
                    if ( filter == null || (filter.Start == "" && filter.End == "") )
                    {
                        return new ResponseViewModel()
                        {
                            statusCode = 200,
                            message = "Filter falure",
                            data = new List<string>()
                        };
                    }
                    filter.From = String.IsNullOrEmpty(filter.From) ? "": filter.From.Trim();
                    filter.To = String.IsNullOrEmpty(filter.To) ? "" : filter.To.Trim();
                    var data = await _context.IndexedRecords
                        .Where(c =>
                        (c.Time >= filter.dStart && c.Time <= filter.dEnd)
                        &&
                        (
                         (filter.From != "" && filter.From == filter.To && (filter.To == c.From || filter.To == c.To))
                         ||
                        (
                        (filter.From == "" || (filter.From != "" && c.From == filter.From))
                        && (filter.To == "" || (filter.To != "" && c.To == filter.To))
                        )
                        )

                        )
                        .Select(c => c.FilePath).ToListAsync();
                    return new ResponseViewModel()
                    {
                        statusCode = 200,
                        message = "",
                        data = data
                    };
                }

                catch (Exception ex)
                {
                    return new ResponseViewModel()
                    {
                        statusCode = 500,
                        message = ex.Message,
                        data = new
                        {
                            ex.Message,
                            ex.StackTrace,
                            ex.Source,
                            ex.Data
                        }
                    };
                }
            }
        }
    }
}
