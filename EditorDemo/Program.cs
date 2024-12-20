// See https://aka.ms/new-console-template for more information

using FooEditEngine;
using FooProject.Collection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

Console.WriteLine("benchmark start");

Stopwatch sw = Stopwatch.StartNew();
var buf = new StringBuffer();
for(int i = 0; i< 100000; i++)
{
    var insertStr = "this is a pen.this is a pen.this is a pen.this is a pen.this is a pen.this is a pen.this is a pen.\n";
    buf.Replace(buf.Length, 0,insertStr,insertStr.Length);
}
sw.Stop();
Console.WriteLine(String.Format("add time:{0} ms", sw.ElapsedMilliseconds));

sw = Stopwatch.StartNew();
buf.ReplaceAll("pen", "cat");
sw.Stop();
Console.WriteLine(String.Format("replace time:{0} ms",sw.ElapsedMilliseconds));
sw = Stopwatch.StartNew();
buf.ReplaceAll("cat", "ratking");
sw.Stop();
Console.WriteLine(String.Format("replace time:{0} ms", sw.ElapsedMilliseconds));
