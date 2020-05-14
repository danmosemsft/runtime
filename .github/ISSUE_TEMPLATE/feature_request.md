---
name: Performance issue
about: Report a performance problem or regression
title: ''
labels: 'tenet-performance'
assignees: ''

---
****
This is a template to help you open create a good issue for a performance problem. Everything under the headings is just a guide: please delete it and replace as appropriate.

**Description**
* Please share a clear and concise description of the performance problem. 
* Include minimal steps to reproduce the problem if possible. E.g.: the smallest possible code snippet; or a small repo to clone, with steps to run it.
* Which version of .NET is the code running on?
* What OS version, and what distro if applicable?
* What is the architecture (x64, x86, ARM, ARM64)?
* If relevant, what are the specs of the machine? If you are posting Benchmark.NET results, this info will be included.

**Regression?**
* Is this a regression, from a previous build, release of .NET Core, or from .NET Framework? Please describe.
* If you can try a previous release or build to find out, that can help us narrow down the problem. If you don't know, just let us know here.

**Data**
* Please include any benchmark results, images of graphs, timings or measurements, or callstacks that are relevant.
* If possible please include text as text rather than images (so it shows up in searches).
* If applicable please include before and after measurements.
* There is helpful information about measuring code in this repo [here](https://github.com/dotnet/performance/blob/master/docs/benchmarking-workflow-dotnet-runtime.md).

**Analysis**
* If you have an idea where the problem might lie, let us know that here.
* Please include any pointers to code, relevant changes, or related issues you know of.
* If you don't know, that's OK. Leave this blank.
