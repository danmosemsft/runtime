# Implementation of System.Text.RegularExpressions

The implementation uses a typical NFA approach that supports back references. Patterns are parsed into a tree (`RegexTree`), translated into an abstract tree (`RegexCode`) by a writer (`RegexWriter`), and then either used in an interpreter (`RegexInterpreter`) or compiled to IL which is executed (`CompiledRegexRunner`). Both of these are instances of `RegexRunner`: in the case of the compiled runner, one must generate a new one from the `RegexCode` using a factory each time the pattern is to be executed.

Regex engines have different features: .NET regular expressions have a couple that others do not have, such as `Capture`s (distinct from `Group`s). It does not support searching UTF-8 text, nor searching a Span over a buffer.

Unlike some DFA based engines, patterns must be trusted. Text may be untrusted with the use of a timeout to prevent catastrophic backtracking.

## Extensibility

Key types have significant protected surface area. This is probably not intended as an extensibility point, but rather as a detail of implementing saving a compiled regex to disk. Saving to disk is implemented by saving an assembly containing three types, one that derives from each of `Regex`, `RegexRunnerFactory`, and `RegexRunner`. This mechanism accounts for all the protected methods (and even protected fields) on these classes. If we were designing them today, we would likely more carefully limit their public surface, and possibly not rely on derived types.

Protected members are part of the public API which cannot be broken, so they may potentially make some future optimizations more difficult. In particular, we must keep them stable in order to remain compatible with regexes saved by .NET Framework.

`RegexCompiler` is abstract for a different reason: to share implementation between `RegexLWCGCompiler` and `RegexAssemblyCompiler`: it based around a field of type `System.Reflection.Emit.ILGenerator` and has protected utility methods and fields. External extension by derivation of `RegexCompiler` would likely be clumsy as it contains knowledge of `RegexLWCGCompiler` and `RegexAssemblyCompiler`.


## Key types

#### Regex (public)

* Represents an executable regular expression with some utility static methods
* Several protected fields and methods but no derived classes exist in this implementation
* Constructor sets RegexCode using RegexParser and RegexWriter; then, if RegexOptions.Compiled, compiles and holds a RegexRunnerFactory and clears RegexCode; these steps only need to be done once for this Regex object
* Various public entry points converge on `Run()` which uses the held RegexRunner if any; if none or in use, creates another with the held RegexRunnerFactory if any; if none, interprets with held RegexCode
* All static methods (such as `Regex.Match`) attempt to find a pre-existing `Regex` object for the requested pattern and options in the `RegexCache`. This is legitimate, since after construction, `Regex` options are thread-safe. If there is a cache hit, execution can begin immediately.

#### RegexOptions (public)

#### MatchEvaluator (public)

#### RegexCompilationInfo (public)

* Parameters to use for regex compilation
* Passed in by app to Regex.CompileToAssembly(..)

## Parsing

#### RegexParser

* Converts pattern string to RegexTree of RegexNodes
* Invoked with `RegexTree Parse(string pattern, RegexOptions options...) {}`
* Also has Escape/Unescape methods, and parses into RegexReplacements
* Does a partial prescan to prep capture slots
* As each `RegexNode` is added, it attempts to reduce (optimize) the newly formed subtree. When parsing completes, there is a final optimization of the whole tree.

#### RegexReplacement

* Parsed replacement pattern
* Created by RegexParser, used in Regex.Replace/Match.Result(..)

#### RegexCharClass

* Representation of single, range, or class
* Created by RegexParser
* Creates packed string to be held on RegexNode
* Optimized for testing char vs class (`CharInClass(..)`)

#### RegexNode

* Node in regex parse tree
* Created by RegexParser
* Some nodes represent subsequent optimizations, rather than individual elements of the pattern
* Holds Children and Next
* Holds char or string (which may be char class), and M and N constants (eg loop bounds)
* Note: polymorphism is not in use here: the interpretation of its fields depends on the integer Type field

#### RegexTree

* Simple holder for root RegexNode, options, and captures datastructure
* Created by RegexParser

#### RegexWriter

* Responsible for translating RegexTree to RegexCode
* Invoked by Regex
* Creates itself `RegexCode Write(RegexTree tree){}`

#### RegexFCD

* Responsible for static pattern prefixes (FC=first chars)
* Created by RegexWriter
* Creates RegexFCs
* .FirstChars() creates RegexPrefix from RegexTree

#### RegexPrefix

* Literal string that match must begin with

#### RegexBoyerMoore

* BoyerMoore table
* Constructed by RegexWriter
* Singleton held on RegexCode
* RegexInterpreter uses it to perform BoyerMoore
* RegexCompiler generates code for BoyerMoore but uses its tables

### RegexCode

