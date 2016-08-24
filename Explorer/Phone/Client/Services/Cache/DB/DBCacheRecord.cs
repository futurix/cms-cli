using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Data.Linq.Mapping;
using Wave.Common;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    [Index(Columns = "CIID", Name = "i_CIID")]
    [Index(Columns = "Key", Name = "i_Key")]
    [Table]
    public sealed class DBCacheRecord : ICacheRecord, INotifyPropertyChanging, INotifyPropertyChanged
    {
        /// <summary>
        /// If the file is smaller than this value, it will be stored in the database. Otherwise
        /// it will be saved as separate file.
        /// </summary>
        /// <remarks>If you are changing this value, don't forget to sync it to the corresponding column size.</remarks>
        public const int FileSizeCutOff = 4096;

        /// <summary>
        /// Maximum size of key binary.
        /// </summary>
        /// <remarks>If you are changing this value, don't forget to sync it to the corresponding column size.</remarks>
        public const int KeySizeMaximum = 2048;
        
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        private long id;
        private Binary ciid;
        private Binary key;
        private short entityType;
        private short cacheMode;
        private string dataFile;
        private Binary data;

        public DBCacheRecord()
        {
        }

        #region Columns

        [Column(Name = "ID", Storage = "id", AutoSync = AutoSync.OnInsert, DbType = "BigInt NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true)]
        public long ID
        {
            get
            {
                return id;
            }
            set
            {
                if (id != value)
                {
                    NotifyPropertyChanging("ID");
                    id = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }

        [Column(Name = "CIID", Storage = "ciid", DbType = "VarBinary(16)", CanBeNull = true)]
        public Binary CIID
        {
            get
            {
                return ciid;
            }
            set
            {
                if (ciid != value)
                {
                    NotifyPropertyChanging("CIID");
                    ciid = value;
                    NotifyPropertyChanged("CIID");
                }
            }
        }

        [Column(Name = "Key", Storage = "key", DbType = "VarBinary(2048)", CanBeNull = true)]
        public Binary Key
        {
            get { return key; }
            set
            {
                if (key != value)
                {
                    NotifyPropertyChanging("Key");
                    key = value;
                    NotifyPropertyChanged("Key");
                }
            }
        }

        [Column(Name = "EntityType", Storage = "entityType", DbType = "SmallInt NOT NULL")]
        public short EntityType
        {
            get { return entityType; }
            set
            {
                if (entityType != value)
                {
                    NotifyPropertyChanging("EntityType");
                    entityType = value;
                    NotifyPropertyChanged("EntityType");
                }
            }
        }

        [Column(Name = "CacheMode", Storage = "cacheMode", DbType = "SmallInt NOT NULL")]
        public short CacheMode
        {
            get { return cacheMode; }
            set
            {
                if (cacheMode != value)
                {
                    NotifyPropertyChanging("CacheMode");
                    cacheMode = value;
                    NotifyPropertyChanged("CacheMode");
                }
            }
        }

        [Column(Name = "DataFile", Storage = "dataFile", DbType = "NVarChar(64)")]
        public string DataFile
        {
            get { return dataFile; }
            set
            {
                if (dataFile != value)
                {
                    NotifyPropertyChanging("DataFile");
                    dataFile = value;
                    NotifyPropertyChanged("DataFile");
                }
            }
        }

        [Column(Name = "Data", Storage = "data", DbType = "VarBinary(4096)")]
        public Binary Data
        {
            get { return data; }
            set
            {
                if (data != value)
                {
                    NotifyPropertyChanging("Data");
                    data = value;
                    NotifyPropertyChanged("Data");
                }
            }
        }

        #endregion

        #region Data management

        public void SetData(byte[] newData)
        {
            if (newData == null)
                return;

            if (newData.Length > FileSizeCutOff)
            {
                // saving to external file
                string fileNameID = Guid.NewGuid().ToString();

                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    IsolatedStorageFileStream fs = 
                        isf.CreateFile(
                            String.Format(
                                CacheStore.CacheFilePathFormat, 
                                DBCacheStore.CacheDirectory, fileNameID, DBCacheStore.CacheFileExtension));

                    using (fs)
                        fs.WriteBytes(newData);
                }

                DataFile = fileNameID;
                Data = null;
            }
            else
            {
                // save directly in the database
                DataFile = null;
                Data = newData;
            }
        }

        public void RemoveLinkedFile()
        {
            if (!String.IsNullOrWhiteSpace(DataFile))
            {
                string filePath = String.Format(CacheStore.CacheFilePathFormat, DBCacheStore.CacheDirectory, dataFile, DBCacheStore.CacheFileExtension);

                if (IsolatedStorageHelper.FileExists(filePath))
                    IsolatedStorageHelper.DeleteFile(filePath);
            }
        }

        #endregion

        #region ICacheRecord implementation

        CacheItemID? ICacheRecord.CIID
        {
            get
            {
                if (CIID != null)
                    return new CacheItemID(CIID.ToArray());
                else
                    return null;
            }
            set
            {
                if (value.HasValue)
                    CIID = value.Value.ToByteArray();
                else
                    CIID = null;
            }
        }

        byte[] ICacheRecord.Key
        {
            get
            {
                if (Key != null)
                    return Key.ToArray();
                else
                    return null;
            }
            set
            {
                Key = value;
            }
        }

        CacheEntityType ICacheRecord.EntityType
        {
            get { return (CacheEntityType)EntityType; }
            set { EntityType = (short)value; }
        }

        CacheMode ICacheRecord.CacheMode
        {
            get { return (CacheMode)CacheMode; }
            set { CacheMode = (short)value; }
        }

        byte[] ICacheRecord.Data
        {
            get
            {
                if (!String.IsNullOrEmpty(DataFile))
                    return IsolatedStorageHelper.ReadFile(
                        String.Format(CacheStore.CacheFilePathFormat, DBCacheStore.CacheDirectory, DataFile, DBCacheStore.CacheFileExtension));
                else if (Data != null)
                    return Data.ToArray();

                return null;
            }
            set
            {
                SetData(value);
            }
        }

        #endregion

        #region Property change methods

        public void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
