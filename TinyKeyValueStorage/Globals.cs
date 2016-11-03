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
        internal static IO _io = new IO();

        //internal static byte storage_max_attributes_per_index_on_disk = 3;
        internal static ulong storage_document_id = 0;
        internal static byte storage_max_attribute_name_length = 20;

        internal static string storage_filename = "";
        internal static int storage_file_buffer_size = 1024 * 64;
        
        internal static class ToSave
        {
            internal static List<DocToSave> lst_docs_to_save = new List<DocToSave>(10);
            internal static int i_docs_tags_to_save = 0;
            internal static int i_docs_data_to_save = 0;
            //internal static int index_chunks_count = 0; //how much chunks of all records it need to be created for index file


            //store info from server side
            internal class DocToSave
            {
                internal byte[] document_tag_hash;
                //internal byte[] document_tag_name;
                internal byte[] document_id;
                internal byte[] document_index_length;
                internal List<byte[]> lst_hash = new List<byte[]>(10);
                internal List<string> lst_name = new List<string>(10);
                internal List<byte> lst_data_type = new List<byte>(10);
                internal List<byte[]> lst_data = new List<byte[]>(10);
                internal List<int> lst_data_len = new List<int>(10);
            }

            internal static void flush()
            {
                lst_docs_to_save.Clear(); i_docs_data_to_save = 0; i_docs_tags_to_save = 0;
            }
        }

        internal static class ToQuery
        {
            internal static List<ulong> lst_attribute = new List<ulong>(10); //model
            internal static List<byte> lst_operator = new List<byte>(10); // >
            internal static List<dynamic> lst_value = new List<dynamic>(10); // 10

            internal static void flush()
            {
                lst_attribute.Clear(); lst_operator.Clear(); lst_value.Clear();
            }
        }

    }
}
