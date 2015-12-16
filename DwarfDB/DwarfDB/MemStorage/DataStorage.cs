/*
 * Created by SharpDevelop.
 * User: Igor.Evdokimov
 * Date: 16.08.2015
 * Time: 15:59
 *
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DwarfDB.DataStructures;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace DwarfDB.Stack
{
    /// <summary>
    /// Exception for data stacks
    /// </summary>
    [Serializable]
    public class DataStorageException : Exception
    {
        public DataStorageException()
        {
        }

        public DataStorageException(string reason) : base("DataStack: " + reason)
        {
            Init();
        }

        public DataStorageException(string reason,
                                    Exception innerException) : base("DataStack: " + reason, innerException)
        {
            Init();
        }

        public DateTime When
        {
            get;
            private set;
        }

        private void Init()
        {
            When = DateTime.Now.ToLocalTime();
        }

        // This constructor is needed for serialization.
        protected DataStorageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// A stack for improving an access to dwarf records
    /// </summary>
    public class DataStorage
    {
        public DataStorage(DataBase _db)
        {
            Modified = true;
            if (_db != null)
                db = _db;

            parallel_opts.MaxDegreeOfParallelism = Environment.ProcessorCount * 4;
        }

        public new void Add(IStructure dta_struct)
        {
            string new_index_hash = dta_struct.GetIndex().HashCode;
            if (dta_struct is Record)
            {
                items_dict.TryAdd(new_index_hash, dta_struct);
                Modified = true;
            }
        }

        public bool TryRemove(IStructure data)
        {
            if (items_dict.ContainsKey(data.GetIndex().HashCode))
            {
                IStructure dummy = null;
                return items_dict.TryRemove(data.GetIndex().HashCode, out dummy);
            }

            return false;
        }

        public void Clear()
        {
            items_dict.Clear();
        }

        public int Count
        {
            get
            {
                return items_dict.Count;
            }
            private set { }
        }

        public bool ContainsHash(string hash)
        {
            return items_dict.ContainsKey(hash);
        }

        private Record GR(DataContainer dc, KeyValuePair<string, IStructure> kvp)
        {
            if (kvp.Value is Record)
            {
                if ((kvp.Value as Record).OwnerDC.Name == dc.Name) { 
                    return kvp.Value as Record;
                }
            }
            return null;
        }

        private IEnumerable<Record> GRFirstHalf( DataContainer dc)
        {
            int half_cnt = (int)(items_dict.Count() / 2) + 1;
            int i = 0;
            foreach (var kvp in items_dict)
            {
                if (i < half_cnt)
                {
                    if (kvp.Value is Record)
                    {
                        if ((kvp.Value as Record).OwnerDC.Name == dc.Name)
                            yield return (kvp.Value as Record);
                    }
                }
                else
                    break;
                ++i;
            }
        }

        private IEnumerable<Record> GRSecHalf(DataContainer dc)
        {
            var reverted = items_dict.Reverse();
            int half_cnt = (int)(reverted.Count() / 2) + 1;
            int i = 0;
            foreach (var kvp in reverted)
            {
                if (i < half_cnt)
                {
                    if (kvp.Value is Record)
                    {
                        if ((kvp.Value as Record).OwnerDC.Name == dc.Name)
                            yield return (kvp.Value as Record);
                    }
                } else
                    break;
                ++i;
            }
        }

        /// <summary>
        /// Receiving records for concrete DataContainer
        /// </summary>
        /// <param name="dc">A data container</param>
        /// <returns></returns>
        public IEnumerable<Record> GetRecords(DataContainer dc)
        {
            var first_half = GRFirstHalf(dc);
            var sec_half = GRSecHalf(dc);
            return first_half.Union(sec_half);
        }

        /// <summary>
        /// Receiving records a record with given index
        /// </summary>
        /// <param name="ind"></param>
        /// <returns></returns>
        public IStructure GetStructure(Index ind)
        {
            try
            {
                if (items_dict.ContainsKey(ind.HashCode))
                    return items_dict[ind.HashCode];
                else
                    return null;
            }
            catch (Exception ex)
            {
                Errors.ErrorProcessing.Display("FAILED TO GET STRUCTURE: " + ex.Message + ":" + ex.StackTrace,
                                               "", "", DateTime.Now);
            }
            return null;
        }

        /// <summary>
        /// Are stack elements array modified?
        /// </summary>
        public bool Modified
        {
            get; private set;
        }

        ParallelOptions parallel_opts = new ParallelOptions();
        DataBase db;
        ConcurrentDictionary<String, IStructure> items_dict = new ConcurrentDictionary<String, IStructure>();
    }
}
