using CSCore;
using CSCore.Codecs.WAV;
using GSRecordMining.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SMBLibrary;
using SMBLibrary.Client;
using SMBLibrary.SMB1;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace GSRecordMining.Services
{
    public class SmbService
    {
        private readonly EncodeService _encodeService;

        public SmbService(EncodeService encodeService)
        {
            _encodeService = encodeService;
        }

        public bool IsValidSMB1Connection(Models.NAS nas)
        {
            IPAddress ip;
            bool ValidateIP = IPAddress.TryParse(nas.Host, out ip);
            
            try
            {
                var smb = new SMB1Client();
                NTStatus actionStatus;
                if (((ValidateIP && smb.Connect(ip, SMBTransportType.DirectTCPTransport))
                    || (!ValidateIP && smb.Connect(nas.Host, SMBTransportType.DirectTCPTransport)))
                    && NTStatus.STATUS_SUCCESS == smb.Login(string.Empty, nas.Username, nas.Password)
                    && smb.ListShares(out actionStatus).Any(c => c.ToUpper() == nas.Sharename.ToUpper()))
                {
                    smb.Disconnect();

                    return true;
                }
            }
            catch
            {
            }
                return false;
        }
        public List<string> getShareFromSMB1Connection(Models.NAS nas)
        {
            IPAddress ip;
            bool ValidateIP = IPAddress.TryParse(nas.Host, out ip);
            var smb = new SMB1Client();
            SMBLibrary.NTStatus actionStatus;
            try
            {
                if (
                    (ValidateIP && smb.Connect(ip, SMBLibrary.SMBTransportType.DirectTCPTransport))
                    || (!ValidateIP && smb.Connect(nas.Host, SMBLibrary.SMBTransportType.DirectTCPTransport))
                    )
                {
                    var status = smb.Login(string.Empty, nas.Username, nas.Password);
                    var shares = smb.ListShares(out actionStatus);
                    smb.Disconnect();
                    return shares;
                }
                else
                {
                    return new List<string>();
                }
            }
            catch
            {
                return new List<string>();

            }
        }
        public List<string> getFileInSMB2Connection( string root, ISMBFileStore tree)
        {
            NTStatus actionStatus;
            object directoryHandle;
            FileStatus fileStatus;

            actionStatus = tree.CreateFile(out directoryHandle, out fileStatus, root, AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
            if (actionStatus == NTStatus.STATUS_SUCCESS)
            {
                List<QueryDirectoryFileInformation> fileList = new List<QueryDirectoryFileInformation>();
                try
                {
                    tree.QueryDirectory(out fileList, directoryHandle, "*", FileInformationClass.FileDirectoryInformation);
                    tree.CloseFile(directoryHandle);
                }
                catch
                {
                }
                List<string> files = new List<string>();
                foreach (FileDirectoryInformation file in fileList)
                {
                    if(file.FileName!="."&& file.FileName != "..")
                    {
                        if(file.FileAttributes == SMBLibrary.FileAttributes.Directory)
                        {
                            files.AddRange(getFileInSMB2Connection((root != String.Empty ? root + "\\" : "") + file.FileName, tree));
                        }
                        else
                        {
                            files.Add( (root != String.Empty ? root + "\\" : "") + file.FileName);
                        }
                    }
                }
                return files;
            }
            else return new List<string>();
        }
        public List<string> listCDRFromSMB2Connection(Models.NAS nas)
        {
            IPAddress ip;
            bool ValidateIP = IPAddress.TryParse(nas.Host, out ip);
            var smb = new SMB2Client();
            NTStatus actionStatus;
            List<string> cdrFiles = new List<string>();
            try
            {
                if (
                    (ValidateIP && smb.Connect(ip, SMBTransportType.DirectTCPTransport))
                    || (!ValidateIP && smb.Connect(nas.Host, SMBTransportType.DirectTCPTransport))
                    )
                {
                    if(NTStatus.STATUS_SUCCESS== smb.Login(string.Empty, nas.Username, nas.Password))
                    {
                        ISMBFileStore tree = smb.TreeConnect(nas.Sharename, out actionStatus);
                        if (actionStatus == NTStatus.STATUS_SUCCESS)
                        {
                            cdrFiles = getFileInSMB2Connection(string.Empty, tree).Where(c=>c.EndsWith(".wav")).ToList();
                        }
                        tree.Disconnect();
                    }
                    smb.Disconnect();
                }
                else
                {
                }
            }
            catch
            {
            }
            return cdrFiles;

        }
        public FileStreamResult getCDRFromSMB2Connection(Models.NAS nas, string filename)
        {
            IPAddress ip;
            bool ValidateIP = IPAddress.TryParse(nas.Host, out ip);
            var smb = new SMB2Client();
            NTStatus actionStatus;
            System.IO.MemoryStream stream = new System.IO.MemoryStream();

            try
            {
                if (
                    (ValidateIP && smb.Connect(ip, SMBTransportType.DirectTCPTransport))
                    || (!ValidateIP && smb.Connect(nas.Host, SMBTransportType.DirectTCPTransport))
                    )
                {
                    if (NTStatus.STATUS_SUCCESS == smb.Login(string.Empty, nas.Username, nas.Password))
                    {
                        ISMBFileStore tree = smb.TreeConnect(nas.Sharename, out actionStatus);
                        object fileHandle;
                        FileStatus fileStatus;
                        if (tree is SMB1FileStore)
                        {
                            filename = @"\\" + filename;
                        }
                        actionStatus = tree.CreateFile(out fileHandle, out fileStatus, filename, AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null);

                        if (actionStatus == NTStatus.STATUS_SUCCESS)
                        {
                            byte[] data;
                            long bytesRead = 0;
                            while (true)
                            {
                                actionStatus = tree.ReadFile(out data, fileHandle, bytesRead, (int)smb.MaxReadSize);
                                if (actionStatus != NTStatus.STATUS_SUCCESS && actionStatus != NTStatus.STATUS_END_OF_FILE)
                                {
                                    throw new Exception("Failed to read from file");
                                }

                                if (actionStatus == NTStatus.STATUS_END_OF_FILE || data.Length == 0)
                                {
                                    break;
                                }
                                bytesRead += data.Length;
                                stream.Write(data, 0, data.Length);
                            }
                        }
                        tree.CloseFile(fileHandle);
                        tree.Disconnect();
                    }
                }
            }
            catch { }
            stream.Position = 0;
            return new FileStreamResult(stream, "audio/wav");
        }


    }
    

}
