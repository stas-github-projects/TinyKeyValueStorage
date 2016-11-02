using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TinyKeyValueStorage
{
    internal class IO
    {
        private FileStream filestream_index;
        private FileStream filestream_data;

        internal bool open_storage()
        {
            bool bool_ret = true;

            string sindex = Globals.storage_filename + ".tki";
            string sdata = Globals.storage_filename + ".tkd";

            FileInfo finfo = new FileInfo(sindex);
            //FileInfo finfo2 = new FileInfo(sdata);
            if (finfo.Exists == false) //create new
            {
                this.filestream_index = new FileStream(sindex, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, Globals.storage_file_buffer_size, true);
                this.filestream_data = new FileStream(sdata, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, Globals.storage_file_buffer_size, true);
            }
            else //open existing
            {
                this.filestream_index = new FileStream(sindex, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, Globals.storage_file_buffer_size, true);
                this.filestream_data = new FileStream(sdata, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, Globals.storage_file_buffer_size, true);
            }

            return bool_ret;
        }

        internal bool is_exist_index()
        {
            return (this.filestream_index == null) ? false : true;
        }

        internal bool is_data_index()
        {
            return (this.filestream_data == null) ? false : true;
        }

        internal bool is_open()
        {
            if (this.filestream_index == null || this.filestream_data == null) { return false; }
            else { return true; }
        }

        internal bool close()
        {
            try { this.filestream_index.Close(); this.filestream_data.Close(); return true; }
            catch (Exception) { return false; }
        }

        internal bool write(ref byte[] b_data, IO_FILE io_file_type)
        {
            bool bool_ret = true;

            try
            {
                if (io_file_type == IO_FILE.INDEX)
                {
                    this.filestream_index.Position = this.filestream_index.Length;
                    this.filestream_index.Write(b_data, 0, b_data.Length);
                    this.filestream_index.Position = 0;
                }
                else if (io_file_type == IO_FILE.DATA)
                {
                    this.filestream_data.Position = this.filestream_data.Length;
                    this.filestream_data.Write(b_data, 0, b_data.Length);
                    this.filestream_data.Position = 0;
                }
            }
            catch (Exception e) { bool_ret = false; }

            return bool_ret;
        }


        internal enum IO_FILE
        { INDEX = 0, DATA = 1 }
    }
}