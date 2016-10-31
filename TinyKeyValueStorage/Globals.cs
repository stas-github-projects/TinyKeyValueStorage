using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TinyKeyValueStorage
{
    internal static class Globals
    {
        internal static DataTypeSerializer _dataserializer = new DataTypeSerializer();
        internal static Service _service = new Service();
        internal static HashFNV _hash = new HashFNV();
        internal static Converter _converter = new Converter();

        internal static byte storage_max_attributes_per_index_on_disk = 3;
        internal static ulong storage_document_id = 0;
        internal static byte storage_max_attribute_name_length = 20;
        
        internal static class ToSave
        {
            internal static List<DocToSave> lst_docs_to_save = new List<DocToSave>(10);
            internal static int index_chunks_count = 0; //how much chunks of all records it need to be created for index file


            //store info from server side
            internal class DocToSave
            {
                internal byte[] document_tag_hash;
                //internal byte[] document_tag_name;
                internal byte[] document_id;
                internal List<byte[]> lst_hash = new List<byte[]>(10);
                internal List<string> lst_name = new List<string>(10);
                internal List<byte> lst_data_type = new List<byte>(10);
                internal List<byte[]> lst_data = new List<byte[]>(10);
            }
        }

    }
}
