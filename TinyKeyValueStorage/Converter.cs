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
            int i = 0, i_index_pos = 0, i_docs_pos = 0, idocscount = Globals.ToSave.lst_docs_to_save.Count;

            byte[] b_docs = new byte[Globals.ToSave.i_docs_data_full_length];
            b_indexes = new byte[Globals.ToSave.i_docs_index_full_length];

            //go through all documents to save
            for (i = 0; i < idocscount; i++)
            {
                Globals._service.InsertBytes(ref b_indexes, Globals.ToSave.lst_index_to_save[i], i_index_pos);
                i_index_pos += Globals.ToSave.lst_index_to_save[i].Length;
                Globals._service.InsertBytes(ref b_docs, Globals.ToSave.lst_docs_to_save[i], i_docs_pos);
                i_docs_pos += Globals.ToSave.lst_docs_to_save[i].Length;
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

        internal bool check_value_for_query(ulong u_given_attribute, dynamic value)
        {
            bool bool_ret = false;



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
