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
        internal byte[] DocumentsToBytes(out byte[] b_indexes)
        {
            int idocscount = Globals.ToSave.lst_docs_to_save.Count;
            //active (1) + doc_id (8)(-deprecated) + doc_tag_hash (8) + doc_full_index_length (4)
            //attributes
            //active (1) + data_type (1) + attrib_hash (8) + attrib_data_len (4) (-deprecated) + attrib_data_pos (8)/attrib_data
            int iindexsize = (1 + 8 + 4) * idocscount + (1 + 1 + 8 + 4 + 8) * Globals.ToSave.i_docs_tags_to_save;//Globals.storage_max_attributes_per_index_on_disk;
            //active (1) + doc_id (8) + doc_tag_hash (8) - doc_tag_name stores in attributes as 'document.tag'
            //attributes
            //active (1) + data_type (1) + attrib_hash (8) + attrib_name (X) + attrib_data_len (4) + attrib_data_pos (8)
            int idocsize = (1 + 8 + 8) * idocscount + ((1 + 1 + 8 + Globals.storage_max_attribute_name_length + 4) * Globals.ToSave.i_docs_tags_to_save) + Globals.ToSave.i_docs_data_to_save;
            int i = 0, idoccount = idocsize;// = idocsize * idocscount;
            int iindexcount = iindexsize;// *idocscount;
            int idocpos = 0, iindexpos = 0;

            //byte[] b_docid;

            byte[] b_docs = new byte[idoccount];
            b_indexes = new byte[iindexcount];

            //go through all documents to save
            for (i = 0; i < idocscount; i++)
            {
                //index header info
                b_indexes[iindexpos] = 1; iindexpos++; //active
                //Globals._service.InsertBytes(ref b_indexes, Globals.ToSave.lst_docs_to_save[i].document_id, iindexpos); iindexpos += 8; //doc_id
                Globals._service.InsertBytes(ref b_indexes, Globals.ToSave.lst_docs_to_save[i].document_tag_hash, iindexpos); iindexpos += 8; //doc_tag_hash
                Globals._service.InsertBytes(ref b_indexes, Globals.ToSave.lst_docs_to_save[i].document_index_length, iindexpos); iindexpos += 4; //doc_full_index_length

                //document header info
                b_docs[idocpos] = 1; idocpos++; //active
                Globals._service.InsertBytes(ref b_docs, Globals.ToSave.lst_docs_to_save[i].document_id, idocpos); idocpos += 8; //doc_id
                Globals._service.InsertBytes(ref b_docs, Globals.ToSave.lst_docs_to_save[i].document_tag_hash, idocpos); idocpos += 8; //doc_tag_hash

                //get all attributes
                for (int j = 0; j < Globals.ToSave.lst_docs_to_save[i].lst_hash.Count; j++)
                {
                    //active (1) + data_type (1) + attrib_hash (8) + attrib_data_len (4) + attrib_data_pos (8)/attrib_data
                    b_indexes[iindexpos] = 1; iindexpos++; //active
                    b_indexes[iindexpos] = Globals.ToSave.lst_docs_to_save[i].lst_data_type[j]; iindexpos++; //data_type
                    Globals._service.InsertBytes(ref b_indexes, Globals.ToSave.lst_docs_to_save[i].document_tag_hash[j], iindexpos); iindexpos += 8; //attrib_hash
                    Globals._service.InsertBytes(ref b_indexes, BitConverter.GetBytes(Globals.ToSave.lst_docs_to_save[i].lst_data_len[j]), iindexpos); iindexpos += 4; //attrib_data_len
                    Globals._service.InsertBytes(ref b_indexes, Globals.ToSave.lst_docs_to_save[i].document_tag_hash[j], iindexpos); iindexpos += 8; //attrib_data_pos

                    //active (1) + data_type (1) + attrib_hash (8) + attrib_name (X) + attrib_data_len (4) + attrib_data (8)
                    b_docs[idocpos] = 1; idocpos++; //active
                    b_docs[idocpos] = Globals.ToSave.lst_docs_to_save[i].lst_data_type[j]; idocpos++; //data_type
                    Globals._service.InsertBytes(ref b_docs, Globals.ToSave.lst_docs_to_save[i].document_tag_hash[j], idocpos); idocpos += 8; //attrib_hash
                    Globals._service.InsertBytes(ref b_docs, Encoding.ASCII.GetBytes(Globals.ToSave.lst_docs_to_save[i].lst_name[j]), idocpos); idocpos += Globals.storage_max_attribute_name_length; //attrib_name
                    Globals._service.InsertBytes(ref b_docs, BitConverter.GetBytes(Globals.ToSave.lst_docs_to_save[i].lst_data_len[j]), idocpos); idocpos += 4; //attrib_data_len
                    Globals._service.InsertBytes(ref b_docs, Globals.ToSave.lst_docs_to_save[i].lst_data[j], idocpos); idocpos += Globals.ToSave.lst_docs_to_save[i].lst_data_len[j]; //attrib_data

                }//for - attributes
            }//for - documents

            //return
            return b_docs;
        }

        private string stemp = "";

        internal bool parse_query(string query)
        {
            bool bool_ret = false;

            string operators = "=<>!";
            int i = 0, ilen = query.Length, istart = 0;
            bool bool_text = false;

            char ch;

            //parse
            for (i = 0; i < ilen; i++)
            {
                ch = query[i];
                if (ch == '\'') //text surrounds with ' '
                {
                    if (bool_text == false)
                    {
                        if (i > istart) //save prev operator
                        {
                            stemp = query.Substring(istart, i - istart);
                            Globals.ToQuery.lst_operator.Add(get_operator(stemp[0]));
                        } 
                        bool_text = true; istart = i+1;
                    }
                    else //save value
                    {
                        stemp = query.Substring(istart, i - istart); //value
                        Globals.ToQuery.lst_value.Add(stemp);
                        //Globals.ToQuery.lst_operator.Add(ch.ToString()); //operator
                        bool_text = false; istart = i + 1;
                    }
                }
                else if (bool_text==false && operators.Contains(ch.ToString()))
                {
                    stemp = query.Substring(istart, i - istart);
                    Globals.ToQuery.lst_attribute.Add(get_attribute_hash(stemp)); //attribute
                    Globals.ToQuery.lst_operator.Add(get_operator(ch)); //operator
                    istart = i+1;
                }
                else if (bool_text == false)
                {
                    if (ch == ',' || ch == ' ' || ch==';')
                    {
                        if (i > istart) //save prev value
                        {
                            stemp = query.Substring(istart, i - istart);
                            Globals.ToQuery.lst_value.Add(stemp);
                        } 
                        istart = i+1;
                    }
                }
            }//for
            //add remains
            if (i > istart) //save prev value
            {
                stemp = query.Substring(istart, i - istart);
                Globals.ToQuery.lst_value.Add(stemp);
            } 

            return bool_ret;
        }


        private ulong get_attribute_hash(string sattribute)
        {
            return Globals._hash.CreateHash64bit(Encoding.ASCII.GetBytes(sattribute));
        }

        private byte get_operator(char soperator)
        {
            //"=<>!";
            switch (soperator)
            {
                case '=': return 1;
                case '<': return 2;
                case '>': return 3;
                case '!': return 4;
                default: return 0;
            }
        }

    }
}
