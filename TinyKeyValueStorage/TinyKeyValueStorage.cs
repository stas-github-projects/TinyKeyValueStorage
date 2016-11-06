using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TinyKeyValueStorage
{
    public class TinyKeyValueStorage
    {
        //int yyy = 0;
        public bool open(string storage_name, params string[] parameters)
        {
            bool bool_ret = false;

            Globals.storage_filename = storage_name;
            bool_ret = Globals._io.open_storage();

            return bool_ret;
        }

        public bool set(Document document)
        {
            //bool bool_ret = false;
            Task<bool> task_set = set_async(document);
            task_set.Wait();
            //result
            return task_set.Result;
        }

        private async Task<bool> set_async(Document document)
        {
            bool bool_ret = false;

            //buffers
            //byte b_type;
            byte[] b_doc_tag;
            byte[] b_data;
            //byte[] b_data_len;
            byte[] b_hash;
            byte[] b_attrib_name;
            //string sname = "";

            //add document.tag
            document.Add("document.tag", document.tag); //add document_tag as one of attributes
            b_doc_tag = BitConverter.GetBytes(Globals._hash.CreateHash64bit(Encoding.ASCII.GetBytes(document.tag)));

            int i = 0, i_index_len = 0, i_doc_len = 0, i_index_pos = 0, i_docs_pos = 0, i_data_len = 0, i_attrib_count = document.key.Count;
            long l_time = DateTime.Now.Ticks, l_pos_to_attrib = 0;
            if (Globals.ToSave.l_virtual_data_length == 0)
            { Globals.ToSave.l_virtual_data_length = Globals._io.docs_file_length(); }

            try
            {
                //index
                //List<byte[]> lst_index = new List<byte[]>(10);
                byte[] b_index = new byte[0];
                //docs
                //List<byte[]> lst_docs = new List<byte[]>(10);
                byte[] b_docs = new byte[0];

                //data buffer
                List<byte> lst_data_type = new List<byte>();
                List<byte[]> lst_data = new List<byte[]>();
                List<int> lst_data_len = new List<int>();

                //convert all data to byte arrays

                //get data lengths
                //go thru all attributes and get all data
                for (i = 0; i < i_attrib_count; i++)
                {
                    lst_data_type.Add(Globals._dataserializer.returnTypeAndRawByteArray(document.value[i], out b_data));
                    lst_data.Add(b_data);
                    lst_data_len.Add(b_data.Length);
                    i_data_len += b_data.Length;
                }

                //headers
                //index - active [1], doc_id [8], document_tag [8], full_index_length [4] - skip last element - 'document.tag'
                i_index_len = Globals.ToSave.i_docs_index_header_length + (Globals.ToSave.i_docs_index_element_length * (i_attrib_count - 1));
                b_index = new byte[i_index_len];
                Globals._service.InsertBytes(ref b_index, (byte)1, i_index_pos); i_index_pos++; //active
                Globals._service.InsertBytes(ref b_index, BitConverter.GetBytes(Globals.storage_document_id), i_index_pos); i_index_pos+=8; //doc_id
                Globals._service.InsertBytes(ref b_index, b_doc_tag, i_index_pos); i_index_pos+=8; //doc_tag
                Globals._service.InsertBytes(ref b_index,  BitConverter.GetBytes(i_index_len), i_index_pos); i_index_pos+=4; //index_full_len

                //docs - active [1], data_type [1], attrib_hash [8], attrib_name [x], attrib_len [4] + data_len
                i_doc_len = Globals.ToSave.i_docs_data_header_length + (Globals.ToSave.i_docs_data_element_length * i_attrib_count) + i_data_len;
                b_docs = new byte[i_doc_len];
                Globals._service.InsertBytes(ref b_docs, (byte)1, i_docs_pos); i_docs_pos++; //active
                Globals._service.InsertBytes(ref b_docs, BitConverter.GetBytes(Globals.storage_document_id), i_docs_pos); i_docs_pos += 8; //doc_id
                Globals._service.InsertBytes(ref b_docs, b_doc_tag, i_docs_pos); i_docs_pos += 8; //doc_tag
                Globals._service.InsertBytes(ref b_docs, BitConverter.GetBytes(l_time), i_docs_pos); i_docs_pos += 8; //created_at
                Globals._service.InsertBytes(ref b_docs, BitConverter.GetBytes(l_time), i_docs_pos); i_docs_pos += 8; //changed_at

                //go thru all attributes
                for (i = 0; i < i_attrib_count; i++)
                {
                    //doc - active [1], data_type [1], attrib_hash [8], attrib_name [x], attrib_len [4] + data
                    Globals._service.InsertBytes(ref b_docs, (byte)1, i_docs_pos); i_docs_pos++; //active
                    Globals._service.InsertBytes(ref b_docs, lst_data_type[i], i_docs_pos); i_docs_pos++; //data_type
                    //Globals._service.InsertBytes(ref b_docs, Globals._dataserializer.returnTypeAndRawByteArray(document.value[i], out b_data));
                    //i_docs_pos++; //data_type
                    //i_data_len = b_data.Length;
                    //b_data_len = BitConverter.GetBytes(i_data_len); //data_len
                    b_attrib_name = Encoding.ASCII.GetBytes(document.key[i]); //attrib_name
                    b_hash = BitConverter.GetBytes(Globals._hash.CreateHash64bit(b_attrib_name));
                    Globals._service.InsertBytes(ref b_docs, b_hash, i_docs_pos); i_docs_pos += 8; //attrib_hash
                    Globals._service.InsertBytes(ref b_docs, b_attrib_name, i_docs_pos); i_docs_pos += Globals.storage_max_attribute_name_length; //attrib_name
                    Globals._service.InsertBytes(ref b_docs, BitConverter.GetBytes(lst_data_len[i]), i_docs_pos); i_docs_pos += 4; //attrib_data_len
                    //Globals.ToSave.i_docs_data_full_length += Globals.ToSave.i_docs_data_header_length;
                    Globals._service.InsertBytes(ref b_docs, lst_data[i], i_docs_pos); i_docs_pos += lst_data_len[i]; //attrib_data
                    //Globals.ToSave.i_docs_data_full_length += i_data_len;
                    //calculate pos of attribute (data_len + data)
                    l_pos_to_attrib = (i_docs_pos - lst_data_len[i] - 4); //- data - data_len

                    //index - active_attrib [1], attrib_hash [8], attrib_pos [8]
                    if ((i + 1) != i_attrib_count) //skip last element - 'document.tag'
                    {
                        Globals._service.InsertBytes(ref b_index, (byte)1, i_index_pos); i_index_pos++; //active
                        if (lst_data_len[i] <= 8) //equal or less than 8 bytes
                        {
                            Globals._service.InsertBytes(ref b_index, (byte)1, i_index_pos); i_index_pos++; //attrib_data_len_more_than_8
                            Globals._service.InsertBytes(ref b_index, b_hash, i_index_pos); i_index_pos += 8; //index_attrib_hash
                            Globals._service.InsertBytes(ref b_index, lst_data[i], i_index_pos); i_index_pos += 8; //index_attrib_data
                        }
                        else
                        {
                            Globals._service.InsertBytes(ref b_index, (byte)0, i_index_pos); i_index_pos++; //attrib_data_len_more_than_8
                            Globals._service.InsertBytes(ref b_index, b_hash, i_index_pos); i_index_pos += 8; //index_attrib_hash
                            Globals._service.InsertBytes(ref b_index, BitConverter.GetBytes(l_pos_to_attrib), i_index_pos); i_index_pos += 8; //index_attrib_pos
                        }
                    }
                }//for - attributes

                //add to save list
                Globals.ToSave.lst_index_to_save.Add(b_index);
                Globals.ToSave.lst_docs_to_save.Add(b_docs);

                Globals.ToSave.i_docs_data_full_length += i_doc_len;
                Globals.ToSave.i_docs_index_full_length += i_index_len;

                Globals.storage_document_id++;
                Globals.ToSave.l_virtual_data_length += i_docs_pos;
                bool_ret = true;
                //Globals.ToSave.index_chunks_count += (document.GetCount() % Globals.storage_max_attributes_per_index_on_disk); //chunks to save
            }
            catch (Exception) { return false; }

            //return
            return await Task.FromResult(bool_ret);
        }

        public bool query(string query)
        {
            bool bool_ret = false;

            //parse query
            Globals._converter.parse_query(query);
            //start search
            Task<bool> task_query = query_async();
            task_query.Wait();

            return bool_ret;
        }

        private async Task<bool> query_async()
        {
            bool bool_ret = false;



            return await Task.FromResult(bool_ret);
        }

        public bool update(string path, object new_value)
        {
            bool bool_ret = false;



            return bool_ret;
        }

        public bool commit()
        {
            Task<bool> task_commit = commit_async();
            task_commit.Wait();
            //clear
            Globals.ToSave.flush();
            //result
            return task_commit.Result;
        }

        private async Task<bool> commit_async()
        {
            bool bool_ret = false;

            if (Globals.ToSave.lst_docs_to_save.Count == 0) { return false; }

            byte[] b_docs = new byte[0];
            byte[] b_indexes = new byte[0];
            b_docs = Globals._converter.DocumentsToBytes(out b_indexes); //get documents + indexes

            //write
            if (Globals._io.is_open() == false)
            { Globals._io.open_storage(); }//reopen storage                            
            //write
            bool_ret = Globals._io.write(ref b_indexes, IO.IO_FILE.INDEX);
            if (bool_ret == true) //write & close connection
            {
                bool_ret = Globals._io.write(ref b_docs, IO.IO_FILE.DATA);
                bool_ret = Globals._io.close();
            }
            else //close connection
            { Globals._io.close(); }

            //result
            return await Task.FromResult(bool_ret);
        }

        //
        //SPECIALS
        //
        //store info from client side
        public class Document
        {
            public string tag = ""; //main identifier of the document (something like 'folder' in filesystems)
            internal List<string> key = new List<string>(10);
            internal List<dynamic> value = new List<dynamic>(10);

            public Document(string tag) { this.tag = tag; }
            public Document(string key, dynamic value)
            { this.Add(key, value); }
            public void Add(string key, dynamic value)
            { this.key.Add(key); this.value.Add(value); }
            public int GetCount()
            { return this.key.Count; }
        }

    }
}