* Abstract representation of the "program" for a particular pattern
* Created by RegexWriter

## Compilation (if not interpreted)

### RegexCompiler (public abstract)

* Responsible for compiling `RegexCode` to a `RegexRunnerFactory`
* As implemented, uses `RegexLWCGCompiler`
* Ha utility method `CompileToAssembly` that invokes `RegexParser` and `RegexWriter` directly then uses `RegexAssemblyCompiler` (see note for that type)
* Key protected methods are GenerateFindFirstChar() and GenerateGo()
* Created and used only from `RegexRunnerFactory Regex.Compile(RegexCode code, RegexOptions options...)`
* Implements `RegexRunnerFactory RegexCompiler.Compile(RegexCode code, RegexOptions options...)`

### RegexLWCGCompiler (is a RegexCompiler)

* Creates a `CompiledRegexRunnerFactory` using `RegexRunnerFactory FactoryInstanceFromCode(RegexCode .. )`

### RegexRunnerFactory (public pure abstract)

* Makes RegexRunners with `RegexRunner CreateInstance()`
* Not relevant to interpreted mode
* Must be thread-safe, as each `Regex` holds one, and `Regex` is thread-safe

### CompiledRegexRunnerFactory (is a RegexRunnerFactory)

* Created by RegexLWCGCompiler
* Creates CompiledRegexRunner

### RegexAssemblyCompiler

* Created and used by `RegexCompiler.CompileToAssembly(...)` to write compiled regex to disk: at present, writing to disk is not implemented, because Reflection.Emit does not support it.

## Execution

### RegexRunner (public abstract)

* Lots of protected members: tracking position, execution stacks, and captures:
    * `protected abstract void Go()`
    * `protected abstract bool FindFirstChar()`
    * `public Match? Scan(System.Text.RegularExpressions.Regex regex, string text...)` calls FindFirstChar() and Go()
* Has a "quick" mode that does not instantiate any captures: used by `Regex.IsMatch`
* Concrete instances created only by `Match? Regex.Run(...)` calling either `RegexRunner CompiledRegexRunnerFactory.CreateInstance()` or newing up a RegexInterpreter
* Derived types:

### RegexInterpreter (is a RegexRunner)

### CompiledRegexRunner (is a RegexRunner)


## Results

### Match (public, is a Group)
* Represents one match of the pattern: there may be several
* Holds a Regex in order to call NextMatch()
* Created by RegexRunner
* Has Dump() for debugging
* Uses MatchSparse type as optimization

### Group (public, is a Capture)
* Represents one capturing group from the match
* Simple data holder

### Capture (public)
* Represents one of the potentially several captures from a capturing group; this is a .NET-only concept.
* Simple data holder

### MatchCollection (public)

### GroupCollection (public)
* Creates Groups

### CaptureCollection (public)
* Create Captures

### RegexParseException (is a ArgumentException)
* Thrown when pattern is invalid
* Contains RegexParseError

### RegexMatchTimeoutException (public)
* Thrown when timeout expires


# Optimizations

### RegexTree

* Every RegexNode.AddChild() calls Reduce() to attempt to optimize subtree as it is being assembled, and parsing ends with call to RegexNode.FinalOptimize() for some optimizations that require the entire tree. The goal is to make a functionally equivalent tree that can produce a more efficient program. With more detailed analysis of the tree and some creativity, more could be done here.

### RegexRunner

* If the pattern begins with a literal, `FindFirstChar()` is used to run quickly to the next point in the text that matches that literal, without using the engine. If the literal is a single character, this can use `IndexOf()` which is vectorized; otherwise it uses `RegexBoyerMoore`. Future optimizations could, for example, handle an alternation of leading literals using the Aho–Corasick algorithm; or use `IndexOf` to find a low-probability char before matching the whole literal. These optimizations are likely to most help in the case of a large text, perhaps with few matches, and a pattern with leading large literals.

// TODO - more here

# Tracing and dumping output

If the engine is built in debug configuration, and `RegexOptions.Debug` is passed, some internal datastructures will be written out with `Debug.Write()`. This includes the pattern itself, then `RegexWriter` will write out the input `RegexTree` with its nodes, and the output `RegexCode`. The `RegexBoyerMoore` dumps its tables - this would likely be relevant only if there was a bug in that class. `RegexRunner`s also dump their state as they execute the pattern. `Match` also has the ability to dump state.

For example, if you are working to optimize the `RegexTree` generated from a pattern, this can be a convenient way to visualize the tree without concerning yourself with the subsequent execution.

When you compile your test program, `RegexOptions.Debug` may not be visible to the compiler: you can use `(RegexOptions)0x0080` instead.

# Debugging

// TODO

# Profiling and benchmarks

// TODO

# Test strategy

// TODO
