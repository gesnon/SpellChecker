using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using SpellChecker;
using System.Diagnostics;
#if DEBUG
    SpellCheck spellCheck = new SpellCheck();
    spellCheck.Check();
#else
    BenchmarkRunner.Run<SpellCheck>();
#endif


