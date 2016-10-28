using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TinyKeyValueStorage
{
    internal class Converter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal byte[] DocumentsToBytes()
        {
            //active (1) + doc_id (8) + doc_tag_hash (8) - doc_tag_name stores in attributes as 'document.tag'
            //attributes
            //active (1) + data_type (1) + attrib_hash (8) + attrib_name (X) + attrib_data_len (4) + attrib_data_pos (8)
            int iindexsize = 1 + 8 + 8 + (1 + 1 + 8 + Globals.storage_max_attribute_name_length + 4 + 8) * Globals.storage_max_attributes_per_index_on_disk;
            int i = 0, ibufcount = iindexsize * Globals.ToSave.index_chunks_count;
            byte[] b_out = new byte[ibufcount];

            //if (document == null) { return new byte[0]; } //error
            for (i = 0; i < Globals.ToSave.lst_docs_to_save.Count;i++ )
            {

            }

            //return
            return b_out;
        }

    }
}
