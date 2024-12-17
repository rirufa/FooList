// See https://aka.ms/new-console-template for more information

using FooProject.Collection;

var buf = new FooProject.Collection.List<char>();
buf.AddRange("012345678\n");
buf.AddRange("abcdefghijklmn\n");
Console.WriteLine(String.Concat<char>(buf));
