# Third-party notices

## XMemCompressionDotNet

The managed XMem LZX TD codec in
`PopStudio.Shared/YFTYLib/OtherProject/XMemCompressionDotNet` is vendored from
[PopGameTool/XMemCompressionDotNet](https://github.com/PopGameTool/XMemCompressionDotNet),
commit `edb9052b89bec72b7a1de51900a1274f0d0ede76`, with the owner's authorization.
The C++ reference is [PopGameTool/XMemCompression](https://github.com/PopGameTool/XMemCompression),
commit `3d369cbc6f7c3c0bf3e4bc881dfe58eeb72bb17e`.
Both are maintained by the same organization as PopStudio. The upstream repositories
do not contain a separate license file; no additional upstream license is asserted here.

Local changes enable nullable annotations in the shared project, resolve a .NET 9
array-copy overload, accelerate match comparison with bounded word reads, correct
the 64 KiB / 256 KiB TD segment-pitch header fields, and reject truncated or invalid
TD frames. No native DLL or NuGet package is required for this codec.

## Flash2Reanim

The XFL-to-REANIM decoder is derived from
[Flash2Reanim](https://github.com/Wanxiaace/Flash2Reanim).

MIT License

Copyright (c) 2026 Wanxi

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
