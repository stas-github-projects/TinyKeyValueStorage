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
        /*
        public bool set(string path, object value)
        {
            bool bool_ret = false;

            //int i = 0, icount = Globals.storage_collections_delim.Length + Globals.storage_keys_delim.Length;
            /*
            char[] test_delim = new char[3];
            test_delim[0] = Globals.storage_collections_delim[0];
            test_delim[1] = Globals.storage_keys_delim[0];
            test_delim[2] = Globals.storage_keys_delim[1];
            string[] sarr = path.Split(test_delim, StringSplitOptions.RemoveEmptyEntries);
            *
            //last element is KEY

            Globals.ToSave.DocToSave _doc = new Globals.ToSave.DocToSave();
            _doc.name = path;
            _doc.data_type = Globals._dataserializer.returnTypeAndRawByteArray(ref value, out _doc.data);
            Globals.ToSave.lst_docs_to_save.Add(_doc);

            //yyy++;
            //if (yyy == 1000)
            //{ Console.Title = Globals.lst_docs_to_save.Count.ToString(); yyy = 0; }


            //check for path

            //add key


            return bool_ret;
        }
        */
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

            try
            {
                byte[] b_out;
                string sname = "";
                Globals.ToSave.DocToSave _doc = new Globals.ToSave.DocToSave();
                _doc.document_id = BitConverter.GetBytes(Globals.storage_document_id);
                //_doc.document_tag_name = Encoding.ASCII.GetBytes(document.tag);
                document.Add("document.tag",document.tag); //add document_tag as one of attributes
                _doc.document_tag_hash = BitConverter.GetBytes(Globals._hash.CreateHash64bit(Encoding.ASCII.GetBytes(document.tag)));
                for (int i = 0; i < document.GetCount(); i++)
                {
                    sname = document.key[i];
                    _doc.lst_name.Add(sname);
                    _doc.lst_hash.Add(BitConverter.GetBytes(Globals._hash.CreateHash64bit(Encoding.ASCII.GetBytes(sname))));
                    _doc.lst_data_type.Add(Globals._dataserializer.returnTypeAndRawByteArray(document.value[i], out b_out));
                    _doc.lst_data.Add(b_out);
                    _doc.lst_data_len.Add(b_out.Length);
                    Globals.ToSave.i_docs_data_to_save += b_out.Length;
                }

                _doc.document_index_length = BitConverter.GetBytes((1 + 8 + 4 + (1 + 1 + 8 + 4 + 8) * document.GetCount()));
                Globals.ToSave.i_docs_tags_to_save += document.GetCount(); //max attributes to save
                Globals.ToSave.lst_docs_to_save.Add(_doc);
                Globals.storage_document_id++;
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
