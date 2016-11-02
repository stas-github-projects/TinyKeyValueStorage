using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace TinyKeyValueStorage_test
{
    class Program
    {
        static void Main(string[] args)
        {

            Stopwatch s = new Stopwatch();
            s.Start();

            TinyKeyValueStorage.TinyKeyValueStorage _storage = new TinyKeyValueStorage.TinyKeyValueStorage();


            _storage.open("test");

            for (int i = 0; i < 10000; i++)
            {
                TinyKeyValueStorage.TinyKeyValueStorage.Document _doc = new TinyKeyValueStorage.TinyKeyValueStorage.Document("goods_toys");
                _doc.Add("title","toy_fx");
                _doc.Add("model", i);
                _doc.Add("price", 123);
                _doc.Add("color", "black");
                _doc.Add("info", "Coolest toy in the history! Buy it!");
                _storage.set2(_doc);
            }

            //s.Start();
            //s.Stop();
            _storage.commit();            
            //_storage.get("hj");

            s.Stop();

            Console.WriteLine("\ntimings: {0} sec / {1} msec / {2} ticks", s.Elapsed.Seconds, s.ElapsedMilliseconds, s.ElapsedTicks);
            Console.ReadKey();
        }
    }

}
