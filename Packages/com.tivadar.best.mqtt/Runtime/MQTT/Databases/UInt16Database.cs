using System;
using System.Collections.Generic;
using System.IO;

using Best.HTTP.Shared.PlatformSupport.Threading;

using Best.HTTP.Shared.Databases;
using Best.HTTP.Shared.Databases.Indexing;
using Best.HTTP.Shared.Databases.MetadataIndexFinders;
using Best.HTTP.Shared.Databases.Utils;

namespace Best.MQTT.Databases
{
    internal class UInt16Metadata : Metadata
    {
        public UInt16 value;

        public override void SaveTo(Stream stream)
        {
            base.SaveTo(stream);

            stream.EncodeUnsignedVariableByteInteger(value);
        }

        public override void LoadFrom(Stream stream)
        {
            base.LoadFrom(stream);

            this.value = (UInt16)stream.DecodeUnsignedVariableByteInteger();
        }

        public override string ToString()
        {
            return $"[Metadata Idx: {Index}, IsDeleted: {IsDeleted}, Value: {value}]";
        }
    }

    internal class UInt16MetadataService : MetadataService<UInt16Metadata, UInt16>
    {
        public UInt16MetadataService(IndexingService<ushort, UInt16Metadata> indexingService)
            : base(indexingService, new FindDeletedMetadataIndexFinder<UInt16Metadata>())
        {
        }

        public UInt16Metadata Create(UInt16 value)
        {
            var result = base.CreateDefault(value, 0, 0, (content, metadata) => metadata.value = content);

            return result;
        }
    }

    internal class UInt16IndexingService : IndexingService<UInt16, UInt16Metadata>
    {
        internal AVLTree<UInt16, int> index = new AVLTree<ushort, int>(Comparer<UInt16>.Default);

        public override void Index(UInt16Metadata metadata)
        {
            base.Index(metadata);
            this.index.Add(metadata.value, metadata.Index, true);
        }

        public override void Remove(UInt16Metadata metadata)
        {
            base.Remove(metadata);
            this.index.Remove(metadata.value);
        }

        public override void Clear()
        {
            base.Clear();
            this.index.Clear();
        }

        public override IEnumerable<int> GetOptimizedIndexes() => this.index.WalkHorizontal();
    }

    internal class UInt16DiskContentParser : IDiskContentParser<UInt16>
    {
        public void Encode(Stream stream, ushort content) {}
        public ushort Parse(Stream stream, int length) => 0;
    }

    internal class UInt16DatabaseOptions : DatabaseOptions
    {
        public UInt16DatabaseOptions(string dbName) : base(dbName)
        {
            base.UseHashFile = false;
        }
    }

    internal class UInt16Database : Database<UInt16, UInt16Metadata, UInt16IndexingService, UInt16MetadataService>
    {
        public UInt16Database(string directory, string dbName, UInt16IndexingService indexingService)
            : base(directory, new UInt16DatabaseOptions(dbName), indexingService, new UInt16DiskContentParser(), new UInt16MetadataService(indexingService))
        {
        }

        public void Add(UInt16 value)
        {
            using (new WriteLock(this.rwlock))
            {
                base.MetadataService.Create(value);
                base.FlagDirty(1);
            }
        }

        public bool Contains(UInt16 key) => this.IndexingService.index.ContainsKey(key);

        public void Remove(UInt16 value)
        {
            using (new WriteLock(this.rwlock))
            {
                var metadataIndexes = this.IndexingService.index.Find(value);

                base.Delete(metadataIndexes);
            }
        }

        public (bool found, UInt16 value) GetNext(UInt16 value)
        {
            using (new ReadLock(this.rwlock))
            {
                // find the smallest one that larger than value
                UInt16 smallest = UInt16.MaxValue;
                for (int i = 0; i < this.MetadataService.Metadatas.Count; ++i)
                {
                    UInt16 currentValue = this.MetadataService.Metadatas[i].value;

                    if (currentValue < smallest && currentValue > value)
                        smallest = currentValue;
                }

                if (smallest == UInt16.MaxValue)
                    return (false, 0);

                return (true, smallest);
            }
        }

        internal ushort Count()
        {
            using (new ReadLock(this.rwlock))
                return (ushort)this.IndexingService.index.ElemCount;
        }
    }
}
